/*******************************************************************************************************************************
 * AK.Login.Tests.Unit.Application.SecurityTokenServiceFactoryTests
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

using AK.Login.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IdentityModel.Protocols.WSTrust;
using System.Security.Claims;

#endregion

namespace AK.Login.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for SecurityTokenServiceFactory.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class SecurityTokenServiceFactoryTests
    {
        [TestMethod, TestCategory("Unit")]
        public void SecurityTokenServiceFactory_Create_Works()
        {
            const string expectedAppliesTo = "http://www.appliesto.com/";

            var certificate = CertificateFactory.Create();
            var certificateStoreMock = new Mock<ICertificateStore>();
            certificateStoreMock.SetupGet(x => x.Certificate).Returns(certificate).Verifiable();

            var securityTokenServiceFactory = new SecurityTokenServiceFactory(certificateStoreMock.Object);

            var securityTokenService = securityTokenServiceFactory.Create(expectedAppliesTo, expectedAppliesTo);

            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.Sid, "Test"));

            var principal = new ClaimsPrincipal();
            principal.AddIdentity(identity);

            var requestSecurityToken = new RequestSecurityToken(RequestTypes.Issue, KeyTypes.Bearer);
            var response = securityTokenService.Issue(principal, requestSecurityToken);

            Assert.IsNotNull(response);
            Assert.AreEqual(expectedAppliesTo, response.AppliesTo.Uri.ToString());
            certificateStoreMock.Verify();
        }
    }
}