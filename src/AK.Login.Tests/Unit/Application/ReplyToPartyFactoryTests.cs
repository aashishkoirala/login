/*******************************************************************************************************************************
 * AK.Login.Tests.Unit.Application.ReplyToPartyFactoryTests
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
using AK.Commons.Services;
using AK.Login.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.ServiceModel.Description;

#endregion

namespace AK.Login.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for ReplyToPartyFactory.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class ReplyToPartyFactoryTests
    {
        [TestMethod, TestCategory("Unit")]
        public void ReplyToPartyFactory_Create_Works()
        {
            var certificate = CertificateFactory.Create();
            var certificateStoreMock = new Mock<ICertificateStore>();
            certificateStoreMock.SetupGet(x => x.Certificate).Returns(certificate).Verifiable();

            var configurationMock = new Mock<IConfiguration>();
            configurationMock
                .Setup(x => x.GetReplyToAddress(ExpectedValue.Realm))
                .Returns(ExpectedValue.ReplyToAddress)
                .Verifiable();
            configurationMock
                .Setup(x => x.GetLoginServiceUrl(ExpectedValue.Realm))
                .Returns(ExpectedValue.LoginServiceUrl)
                .Verifiable();

            var serviceEndPointAccessed = false;

            ServiceCallerFactory.ServiceEndpointAccessor = type =>
                {
                    if (type != typeof (ILoginService)) throw new NotSupportedException();

                    serviceEndPointAccessed = true;
                    return new ServiceEndpoint(ContractDescription.GetContract(typeof (ILoginService)));
                };

            var replyToPartyFactory = new ReplyToPartyFactory(
                configurationMock.Object, new Mock<IAppLogger>().Object, certificateStoreMock.Object);

            var replyToParty = replyToPartyFactory.Create(ExpectedValue.Realm);

            Assert.IsNotNull(replyToParty);
            Assert.IsTrue(serviceEndPointAccessed);
            Assert.AreEqual(ExpectedValue.Realm, replyToParty.Realm);
            Assert.AreEqual(ExpectedValue.ReplyToAddress, replyToParty.ReplyToAddress);

            certificateStoreMock.Verify();
            configurationMock.Verify();
        }

        private static class ExpectedValue
        {
            public const string Realm = "TestRealm";
            public const string ReplyToAddress = "ReplyToAddress";
            public const string LoginServiceUrl = "http://www.test.com";
        }
    }
}