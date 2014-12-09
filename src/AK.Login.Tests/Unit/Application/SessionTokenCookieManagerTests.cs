/*******************************************************************************************************************************
 * AK.Login.Tests.Unit.Application.SessionTokenCookieManagerTests
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
using AK.Login.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IdentityModel.Tokens;
using System.IO;
using System.Security.Claims;
using System.Web;

#endregion

namespace AK.Login.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for SessionTokenCookieManager.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class SessionTokenCookieManagerTests
    {
        private static ISessionTokenCookieManager sessionTokenCookieManager;
        private static Mock<ICertificateStore> certificateStoreMock;
        private static Mock<IConfiguration> configurationMock;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            var certificate = CertificateFactory.Create();
            certificateStoreMock = new Mock<ICertificateStore>();
            certificateStoreMock.SetupGet(x => x.Certificate).Returns(certificate).Verifiable();

            configurationMock = new Mock<IConfiguration>();
            configurationMock.SetupGet(x => x.RequireSsl).Returns(false).Verifiable();

            sessionTokenCookieManager = new SessionTokenCookieManager(
                configurationMock.Object, certificateStoreMock.Object, new Mock<IAppLogger>().Object);
        }

        [TestMethod, TestCategory("Unit")]
        public void SessionTokenCookieManager_WriteSessionTokenCookie_Works()
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.Sid, "Test"));

            var principal = new ClaimsPrincipal(identity);
            var sessionSecurityToken = new SessionSecurityToken(principal, "Context");

            using (var writer = new StringWriter())
            {
                HttpContext.Current = new HttpContext(
                    new HttpRequest(string.Empty, "http://www.test.com", string.Empty), new HttpResponse(writer));

                sessionTokenCookieManager.WriteSessionTokenCookie(sessionSecurityToken);
            }

            certificateStoreMock.Verify();
            configurationMock.Verify();
        }

        [TestMethod, TestCategory("Unit")]
        public void SessionTokenCookieManager_ReadSessionTokenCookie_Works()
        {
            // There is no easy way to fake an actual request with a session token cookie,
            // so skipping this one for now.
        }

        [TestMethod, TestCategory("Unit")]
        public void SessionTokenCookieManager_DeleteSessionTokenCookie_Works()
        {
            using (var writer = new StringWriter())
            {
                HttpContext.Current = new HttpContext(
                    new HttpRequest(string.Empty, "http://www.test.com", string.Empty), new HttpResponse(writer));

                sessionTokenCookieManager.DeleteSessionTokenCookie();
            }

            certificateStoreMock.Verify();
            configurationMock.Verify();
        }
    }
}