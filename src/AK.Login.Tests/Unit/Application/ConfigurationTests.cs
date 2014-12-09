/*******************************************************************************************************************************
 * AK.Login.Tests.Unit.Application.ConfigurationTests
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

using AK.Commons.Configuration;
using AK.Commons.Logging;
using AK.Login.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

#endregion

namespace AK.Login.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for Configuration.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class ConfigurationTests
    {
        [TestMethod, TestCategory("Unit")]
        public void Configuration_Properties_Work()
        {
            var logger = new Mock<IAppLogger>().Object;
            var configMock = new Mock<IAppConfig>();

            configMock.Setup(x => x.Get("IsRelaxedSecurityMode", false)).Returns(ExpectedValue.IsRelaxedSecurityMode);
            configMock.Setup(x => x.Get<string>("Facebook.AppKey")).Returns(ExpectedValue.FacebookAppKey);
            configMock.Setup(x => x.Get<string>("Google.ClientSecretPath"))
                      .Returns(ExpectedValue.GoogleClientSecretPath);
            configMock.Setup(x => x.Get<string>("Google.ApplicationName")).Returns(ExpectedValue.GoogleApplicationName);
            configMock.Setup(x => x.Get("RequireSsl", true)).Returns(ExpectedValue.RequireSsl);
            configMock.Setup(x => x.Get("BaseUrl", "")).Returns(ExpectedValue.BaseUrl);
            configMock.Setup(x => x.Get("LoginPath", "Login")).Returns(ExpectedValue.LoginPath);

            var configuration = new Configuration(configMock.Object, logger);

            Assert.AreEqual(ExpectedValue.IsRelaxedSecurityMode, configuration.IsRelaxedSecurityMode);
            Assert.AreEqual(ExpectedValue.FacebookAppKey, configuration.FacebookAppKey);
            Assert.AreEqual(ExpectedValue.GoogleClientSecretPath, configuration.GoogleClientSecretPath);
            Assert.AreEqual(ExpectedValue.GoogleApplicationName, configuration.GoogleApplicationName);
            Assert.AreEqual(ExpectedValue.RequireSsl, configuration.RequireSsl);
            Assert.AreEqual(ExpectedValue.BaseUrl, configuration.BaseUrl);
            Assert.AreEqual(ExpectedValue.LoginPath, configuration.LoginPath);

            configMock.VerifyAll();
        }

        [TestMethod, TestCategory("Unit")]
        public void Configuration_GetReplyToAddress_Works()
        {
            var logger = new Mock<IAppLogger>().Object;
            var configMock = new Mock<IAppConfig>();

            const string realm = "TestRealm";
            var key = string.Format("ReplyToParty.{0}.ReplyToAddress", realm);

            configMock.Setup(x => x.Get<string>(key)).Returns(ExpectedValue.ReplyToAddress);

            var configuration = new Configuration(configMock.Object, logger);
            var replyToAddress = configuration.GetReplyToAddress(realm);

            Assert.AreEqual(ExpectedValue.ReplyToAddress, replyToAddress);
            configMock.VerifyAll();
        }

        [TestMethod, TestCategory("Unit")]
        public void Configuration_GetLoginServiceUrl_Works()
        {
            var logger = new Mock<IAppLogger>().Object;
            var configMock = new Mock<IAppConfig>();

            const string realm = "TestRealm";
            var key = string.Format("ReplyToParty.{0}.LoginServiceUrl", realm);

            configMock.Setup(x => x.Get<string>(key)).Returns(ExpectedValue.LoginServiceUrl);

            var configuration = new Configuration(configMock.Object, logger);
            var loginServiceUrl = configuration.GetLoginServiceUrl(realm);

            Assert.AreEqual(ExpectedValue.LoginServiceUrl, loginServiceUrl);
            configMock.VerifyAll();
        }

        private static class ExpectedValue
        {
            public const bool IsRelaxedSecurityMode = true;
            public const string FacebookAppKey = "FacebookAppKey";
            public const string GoogleClientSecretPath = "GoogleClientSecretPath";
            public const string GoogleApplicationName = "GoogleApplicationName";
            public const bool RequireSsl = true;
            public const string BaseUrl = "BaseUrl";
            public const string LoginPath = "LoginPath";
            public const string ReplyToAddress = "ReplyToAddress";
            public const string LoginServiceUrl = "LoginServiceUrl";
        }
    }
}