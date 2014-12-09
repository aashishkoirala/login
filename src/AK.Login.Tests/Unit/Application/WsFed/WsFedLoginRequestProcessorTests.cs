/*******************************************************************************************************************************
 * AK.Login.Tests.Unit.Application.WsFed.WsFedLoginRequestProcessorTests
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

using AK.Commons.Services;
using AK.Login.Application;
using AK.Login.Application.WsFed;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IdentityModel;
using System.IdentityModel.Configuration;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Web;

#endregion

namespace AK.Login.Tests.Unit.Application.WsFed
{
    /// <summary>
    /// Unit tests for WsFedLoginRequestProcessor.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class WsFedLoginRequestProcessorTests
    {
        [TestMethod, TestCategory("Unit")]
        public void WsFedLoginRequestProcessor_Process_Sign_In_Initial_Not_Logged_In_Works()
        {
            const string baseUrl = "http://www.test.com/";
            const string loginPath = "TestLoginPath";
            const string realm = "TestRealm";

            var expectedRedirectUrl = string.Format("{0}{1}?wa=wsignin1.0&wtrealm={2}",
                                                    baseUrl, loginPath, realm);

            var request = new HttpRequest(string.Empty, baseUrl, string.Empty);

            var requestInfo = new LoginRequestInfo
                {
                    Parsed = true,
                    Type = LoginRequestType.SignIn,
                    Stage = LoginStage.Initial,
                    Protocol = ProtocolName.WsFed,
                    Message = new SignInRequestMessage(new Uri(baseUrl), realm),
                    Request = new HttpRequestWrapper(request)
                };

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.SetupGet(x => x.BaseUrl).Returns(baseUrl).Verifiable();
            configurationMock.SetupGet(x => x.LoginPath).Returns(loginPath).Verifiable();

            var sessionTokenCookieManagerMock = new Mock<ISessionTokenCookieManager>();
            sessionTokenCookieManagerMock
                .Setup(x => x.ReadSessionTokenCookie())
                .Returns(new OperationResult<SessionSecurityToken>(LoginErrorCodes.CannotReadSessionTokenCookie))
                .Verifiable();

            var cacheMock = new Mock<HttpCachePolicyBase>();
            string redirectUrl = null;
            var responseMock = new Mock<HttpResponseBase>();
            responseMock.Setup(x => x.Redirect(It.IsAny<string>(), true))
                        .Callback<string, bool>((url, endResponse) => redirectUrl = url)
                        .Verifiable();
            responseMock.SetupGet(x => x.Cache)
                        .Returns(cacheMock.Object)
                        .Verifiable();

            var response = responseMock.Object;
            HttpResponseBase responseCookieWrittenTo = null;
            string cookieUrl = null;

            var wsFedInitialUrlCookieManagerMock = new Mock<IWsFedInitialUrlCookieManager>();
            wsFedInitialUrlCookieManagerMock
                .Setup(x => x.Write(baseUrl, response))
                .Callback<string, HttpResponseBase>((
                    urlParam, responseParam) =>
                    {
                        responseCookieWrittenTo = responseParam;
                        cookieUrl = urlParam;
                    })
                .Verifiable();

            var loginRequestProcessor = new WsFedLoginRequestProcessor(
                sessionTokenCookieManagerMock.Object,
                new Mock<ISecurityTokenServiceFactory>().Object,
                wsFedInitialUrlCookieManagerMock.Object,
                configurationMock.Object);

            loginRequestProcessor.Process(requestInfo, response);

            Assert.AreEqual(baseUrl, cookieUrl);
            Assert.AreSame(response, responseCookieWrittenTo);
            Assert.AreEqual(expectedRedirectUrl, redirectUrl);

            responseMock.Verify();
            configurationMock.Verify();
            sessionTokenCookieManagerMock.Verify();
            wsFedInitialUrlCookieManagerMock.Verify();
        }

        [TestMethod, TestCategory("Unit")]
        public void WsFedLoginRequestProcessor_Process_Sign_In_Initial_Logged_In_Works()
        {
            const string replyToAddress = "http://replyto.com/";
            const string baseUrl = "http://www.test.com/";
            const string realm = "urn:TestRealm";

            const string expectedResult =
                "&lt;trust:RequestSecurityTokenResponseCollection xmlns:trust=&quot;" +
                "http://docs.oasis-open.org/ws-sx/ws-trust/200512&quot;>&lt;trust:" +
                "RequestSecurityTokenResponse>&lt;trust:Lifetime";

            var expectedSnippets = new[]
                {
                    string.Format("<form method=\"POST\" name=\"hiddenform\" action=\"{0}\">", replyToAddress),
                    "<input type=\"hidden\" name=\"wa\" value=\"wsignin1.0\" />",
                    string.Format("<input type=\"hidden\" name=\"wresult\" value=\"{0}", expectedResult)
                };

            var replyToPartyMock = new Mock<IReplyToParty>();
            replyToPartyMock.SetupGet(x => x.ReplyToAddress).Returns(replyToAddress).Verifiable();

            var requestInfo = new LoginRequestInfo
                {
                    Parsed = true,
                    Protocol = ProtocolName.WsFed,
                    Type = LoginRequestType.SignIn,
                    Stage = LoginStage.Initial,
                    ReplyToParty = replyToPartyMock.Object,
                    Message = new SignInRequestMessage(new Uri(baseUrl), realm)
                };

            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.Sid, "Test"));

            var principal = new ClaimsPrincipal(identity);
            var token = new SessionSecurityToken(principal);

            var sessionTokenCookieManagerMock = new Mock<ISessionTokenCookieManager>();
            sessionTokenCookieManagerMock
                .Setup(x => x.ReadSessionTokenCookie())
                .Returns(new OperationResult<SessionSecurityToken>(token))
                .Verifiable();

            var securityTokenServiceFactoryMock = new Mock<ISecurityTokenServiceFactory>();
            securityTokenServiceFactoryMock
                .Setup(x => x.Create(replyToAddress, replyToAddress))
                .Returns(new TestSecurityTokenService(replyToAddress))
                .Verifiable();

            var cacheMock = new Mock<HttpCachePolicyBase>();
            var responseMock = new Mock<HttpResponseBase>();

            responseMock
                .Setup(x => x.Write(It.IsAny<string>()))
                .Callback<string>(value =>
                    {
                        foreach (var expectedSnippet in expectedSnippets)
                            Assert.IsTrue(value.Contains(expectedSnippet));
                    })
                .Verifiable();
            responseMock.SetupGet(x => x.Cache)
                        .Returns(cacheMock.Object)
                        .Verifiable();

            var loginRequestProcessor = new WsFedLoginRequestProcessor(
                sessionTokenCookieManagerMock.Object,
                securityTokenServiceFactoryMock.Object,
                new Mock<IWsFedInitialUrlCookieManager>().Object,
                new Mock<IConfiguration>().Object);

            loginRequestProcessor.Process(requestInfo, responseMock.Object);

            cacheMock.Verify();
            replyToPartyMock.Verify();
            sessionTokenCookieManagerMock.Verify();
            securityTokenServiceFactoryMock.Verify();
            responseMock.Verify();
        }

        [TestMethod, TestCategory("Unit")]
        public void WsFedLoginRequestProcessor_Process_Sign_In_Authenticated_Works()
        {
            const string expectedUrl = "http://www.test.com/";
            const string realm = "TestRealm";

            var request = new HttpRequest(string.Empty, expectedUrl, string.Empty);

            var requestInfo = new LoginRequestInfo
                {
                    Parsed = true,
                    Type = LoginRequestType.SignIn,
                    Protocol = ProtocolName.WsFed,
                    Stage = LoginStage.Authenticated,
                    Message = new SignInRequestMessage(new Uri(expectedUrl), realm),
                    Request = new HttpRequestWrapper(request)
                };

            var cacheMock = new Mock<HttpCachePolicyBase>();
            var responseMock = new Mock<HttpResponseBase>();
            responseMock.Setup(x => x.Redirect(It.IsAny<string>()))
                        .Callback<string>(url => Assert.AreEqual(expectedUrl, url))
                        .Verifiable();
            responseMock.SetupGet(x => x.Cache)
                        .Returns(cacheMock.Object)
                        .Verifiable();

            var wsFedInitialUrlCookieManagerMock = new Mock<IWsFedInitialUrlCookieManager>();
            wsFedInitialUrlCookieManagerMock
                .Setup(x => x.Read(requestInfo.Request))
                .Returns(expectedUrl)
                .Verifiable();
            wsFedInitialUrlCookieManagerMock
                .Setup(x => x.Clear(responseMock.Object))
                .Verifiable();

            var sessionTokenCookieManagerMock = new Mock<ISessionTokenCookieManager>();
            sessionTokenCookieManagerMock
                .Setup(x => x.ReadSessionTokenCookie())
                .Returns(new OperationResult<SessionSecurityToken>(LoginErrorCodes.CannotReadSessionTokenCookie))
                .Verifiable();

            var loginRequestProecssor = new WsFedLoginRequestProcessor(
                sessionTokenCookieManagerMock.Object,
                new Mock<ISecurityTokenServiceFactory>().Object,
                wsFedInitialUrlCookieManagerMock.Object,
                new Mock<IConfiguration>().Object);

            loginRequestProecssor.Process(requestInfo, responseMock.Object);

            cacheMock.Verify();
            responseMock.Verify();
            wsFedInitialUrlCookieManagerMock.Verify();
            sessionTokenCookieManagerMock.Verify();
        }

        [TestMethod, TestCategory("Unit")]
        public void WsFedLoginRequestProcessor_Process_Sign_In_Invalid_Stage_Nothing_Happens()
        {
            const string baseUrl = "http://www.test.com/";
            const string realm = "TestRealm";

            var request = new HttpRequest(string.Empty, baseUrl, string.Empty);

            var requestInfo = new LoginRequestInfo
                {
                    Parsed = true,
                    Type = LoginRequestType.SignIn,
                    Protocol = ProtocolName.WsFed,
                    Stage = LoginStage.ShowLoginPage,
                    Message = new SignInRequestMessage(new Uri(baseUrl), realm),
                    Request = new HttpRequestWrapper(request)
                };

            var responseMock = new Mock<HttpResponseBase>();
            responseMock.Setup(x => x.Redirect(It.IsAny<string>()))
                        .Callback<string>(url => Assert.AreEqual(baseUrl, url))
                        .Verifiable();

            var sessionTokenCookieManagerMock = new Mock<ISessionTokenCookieManager>();
            sessionTokenCookieManagerMock
                .Setup(x => x.ReadSessionTokenCookie())
                .Returns(new OperationResult<SessionSecurityToken>(LoginErrorCodes.CannotReadSessionTokenCookie))
                .Verifiable();

            var loginRequestProecssor = new WsFedLoginRequestProcessor(
                sessionTokenCookieManagerMock.Object,
                new Mock<ISecurityTokenServiceFactory>().Object,
                new Mock<IWsFedInitialUrlCookieManager>().Object,
                new Mock<IConfiguration>().Object);

            loginRequestProecssor.Process(requestInfo, responseMock.Object);

            responseMock.VerifyGet(x => x.Cache, Times.Never);
            responseMock.Verify(x => x.Write(It.IsAny<string>()), Times.Never);
            responseMock.Verify(x => x.Redirect(It.IsAny<string>()), Times.Never);
            responseMock.Verify(x => x.Redirect(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [TestMethod, TestCategory("Unit")]
        public void WsFedLoginRequestProcessor_Process_Sign_Out_Works()
        {
            const string baseUrl = "http://www.test.com/";
            const string replyToAddress = "http://www.replyto.com/";

            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.Sid, "Test"));

            var principal = new ClaimsPrincipal(identity);
            var token = new SessionSecurityToken(principal);

            var sessionTokenCookieManagerMock = new Mock<ISessionTokenCookieManager>();
            sessionTokenCookieManagerMock
                .Setup(x => x.DeleteSessionTokenCookie())
                .Verifiable();
            sessionTokenCookieManagerMock
                .Setup(x => x.ReadSessionTokenCookie())
                .Returns(new OperationResult<SessionSecurityToken>(token))
                .Verifiable();

            var replyToPartyMock = new Mock<IReplyToParty>();
            replyToPartyMock.SetupGet(x => x.ReplyToAddress).Returns(replyToAddress).Verifiable();

            var requestInfo = new LoginRequestInfo
                {
                    Parsed = true,
                    Type = LoginRequestType.SignOut,
                    Protocol = ProtocolName.WsFed,
                    Message = new SignOutRequestMessage(new Uri(baseUrl), replyToAddress),
                    ReplyToParty = replyToPartyMock.Object
                };

            var wsFedLoginRequestProcessor = new WsFedLoginRequestProcessor(
                sessionTokenCookieManagerMock.Object,
                new Mock<ISecurityTokenServiceFactory>().Object,
                new Mock<IWsFedInitialUrlCookieManager>().Object,
                new Mock<IConfiguration>().Object)
                {
                    ProcessSignOutRequestCalling = (message, claimsPrincipal, url) =>
                        {
                            Assert.AreSame(requestInfo.Message, message);
                            Assert.AreSame(principal, claimsPrincipal);
                            Assert.AreEqual(replyToAddress, url);
                        }
                };

            // ReSharper disable EmptyGeneralCatchClause

            try
            {
                wsFedLoginRequestProcessor.Process(requestInfo, new Mock<HttpResponseBase>().Object);
            }
            catch
            {
            }

            // ReSharper restore EmptyGeneralCatchClause

            sessionTokenCookieManagerMock.Verify();
            replyToPartyMock.Verify();
        }

        public class TestSecurityTokenServiceConfiguration : SecurityTokenServiceConfiguration
        {
            public TestSecurityTokenServiceConfiguration()
            {
                this.SecurityTokenService = typeof (TestSecurityTokenService);
                this.TokenIssuerName = GeneralConstant.TokenIssuerName;
            }
        }

        public class TestSecurityTokenService : SecurityTokenService
        {
            private readonly string replyTo;
            private readonly X509Certificate2 certificate;

            public TestSecurityTokenService(string replyTo) : base(new TestSecurityTokenServiceConfiguration())
            {
                this.replyTo = replyTo;
                this.certificate = CertificateFactory.Create();
            }

            protected override Scope GetScope(ClaimsPrincipal principal, RequestSecurityToken request)
            {
                return new Scope(this.replyTo, new X509SigningCredentials(this.certificate))
                    {
                        ReplyToAddress = this.replyTo,
                        TokenEncryptionRequired = false
                    };
            }

            protected override ClaimsIdentity GetOutputClaimsIdentity(
                ClaimsPrincipal principal, RequestSecurityToken request, Scope scope)
            {
                return principal.Identities.Single();
            }
        }
    }
}