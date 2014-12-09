/*******************************************************************************************************************************
 * AK.Login.Tests.Unit.Application.LoginServiceEndpointFactoryTests
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

#endregion

namespace AK.Login.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for LoginServiceEndpointFactory.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class LoginServiceEndpointFactoryTests
    {
        [TestMethod, TestCategory("Unit")]
        public void LoginServiceEndpointFactory_Create_Works_In_Relaxed_Security_Mode()
        {
            var certificate = CertificateFactory.Create();

            var certificateStoreMock = new Mock<ICertificateStore>();
            certificateStoreMock.SetupGet(x => x.Certificate).Returns(certificate).Verifiable();

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.SetupGet(x => x.IsRelaxedSecurityMode).Returns(true).Verifiable();

            var loginServiceEndpointFactory = new LoginServiceEndpointFactory(
                configurationMock.Object, certificateStoreMock.Object);

            var serviceEndpoint = loginServiceEndpointFactory.Create();

            Assert.IsNotNull(serviceEndpoint);
            certificateStoreMock.Verify();
            configurationMock.Verify();
        }

        [TestMethod, TestCategory("Unit")]
        public void LoginServiceEndpointFactory_Create_Works_In_Non_Relaxed_Security_Mode()
        {
            var certificate = CertificateFactory.Create();

            var certificateStoreMock = new Mock<ICertificateStore>();
            certificateStoreMock.SetupGet(x => x.Certificate).Returns(certificate).Verifiable();

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.SetupGet(x => x.IsRelaxedSecurityMode).Returns(false).Verifiable();

            var loginServiceEndpointFactory = new LoginServiceEndpointFactory(
                configurationMock.Object, certificateStoreMock.Object);

            var serviceEndpoint = loginServiceEndpointFactory.Create();

            Assert.IsNotNull(serviceEndpoint);
            certificateStoreMock.Verify();
            configurationMock.Verify();
        }
    }
}