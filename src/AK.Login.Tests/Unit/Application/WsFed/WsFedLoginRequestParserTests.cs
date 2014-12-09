/*******************************************************************************************************************************
 * AK.Login.Tests.Unit.Application.WsFed.WsFedLoginRequestParserTests
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
using AK.Login.Application.WsFed;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Specialized;
using System.IdentityModel.Services;
using System.Web;

#endregion

namespace AK.Login.Tests.Unit.Application.WsFed
{
    /// <summary>
    /// Unit tests for WsFedLoginRequestParser.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class WsFedLoginRequestParserTests
    {
        [TestMethod, TestCategory("Unit")]
        public void WsFedLoginRequestParser_Parse_Initial_Sign_In_Get_Works()
        {
            string realm;
            Mock<IReplyToParty> replyToPartyMock;
            Mock<IReplyToPartyFactory> replyToPartyFactoryMock;
            Mock<IWsFedInitialUrlCookieManager> wsFedInitialUrlCookieManagerMock;

            var parser = GetWsFedLoginRequestParser(
                out realm, out replyToPartyMock, out replyToPartyFactoryMock,
                out wsFedInitialUrlCookieManagerMock);

            var queryString = string.Format("wa=wsignin1.0&wtrealm={0}", realm);
            var url = string.Format("http://www.test.com/?{0}", queryString);

            var request = new HttpRequest(string.Empty, url, queryString);
            var requestInfo = parser.Parse(new HttpRequestWrapper(request), LoginStage.Initial);

            Assert.IsTrue(requestInfo.Parsed);
            Assert.AreEqual(realm, requestInfo.Realm);
            Assert.AreEqual(ProtocolName.WsFed, requestInfo.Protocol);
            Assert.AreEqual(LoginRequestType.SignIn, requestInfo.Type);
            Assert.IsInstanceOfType(requestInfo.Message, typeof (SignInRequestMessage));

            replyToPartyFactoryMock.Verify();
        }

        [TestMethod, TestCategory("Unit")]
        public void WsFedLoginRequestParser_Parse_Initial_Sign_In_Post_Works()
        {
            string realm;
            Mock<IReplyToParty> replyToPartyMock;
            Mock<IReplyToPartyFactory> replyToPartyFactoryMock;
            Mock<IWsFedInitialUrlCookieManager> wsFedInitialUrlCookieManagerMock;

            var parser = GetWsFedLoginRequestParser(
                out realm, out replyToPartyMock, out replyToPartyFactoryMock,
                out wsFedInitialUrlCookieManagerMock);

            var form = new NameValueCollection();
            form["wa"] = "wsignin1.0";
            form["wtrealm"] = realm;
            form["wreply"] = "http://www.test.com";

            var unvalidatedRequestMock = new Mock<UnvalidatedRequestValuesBase>();
            unvalidatedRequestMock.SetupGet(x => x.Form).Returns(form).Verifiable();

            var requestMock = new Mock<HttpRequestBase>();
            requestMock.SetupGet(x => x.Url).Returns(new Uri("http://www.test.com")).Verifiable();
            requestMock.SetupGet(x => x.HttpMethod).Returns("POST").Verifiable();
            requestMock.SetupGet(x => x.Form).Returns(form).Verifiable();
            requestMock.SetupGet(x => x.Unvalidated).Returns(unvalidatedRequestMock.Object).Verifiable();

            var requestInfo = parser.Parse(requestMock.Object, LoginStage.Initial);

            Assert.IsTrue(requestInfo.Parsed);
            Assert.AreEqual(realm, requestInfo.Realm);
            Assert.AreEqual(ProtocolName.WsFed, requestInfo.Protocol);
            Assert.AreEqual(LoginRequestType.SignIn, requestInfo.Type);
            Assert.IsInstanceOfType(requestInfo.Message, typeof (SignInRequestMessage));

            requestMock.Verify();
            unvalidatedRequestMock.Verify();
            replyToPartyFactoryMock.Verify();
        }

        [TestMethod, TestCategory("Unit")]
        public void WsFedLoginRequestParser_Parse_Authenticated_Sign_In_Works()
        {
            string realm;
            Mock<IReplyToParty> replyToPartyMock;
            Mock<IReplyToPartyFactory> replyToPartyFactoryMock;
            Mock<IWsFedInitialUrlCookieManager> wsFedInitialUrlCookieManagerMock;

            var parser = GetWsFedLoginRequestParser(
                out realm, out replyToPartyMock, out replyToPartyFactoryMock,
                out wsFedInitialUrlCookieManagerMock);

            var request = new HttpRequest(string.Empty, "http://www.test.com", string.Empty);
            var requestInfo = parser.Parse(new HttpRequestWrapper(request), LoginStage.Authenticated);

            Assert.IsTrue(requestInfo.Parsed);
            Assert.AreEqual(realm, requestInfo.Realm);
            Assert.AreEqual(ProtocolName.WsFed, requestInfo.Protocol);
            Assert.AreEqual(LoginRequestType.SignIn, requestInfo.Type);
            Assert.IsInstanceOfType(requestInfo.Message, typeof (SignInRequestMessage));

            replyToPartyFactoryMock.Verify();
            wsFedInitialUrlCookieManagerMock.Verify();
        }

        [TestMethod, TestCategory("Unit")]
        public void WsFedLoginRequestParser_Parse_Sign_Out_Get_Works()
        {
            string realm;
            Mock<IReplyToParty> replyToPartyMock;
            Mock<IReplyToPartyFactory> replyToPartyFactoryMock;
            Mock<IWsFedInitialUrlCookieManager> wsFedInitialUrlCookieManagerMock;

            var parser = GetWsFedLoginRequestParser(
                out realm, out replyToPartyMock, out replyToPartyFactoryMock,
                out wsFedInitialUrlCookieManagerMock);

            var queryString = string.Format("wa=wsignout1.0&wtrealm={0}&wreply={0}", realm);
            var url = string.Format("http://www.test.com/?{0}", queryString);

            var request = new HttpRequest(string.Empty, url, queryString);
            var requestInfo = parser.Parse(new HttpRequestWrapper(request), LoginStage.Initial);

            Assert.IsTrue(requestInfo.Parsed);
            Assert.AreEqual(realm, requestInfo.Realm);
            Assert.AreEqual(ProtocolName.WsFed, requestInfo.Protocol);
            Assert.AreEqual(LoginRequestType.SignOut, requestInfo.Type);
            Assert.IsInstanceOfType(requestInfo.Message, typeof (SignOutRequestMessage));

            replyToPartyFactoryMock.Verify();
        }

        [TestMethod, TestCategory("Unit")]
        public void WsFedLoginRequestParser_Parse_Sign_Out_Post_Works()
        {
            string realm;
            Mock<IReplyToParty> replyToPartyMock;
            Mock<IReplyToPartyFactory> replyToPartyFactoryMock;
            Mock<IWsFedInitialUrlCookieManager> wsFedInitialUrlCookieManagerMock;

            var parser = GetWsFedLoginRequestParser(
                out realm, out replyToPartyMock, out replyToPartyFactoryMock,
                out wsFedInitialUrlCookieManagerMock);

            var form = new NameValueCollection();
            form["wa"] = "wsignout1.0";
            form["wtrealm"] = realm;
            form["wreply"] = realm;

            var unvalidatedRequestMock = new Mock<UnvalidatedRequestValuesBase>();
            unvalidatedRequestMock.SetupGet(x => x.Form).Returns(form).Verifiable();

            var requestMock = new Mock<HttpRequestBase>();
            requestMock.SetupGet(x => x.Url).Returns(new Uri("http://www.test.com")).Verifiable();
            requestMock.SetupGet(x => x.HttpMethod).Returns("POST").Verifiable();
            requestMock.SetupGet(x => x.Form).Returns(form).Verifiable();
            requestMock.SetupGet(x => x.Unvalidated).Returns(unvalidatedRequestMock.Object).Verifiable();

            var requestInfo = parser.Parse(requestMock.Object, LoginStage.Initial);

            Assert.IsTrue(requestInfo.Parsed);
            Assert.AreEqual(realm, requestInfo.Realm);
            Assert.AreEqual(ProtocolName.WsFed, requestInfo.Protocol);
            Assert.AreEqual(LoginRequestType.SignOut, requestInfo.Type);
            Assert.IsInstanceOfType(requestInfo.Message, typeof (SignOutRequestMessage));

            requestMock.Verify();
            unvalidatedRequestMock.Verify();
        }

        [TestMethod, TestCategory("Unit")]
        public void WsFedLoginRequestParser_Parse_Invalid_Request_Not_Parsed()
        {
            string realm;
            Mock<IReplyToParty> replyToPartyMock;
            Mock<IReplyToPartyFactory> replyToPartyFactoryMock;
            Mock<IWsFedInitialUrlCookieManager> wsFedInitialUrlCookieManagerMock;

            var parser = GetWsFedLoginRequestParser(
                out realm, out replyToPartyMock, out replyToPartyFactoryMock,
                out wsFedInitialUrlCookieManagerMock);

            var request = new HttpRequest(string.Empty, "http://www.test.com", string.Empty);
            var requestInfo = parser.Parse(new HttpRequestWrapper(request), LoginStage.Initial);

            Assert.IsFalse(requestInfo.Parsed);
        }

        private static ILoginRequestParser GetWsFedLoginRequestParser(
            out string realm,
            out Mock<IReplyToParty> replyToPartyMock,
            out Mock<IReplyToPartyFactory> replyToPartyFactoryMock,
            out Mock<IWsFedInitialUrlCookieManager> wsFedInitialUrlCookieManagerMock)
        {
            realm = "TestRealm";
            var realmCopy = realm;

            var queryString = string.Format("wa=wsignin1.0&wtrealm={0}", realm);
            var url = string.Format("http://www.test.com/?{0}", queryString);

            replyToPartyMock = new Mock<IReplyToParty>();
            replyToPartyMock.SetupGet(x => x.Realm).Returns(realm).Verifiable();
            replyToPartyMock.SetupGet(x => x.ReplyToAddress).Returns("http://www.test.com").Verifiable();

            replyToPartyFactoryMock = new Mock<IReplyToPartyFactory>();
            replyToPartyFactoryMock.Setup(x => x.Create(realmCopy)).Returns(replyToPartyMock.Object).Verifiable();

            wsFedInitialUrlCookieManagerMock = new Mock<IWsFedInitialUrlCookieManager>();
            wsFedInitialUrlCookieManagerMock
                .Setup(x => x.Read(It.IsAny<HttpRequestBase>()))
                .Returns(url)
                .Verifiable();

            return new WsFedLoginRequestParser(replyToPartyFactoryMock.Object, wsFedInitialUrlCookieManagerMock.Object);
        }
    }
}