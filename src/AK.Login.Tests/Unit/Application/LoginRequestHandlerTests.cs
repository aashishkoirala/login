/*******************************************************************************************************************************
 * AK.Login.Tests.Unit.Application.LoginRequestHandlerTests
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

using AK.Commons.Composition;
using AK.Commons.Logging;
using AK.Login.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using System.Web;
using System.Web.Mvc;

#endregion

namespace AK.Login.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for LoginRequestHandler.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class LoginRequestHandlerTests
    {
        [TestMethod, TestCategory("Unit")]
        public void LoginRequestHandler_Parse_Returns_First_Parsed()
        {
            const string expectedProtocol = "Test";

            var parserThatParsesMock = new Mock<ILoginRequestParser>();
            parserThatParsesMock
                .Setup(x => x.Parse(It.IsAny<HttpRequestBase>(), It.IsAny<LoginStage>()))
                .Returns(new LoginRequestInfo {Parsed = true, Protocol = expectedProtocol})
                .Verifiable();

            var parserThatDoesNotParseMock = new Mock<ILoginRequestParser>();
            parserThatDoesNotParseMock
                .Setup(x => x.Parse(It.IsAny<HttpRequestBase>(), It.IsAny<LoginStage>()))
                .Returns(new LoginRequestInfo {Parsed = false})
                .Verifiable();

            var parsers = new[] {parserThatDoesNotParseMock.Object, parserThatParsesMock.Object};

            var loginRequestHandler = new LoginRequestHandler(
                new Mock<IComposer>().Object, new Mock<IAppLogger>().Object, parsers);

            var requestInfo = loginRequestHandler.Parse(
                LoginStage.Initial,
                new HttpRequestWrapper(new HttpRequest(null, "http://www.test.com", null)));

            Assert.IsNotNull(requestInfo);
            Assert.IsTrue(requestInfo.Parsed);
            Assert.AreEqual(expectedProtocol, requestInfo.Protocol);
            Assert.AreEqual(LoginStage.Initial, requestInfo.Stage);
            parserThatParsesMock.Verify();
            parserThatDoesNotParseMock.Verify();
        }

        [TestMethod, TestCategory("Unit")]
        public void LoginRequestHandler_Parse_Returns_Null_When_Not_Parsed()
        {
            var parserThatDoesNotParseMock = new Mock<ILoginRequestParser>();
            parserThatDoesNotParseMock
                .Setup(x => x.Parse(It.IsAny<HttpRequestBase>(), It.IsAny<LoginStage>()))
                .Returns(new LoginRequestInfo {Parsed = false})
                .Verifiable();

            var parsers = new[] {parserThatDoesNotParseMock.Object};

            var loginRequestHandler = new LoginRequestHandler(
                new Mock<IComposer>().Object, new Mock<IAppLogger>().Object, parsers);

            var requestInfo = loginRequestHandler.Parse(
                LoginStage.Initial,
                new HttpRequestWrapper(new HttpRequest(null, "http://www.test.com", null)));

            Assert.IsNull(requestInfo);
            parserThatDoesNotParseMock.Verify();
        }

        [TestMethod, TestCategory("Unit")]
        public void LoginRequestHandler_Execute_Interaction_Works()
        {
            const string protocol = "Test";

            var loginRequestInfo = new LoginRequestInfo
                {
                    Protocol = protocol,
                    Stage = LoginStage.Initial,
                    Request = new HttpRequestWrapper(new HttpRequest(string.Empty, "http://www.test.com", string.Empty))
                };

            var processorMock = new Mock<ILoginRequestProcessor>();
            processorMock
                .Setup(x => x.Process(loginRequestInfo, It.IsAny<HttpResponseBase>()))
                .Verifiable();

            var composerMock = new Mock<IComposer>();
            composerMock
                .Setup(x => x.Resolve<ILoginRequestProcessor>(protocol))
                .Returns(processorMock.Object)
                .Verifiable();

            var loginRequestHandler = new LoginRequestHandler(
                composerMock.Object,
                new Mock<IAppLogger>().Object,
                Enumerable.Empty<ILoginRequestParser>());

            var result = loginRequestHandler.Execute(loginRequestInfo, new HttpResponseWrapper(new HttpResponse(null)));

            Assert.IsInstanceOfType(result, typeof (EmptyResult));
            composerMock.Verify();
            processorMock.Verify();
        }
    }
}