/*******************************************************************************************************************************
 * AK.Login.Tests.Unit.Presentation.LoginControllerTests
 * Copyright © 2014 Aashish Koirala <http://aashishkoirala.github.io>
 * 
 * This file is part of AK-Login.
 *  
 * AK-Login is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * AK-Login is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with AK-Login.  If not, see <http://www.gnu.org/licenses/>.
 * 
 *******************************************************************************************************************************/

#region Namespace Imports

using AK.Commons.Logging;
using AK.Commons.Security;
using AK.Login.Application;
using AK.Login.Application.Facebook;
using AK.Login.Application.Google;
using AK.Login.Presentation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

#endregion

namespace AK.Login.Tests.Unit.Presentation
{
    /// <summary>
    /// Unit tests for LoginController.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class LoginControllerTests
    {
        #region Main

        [TestMethod, TestCategory("Unit")]
        public void LoginController_Main_Parsable_Request_Works()
        {
            var requestHandlerMock = new Mock<ILoginRequestHandler>();
            requestHandlerMock
                .Setup(x => x.Parse(LoginStage.Initial, It.IsAny<HttpRequestBase>()))
                .Returns(new LoginRequestInfo {Parsed = true})
                .Verifiable();
            requestHandlerMock
                .Setup(x => x.Execute(It.Is<LoginRequestInfo>(y => y.Parsed), It.IsAny<HttpResponseBase>()))
                .Returns(new EmptyResult())
                .Verifiable();

            var containerMock = new Mock<IApplicationContainer>();
            containerMock
                .SetupGet(x => x.Logger)
                .Returns(new Mock<IAppLogger>().Object)
                .Verifiable();
            containerMock
                .Setup(x => x.RequestHandler)
                .Returns(requestHandlerMock.Object)
                .Verifiable();

            var loginController = new LoginController {ContainerOverride = containerMock.Object};
            var result = loginController.Main();

            Assert.IsInstanceOfType(result, typeof (EmptyResult));
            containerMock.Verify();
            requestHandlerMock.Verify();
        }

        [TestMethod, TestCategory("Unit")]
        public void LoginController_Main_Unparsable_Request_Works()
        {
            var requestHandlerMock = new Mock<ILoginRequestHandler>();
            requestHandlerMock
                .Setup(x => x.Parse(LoginStage.Initial, It.IsAny<HttpRequestBase>()))
                .Returns((LoginRequestInfo) null)
                .Verifiable();

            var containerMock = new Mock<IApplicationContainer>();
            containerMock
                .SetupGet(x => x.Logger)
                .Returns(new Mock<IAppLogger>().Object)
                .Verifiable();
            containerMock
                .SetupGet(x => x.RequestHandler)
                .Returns(requestHandlerMock.Object)
                .Verifiable();

            var loginController = new LoginController {ContainerOverride = containerMock.Object};
            var result = loginController.Main();

            Assert.IsInstanceOfType(result, typeof (ViewResult));

            var viewResult = (ViewResult) result;
            Assert.IsTrue(viewResult.ViewName == string.Empty || viewResult.ViewName == "Main");

            containerMock.Verify();
            requestHandlerMock.Verify();
        }

        #endregion

        #region Login

        [TestMethod, TestCategory("Unit")]
        public void LoginController_Login_Works()
        {
            TestLoginAction(true);
            TestLoginAction(false);
        }

        private static void TestLoginAction(bool isIe)
        {
            var expectedLoginSplashInfo = new LoginSplashInfo();

            var loginServiceMock = new Mock<ILoginService>();
            loginServiceMock
                .Setup(x => x.GetLoginSplashInfo())
                .Returns(expectedLoginSplashInfo)
                .Verifiable();

            var replyToPartyMock = new Mock<IReplyToParty>();
            replyToPartyMock
                .SetupGet(x => x.LoginService)
                .Returns(loginServiceMock.Object)
                .Verifiable();

            var expectedRequestInfo = new LoginRequestInfo {ReplyToParty = replyToPartyMock.Object};

            var requestHandlerMock = new Mock<ILoginRequestHandler>();
            requestHandlerMock
                .Setup(x => x.Parse(LoginStage.ShowLoginPage, It.IsAny<HttpRequestBase>()))
                .Returns(expectedRequestInfo)
                .Verifiable();
            requestHandlerMock
                .Setup(x => x.Execute(expectedRequestInfo, It.IsAny<HttpResponseBase>()))
                .Verifiable();

            var containerMock = new Mock<IApplicationContainer>();
            containerMock
                .SetupGet(x => x.Logger)
                .Returns(new Mock<IAppLogger>().Object)
                .Verifiable();
            containerMock
                .Setup(x => x.RequestHandler)
                .Returns(requestHandlerMock.Object)
                .Verifiable();

            var browserMock = new Mock<HttpBrowserCapabilitiesBase>();
            browserMock
                .SetupGet(x => x.Browser)
                .Returns(isIe ? "IE" : "Not IE")
                .Verifiable();

            var requestMock = new Mock<HttpRequestBase>();
            requestMock
                .SetupGet(x => x.Url)
                .Returns(new Uri("http://www.test.com"))
                .Verifiable();
            requestMock
                .SetupGet(x => x.Browser)
                .Returns(browserMock.Object)
                .Verifiable();

            var loginController = new LoginController
                {
                    ContainerOverride = containerMock.Object,
                    RequestOverride = requestMock.Object
                };

            var result = loginController.Login();
            Assert.IsInstanceOfType(result, typeof (ViewResult));

            var viewResult = (ViewResult) result;
            Assert.IsInstanceOfType(viewResult.Model, typeof (LoginViewModel));

            var model = (LoginViewModel) viewResult.Model;

            Assert.AreSame(expectedLoginSplashInfo, model.Splash);
            Assert.IsTrue(isIe ? model.ShowIeWarning : !model.ShowIeWarning);
            Assert.AreEqual("www.test.com", model.Domain);

            loginServiceMock.Verify();
            replyToPartyMock.Verify();
            requestHandlerMock.Verify();
            containerMock.Verify();
            browserMock.Verify();
            requestMock.Verify();
        }

        #endregion

        #region Facebook

        [TestMethod, TestCategory("Unit")]
        public void LoginController_Facebook_Logged_In_Redirects_To_Authenticated()
        {
            var expectedLoginUserInfo = new LoginUserInfo();

            var facebookAuthenticatorMock = new Mock<IFacebookAuthenticator>();
            facebookAuthenticatorMock
                .Setup(x => x.Login(It.IsAny<FacebookLoginRequest>()))
                .Returns(new FacebookLoginResult {IsLoggedIn = true, LoginUserInfo = expectedLoginUserInfo})
                .Verifiable();

            var userInfoCookieManagerMock = new Mock<ILoginUserInfoCookieManager>();
            userInfoCookieManagerMock
                .Setup(x => x.Write(expectedLoginUserInfo, It.IsAny<HttpResponseBase>()))
                .Verifiable();

            var containerMock = new Mock<IApplicationContainer>();
            containerMock.SetupGet(x => x.Logger).Returns(new Mock<IAppLogger>().Object).Verifiable();
            containerMock.SetupGet(x => x.FacebookAuthenticator).Returns(facebookAuthenticatorMock.Object).Verifiable();
            containerMock.SetupGet(x => x.UserInfoCookieManager).Returns(userInfoCookieManagerMock.Object).Verifiable();

            var loginController = new LoginController {ContainerOverride = containerMock.Object};

            var result = loginController.Facebook(new FacebookLoginRequest());
            Assert.IsInstanceOfType(result, typeof (RedirectToRouteResult));

            var redirectToRouteResult = (RedirectToRouteResult) result;
            Assert.AreEqual("Authenticated", redirectToRouteResult.RouteValues["action"]);

            facebookAuthenticatorMock.Verify();
            userInfoCookieManagerMock.Verify();
            containerMock.Verify();
        }

        [TestMethod, TestCategory("Unit")]
        public void LoginController_Facebook_Not_Logged_In_Returns_Unauthorized()
        {
            var facebookAuthenticatorMock = new Mock<IFacebookAuthenticator>();
            facebookAuthenticatorMock
                .Setup(x => x.Login(It.IsAny<FacebookLoginRequest>()))
                .Returns(new FacebookLoginResult {IsLoggedIn = false})
                .Verifiable();

            var containerMock = new Mock<IApplicationContainer>();
            containerMock.SetupGet(x => x.Logger).Returns(new Mock<IAppLogger>().Object).Verifiable();
            containerMock.SetupGet(x => x.FacebookAuthenticator).Returns(facebookAuthenticatorMock.Object).Verifiable();

            var loginController = new LoginController {ContainerOverride = containerMock.Object};

            var result = loginController.Facebook(new FacebookLoginRequest());
            Assert.IsInstanceOfType(result, typeof (HttpUnauthorizedResult));

            facebookAuthenticatorMock.Verify();
            containerMock.Verify();
        }

        #endregion

        #region Google

        [TestMethod, TestCategory("Unit")]
        public void LoginController_Google_Logged_In_Redirects_To_Authenticated()
        {
            var expectedLoginUserInfo = new LoginUserInfo();

            var googleAuthenticatorMock = new Mock<IGoogleAuthenticator>();
            googleAuthenticatorMock
                .Setup(x => x.Login(It.IsAny<LoginController>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Factory.StartNew(
                    () => new GoogleLoginResult {IsLoggedIn = true, LoginUserInfo = expectedLoginUserInfo}))
                .Verifiable();

            var userInfoCookieManagerMock = new Mock<ILoginUserInfoCookieManager>();
            userInfoCookieManagerMock
                .Setup(x => x.Write(expectedLoginUserInfo, It.IsAny<HttpResponseBase>()))
                .Verifiable();

            var containerMock = new Mock<IApplicationContainer>();
            containerMock.SetupGet(x => x.Logger).Returns(new Mock<IAppLogger>().Object).Verifiable();
            containerMock.SetupGet(x => x.GoogleAuthenticator).Returns(googleAuthenticatorMock.Object).Verifiable();
            containerMock.SetupGet(x => x.UserInfoCookieManager).Returns(userInfoCookieManagerMock.Object).Verifiable();

            var loginController = new LoginController
                {
                    ContainerOverride = containerMock.Object,
                    GoogleAuthCallbackOverride = string.Empty
                };

            var resultTask = loginController.Google(new CancellationToken());
            var result = resultTask.Result;

            Assert.IsInstanceOfType(result, typeof (RedirectToRouteResult));

            var redirectToRouteResult = (RedirectToRouteResult) result;
            Assert.AreEqual("Authenticated", redirectToRouteResult.RouteValues["action"]);

            googleAuthenticatorMock.Verify();
            userInfoCookieManagerMock.Verify();
            containerMock.Verify();
        }

        [TestMethod, TestCategory("Unit")]
        public void LoginController_Google_Not_Logged_In_Redirects_To_Challenge()
        {
            const string expectedRedirectUrl = "http://www.google.com";

            var googleAuthenticatorMock = new Mock<IGoogleAuthenticator>();
            googleAuthenticatorMock
                .Setup(x => x.Login(It.IsAny<LoginController>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Factory.StartNew(
                    () => new GoogleLoginResult {IsLoggedIn = false, RedirectUrl = expectedRedirectUrl}))
                .Verifiable();

            var containerMock = new Mock<IApplicationContainer>();
            containerMock.SetupGet(x => x.Logger).Returns(new Mock<IAppLogger>().Object).Verifiable();
            containerMock.SetupGet(x => x.GoogleAuthenticator).Returns(googleAuthenticatorMock.Object).Verifiable();

            var loginController = new LoginController
                {
                    ContainerOverride = containerMock.Object,
                    GoogleAuthCallbackOverride = string.Empty
                };

            var resultTask = loginController.Google(new CancellationToken());

            var result = resultTask.Result;
            Assert.IsInstanceOfType(result, typeof (RedirectResult));

            var redirectResult = (RedirectResult) result;
            Assert.AreEqual(expectedRedirectUrl, redirectResult.Url);

            containerMock.Verify();
            googleAuthenticatorMock.Verify();
        }

        #endregion

        #region Authenticated

        [TestMethod, TestCategory("Unit")]
        public void LoginController_Authenticated_User_Found_Writes_Cookie()
        {
            const string userName = "UserName";

            var expectedUserInfo = new LoginUserInfo
                {
                    UserExists = true,
                    UserName = userName,
                    UserId = "1234",
                    DisplayName = "Test Display Name"
                };

            // ReSharper disable ImplicitlyCapturedClosure

            var loginServiceMock = new Mock<ILoginService>();
            loginServiceMock.Setup(x => x.GetUser(userName)).Returns(expectedUserInfo).Verifiable();

            // ReSharper restore ImplicitlyCapturedClosure

            var replyToPartyMock = new Mock<IReplyToParty>();
            replyToPartyMock.SetupGet(x => x.LoginService).Returns(loginServiceMock.Object).Verifiable();

            var requestInfo = new LoginRequestInfo {ReplyToParty = replyToPartyMock.Object};

            var requestHandlerMock = new Mock<ILoginRequestHandler>();
            requestHandlerMock
                .Setup(x => x.Parse(LoginStage.Authenticated, It.IsAny<HttpRequestBase>()))
                .Returns(requestInfo)
                .Verifiable();
            requestHandlerMock
                .Setup(x => x.Execute(requestInfo, It.IsAny<HttpResponseBase>()))
                .Returns(new EmptyResult())
                .Verifiable();

            var userInfoCookieManagerMock = new Mock<ILoginUserInfoCookieManager>();
            userInfoCookieManagerMock
                .Setup(x => x.Read(It.IsAny<HttpRequestBase>()))
                .Returns(expectedUserInfo)
                .Verifiable();
            userInfoCookieManagerMock.Setup(x => x.Clear(It.IsAny<HttpResponseBase>())).Verifiable();

            var sessionTokenCookieWriterMock = new Mock<ISessionTokenCookieWriter>();
            sessionTokenCookieWriterMock
                .Setup(x => x.Write(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((
                    userId, userNameParam, displayName) =>
                    {
                        Assert.AreEqual(expectedUserInfo.UserId, userId);
                        Assert.AreEqual(userName, userNameParam);
                        Assert.AreEqual(expectedUserInfo.DisplayName, displayName);
                    })
                .Verifiable();

            var containerMock = new Mock<IApplicationContainer>();
            containerMock.SetupGet(x => x.Logger).Returns(new Mock<IAppLogger>().Object).Verifiable();
            containerMock.SetupGet(x => x.RequestHandler).Returns(requestHandlerMock.Object).Verifiable();
            containerMock.SetupGet(x => x.UserInfoCookieManager).Returns(userInfoCookieManagerMock.Object).Verifiable();
            containerMock.SetupGet(x => x.SessionTokenCookieWriter)
                         .Returns(sessionTokenCookieWriterMock.Object)
                         .Verifiable();

            var loginController = new LoginController {ContainerOverride = containerMock.Object};

            var result = loginController.Authenticated();
            Assert.IsInstanceOfType(result, typeof (EmptyResult));

            loginServiceMock.Verify();
            replyToPartyMock.Verify();
            requestHandlerMock.Verify();
            userInfoCookieManagerMock.Verify();
            sessionTokenCookieWriterMock.Verify();
            containerMock.Verify();
        }

        [TestMethod, TestCategory("Unit")]
        public void LoginController_Authenticated_User_Not_Found_Intercepts_For_Details()
        {
            const string userName = "UserName";

            var userInfo = new LoginUserInfo {UserExists = false, UserName = userName};
            var splashInfo = new LoginSplashInfo();

            var loginServiceMock = new Mock<ILoginService>();
            loginServiceMock.Setup(x => x.GetUser(userName)).Returns(userInfo).Verifiable();
            loginServiceMock.Setup(x => x.GetLoginSplashInfo()).Returns(splashInfo).Verifiable();

            var replyToPartyMock = new Mock<IReplyToParty>();
            replyToPartyMock.SetupGet(x => x.LoginService).Returns(loginServiceMock.Object).Verifiable();

            var requestInfo = new LoginRequestInfo {ReplyToParty = replyToPartyMock.Object};

            var requestHandlerMock = new Mock<ILoginRequestHandler>();
            requestHandlerMock
                .Setup(x => x.Parse(LoginStage.Authenticated, It.IsAny<HttpRequestBase>()))
                .Returns(requestInfo)
                .Verifiable();

            var userInfoCookieManagerMock = new Mock<ILoginUserInfoCookieManager>();
            userInfoCookieManagerMock
                .Setup(x => x.Read(It.IsAny<HttpRequestBase>()))
                .Returns(userInfo)
                .Verifiable();

            var containerMock = new Mock<IApplicationContainer>();
            containerMock.SetupGet(x => x.Logger).Returns(new Mock<IAppLogger>().Object).Verifiable();
            containerMock.SetupGet(x => x.RequestHandler).Returns(requestHandlerMock.Object).Verifiable();
            containerMock.SetupGet(x => x.UserInfoCookieManager).Returns(userInfoCookieManagerMock.Object).Verifiable();

            var loginController = new LoginController
                {
                    ContainerOverride = containerMock.Object,
                    RequestOverride =
                        new HttpRequestWrapper(new HttpRequest(string.Empty, "http://www.test.com", string.Empty))
                };

            var result = loginController.Authenticated();
            Assert.IsInstanceOfType(result, typeof (ViewResult));

            var viewResult = (ViewResult) result;
            Assert.AreEqual("UserDetails", viewResult.ViewName);
            Assert.IsInstanceOfType(viewResult.Model, typeof (LoginViewModel));

            var model = (LoginViewModel) viewResult.Model;
            Assert.AreSame(splashInfo, model.Splash);
            Assert.AreEqual(userInfo, model.User);

            loginServiceMock.Verify();
            replyToPartyMock.Verify();
            requestHandlerMock.Verify();
            userInfoCookieManagerMock.Verify();
            containerMock.Verify();
        }

        [TestMethod, TestCategory("Unit")]
        public void LoginController_Authenticated_No_User_Name_Returns_Unauthorized()
        {
            var requestHandlerMock = new Mock<ILoginRequestHandler>();
            requestHandlerMock
                .Setup(x => x.Parse(LoginStage.Authenticated, It.IsAny<HttpRequestBase>()))
                .Returns(new LoginRequestInfo())
                .Verifiable();

            var userInfoCookieManagerMock = new Mock<ILoginUserInfoCookieManager>();
            userInfoCookieManagerMock
                .Setup(x => x.Read(It.IsAny<HttpRequestBase>()))
                .Returns(new LoginUserInfo {UserName = string.Empty})
                .Verifiable();

            var containerMock = new Mock<IApplicationContainer>();
            containerMock.SetupGet(x => x.Logger).Returns(new Mock<IAppLogger>().Object).Verifiable();
            containerMock.SetupGet(x => x.RequestHandler).Returns(requestHandlerMock.Object).Verifiable();
            containerMock.SetupGet(x => x.UserInfoCookieManager).Returns(userInfoCookieManagerMock.Object).Verifiable();

            var loginController = new LoginController {ContainerOverride = containerMock.Object};

            var result = loginController.Authenticated();
            Assert.IsInstanceOfType(result, typeof (HttpUnauthorizedResult));

            requestHandlerMock.Verify();
            userInfoCookieManagerMock.Verify();
            containerMock.Verify();
        }

        #endregion

        #region UserDetails

        [TestMethod, TestCategory("Unit")]
        public void LoginController_UserDetails_Valid_Writes_Cookie()
        {
            const string userName = "UserName";
            const string expectedDisplayName = "New display name";

            var expectedUserInfo = new LoginUserInfo
                {
                    UserExists = true,
                    UserName = userName,
                    UserId = "1234",
                    DisplayName = "Test Display Name"
                };

            // ReSharper disable ImplicitlyCapturedClosure

            var loginServiceMock = new Mock<ILoginService>();
            loginServiceMock.Setup(x => x.CreateUser(expectedUserInfo)).Returns(expectedUserInfo).Verifiable();

            // ReSharper restore ImplicitlyCapturedClosure

            var replyToPartyMock = new Mock<IReplyToParty>();
            replyToPartyMock.SetupGet(x => x.LoginService).Returns(loginServiceMock.Object).Verifiable();

            var requestInfo = new LoginRequestInfo {ReplyToParty = replyToPartyMock.Object};

            var requestHandlerMock = new Mock<ILoginRequestHandler>();
            requestHandlerMock
                .Setup(x => x.Parse(LoginStage.Authenticated, It.IsAny<HttpRequestBase>()))
                .Returns(requestInfo)
                .Verifiable();
            requestHandlerMock
                .Setup(x => x.Execute(requestInfo, It.IsAny<HttpResponseBase>()))
                .Returns(new EmptyResult())
                .Verifiable();

            var userInfoCookieManagerMock = new Mock<ILoginUserInfoCookieManager>();
            userInfoCookieManagerMock
                .Setup(x => x.Read(It.IsAny<HttpRequestBase>()))
                .Returns(expectedUserInfo)
                .Verifiable();
            userInfoCookieManagerMock.Setup(x => x.Clear(It.IsAny<HttpResponseBase>())).Verifiable();

            var sessionTokenCookieWriterMock = new Mock<ISessionTokenCookieWriter>();
            sessionTokenCookieWriterMock
                .Setup(x => x.Write(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((
                    userId, userNameParam, displayName) =>
                    {
                        Assert.AreEqual(expectedUserInfo.UserId, userId);
                        Assert.AreEqual(userName, userNameParam);
                        Assert.AreEqual(expectedDisplayName, displayName);
                    })
                .Verifiable();

            var containerMock = new Mock<IApplicationContainer>();
            containerMock.SetupGet(x => x.Logger).Returns(new Mock<IAppLogger>().Object).Verifiable();
            containerMock.SetupGet(x => x.RequestHandler).Returns(requestHandlerMock.Object).Verifiable();
            containerMock.SetupGet(x => x.UserInfoCookieManager).Returns(userInfoCookieManagerMock.Object).Verifiable();
            containerMock.SetupGet(x => x.SessionTokenCookieWriter)
                         .Returns(sessionTokenCookieWriterMock.Object)
                         .Verifiable();

            var loginController = new LoginController {ContainerOverride = containerMock.Object};

            var result =
                loginController.UserDetails(new LoginViewModel
                    {
                        User = new LoginUserInfo {DisplayName = expectedDisplayName}
                    });
            Assert.IsInstanceOfType(result, typeof (EmptyResult));

            loginServiceMock.Verify();
            replyToPartyMock.Verify();
            requestHandlerMock.Verify();
            userInfoCookieManagerMock.Verify();
            sessionTokenCookieWriterMock.Verify();
            containerMock.Verify();
        }

        [TestMethod, TestCategory("Unit")]
        public void LoginController_UserDetails_Invalid_Kicked_Out()
        {
            var containerMock = new Mock<IApplicationContainer>();
            containerMock.SetupGet(x => x.Logger).Returns(new Mock<IAppLogger>().Object).Verifiable();

            var loginController = new LoginController {ContainerOverride = containerMock.Object};
            var expectedViewModel = new LoginViewModel {User = new LoginUserInfo {DisplayName = string.Empty}};

            var result = loginController.UserDetails(expectedViewModel);
            Assert.IsInstanceOfType(result, typeof (ViewResult));

            var viewResult = (ViewResult) result;
            Assert.IsInstanceOfType(viewResult.Model, typeof (LoginViewModel));

            var model = (LoginViewModel) viewResult.Model;
            Assert.AreSame(expectedViewModel, model);
            Assert.AreEqual("Must enter something!", model.User.DisplayName);

            containerMock.Verify();
        }

        #endregion
    }
}