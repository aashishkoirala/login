/*******************************************************************************************************************************
 * AK.Login.Tests.Integration.Presentation.LoginControllerTests
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

using AK.Commons;
using AK.Commons.Configuration;
using AK.Commons.Security;
using AK.Commons.Services;
using AK.Login.Application;
using AK.Login.Application.WsFed;
using AK.Login.Presentation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Web;
using System.Web.Mvc;

#endregion

namespace AK.Login.Tests.Integration.Presentation
{
    /// <summary>
    /// Minimal coverage sanity-check type integration tests for LoginController.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class LoginControllerTests
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            AppEnvironment.Initialize("AKLoginTests", new InitializationOptions
                {
                    ConfigStore = config.GetConfigStore(),
                    EnableLogging = true,
                    GenerateServiceClients = false
                });

            ServiceCallerFactory.ServiceEndpointAccessor = type =>
                {
                    if (!typeof (ILoginService).IsAssignableFrom(type))
                        throw new NotSupportedException();

                    return AppEnvironment.Composer.Resolve<ILoginServiceEndpointFactory>().Create();
                };

            var loginServiceMock = new Mock<ILoginService>();
            loginServiceMock.Setup(x => x.GetLoginSplashInfo()).Returns(new LoginSplashInfo());

            AppEnvironment.Composer.Resolve<IReplyToPartyFactory>().LoginServiceOverride =
                realm => loginServiceMock.Object;
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            AppEnvironment.ShutDown();
        }

        [TestMethod, TestCategory("Integration")]
        public void LoginController_Main_Unparsable_Request_Returns_Main_View()
        {
            var request = new HttpRequest(string.Empty, "http://www.test.com", string.Empty);
            var controller = new LoginController {RequestOverride = new HttpRequestWrapper(request)};

            var result = controller.Main();
            Assert.IsInstanceOfType(result, typeof (ViewResult));

            var viewResult = (ViewResult) result;
            Assert.IsTrue(viewResult.ViewName == string.Empty || viewResult.ViewName == "Main");
        }

        [TestMethod, TestCategory("Integration")]
        public void LoginController_Main_WsFed_Sign_In_Request_Not_Logged_In_Redirects_To_Login()
        {
            const string expectedInitialUrl = "http://www.test.com/?wa=wsignin1.0&wtrealm=Test";
            const string expectedRedirectUrl = "http://www.test.com/Login?wa=wsignin1.0&wtrealm=Test";
            var redirectUrl = string.Empty;

            var request = new HttpRequest(string.Empty, expectedInitialUrl, "wa=wsignin1.0&wtrealm=Test");

            var cookies = new HttpCookieCollection();
            var cacheMock = new Mock<HttpCachePolicyBase>();

            var responseMock = new Mock<HttpResponseBase>();
            responseMock.SetupGet(x => x.Cookies).Returns(cookies);
            responseMock.SetupGet(x => x.Cache).Returns(cacheMock.Object);
            responseMock
                .Setup(x => x.Redirect(It.IsAny<string>(), true))
                .Callback<string, bool>((url, endResponse) => redirectUrl = url);

            HttpContext.Current = new HttpContext(request, new HttpResponse(null));

            var controller = new LoginController
                {
                    RequestOverride = new HttpRequestWrapper(request),
                    ResponseOverride = responseMock.Object
                };

            var result = controller.Main();
            Assert.IsInstanceOfType(result, typeof (EmptyResult));
            Assert.AreEqual(expectedRedirectUrl, redirectUrl);

            var cookie = cookies[WsFedConstant.InitialUrlCookieName];
            Assert.IsNotNull(cookie);
            Assert.AreEqual(expectedInitialUrl, cookie.Value);
        }

        [TestMethod, TestCategory("Integration")]
        public void LoginController_Login_Returns_Login_View()
        {
            var request = new HttpRequest(
                string.Empty,
                "http://www.test.com/Login?wa=wsignin1.0&wtrealm=Test",
                "wa=wsignin1.0&wtrealm=Test");

            var queryString = new NameValueCollection();
            queryString["wa"] = "wsignin1.0";
            queryString["wtrealm"] = "Test";

            var cookies = new HttpCookieCollection();
            var cacheMock = new Mock<HttpCachePolicyBase>();
            var browserMock = new Mock<HttpBrowserCapabilitiesBase>();
            browserMock.SetupGet(x => x.Browser).Returns("Something");

            var requestMock = new Mock<HttpRequestBase>();
            requestMock.SetupGet(x => x.Url).Returns(new Uri("http://www.test.com/Login?wa=wsignin1.0&wtrealm=Test"));
            requestMock.SetupGet(x => x.Browser).Returns(browserMock.Object);
            requestMock.SetupGet(x => x.QueryString).Returns(queryString);

            var responseMock = new Mock<HttpResponseBase>();
            responseMock.SetupGet(x => x.Cookies).Returns(cookies);
            responseMock.SetupGet(x => x.Cache).Returns(cacheMock.Object);

            HttpContext.Current = new HttpContext(request, new HttpResponse(null));

            var controller = new LoginController
                {
                    RequestOverride = requestMock.Object,
                    ResponseOverride = responseMock.Object
                };

            var result = controller.Login();
            Assert.IsInstanceOfType(result, typeof (ViewResult));

            var viewResult = (ViewResult) result;
            Assert.IsTrue(viewResult.ViewName == string.Empty || viewResult.ViewName == "Login");
        }
    }
}