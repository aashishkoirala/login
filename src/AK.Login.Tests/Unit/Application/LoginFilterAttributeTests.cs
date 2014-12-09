/*******************************************************************************************************************************
 * AK.Login.Tests.Unit.Application.LoginFilterAttributeTests
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
using AK.Login.Presentation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

#endregion

namespace AK.Login.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for LoginFilterAttribute.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class LoginFilterAttributeTests
    {
        [TestMethod, TestCategory("Unit")]
        public void LoginFilterAttribute_Works_When_BaseUrl_In_Config()
        {
            var baseUrlSet = false;

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.SetupGet(x => x.BaseUrl).Returns("SomeValue").Verifiable();
            configurationMock.SetupSet(x => x.BaseUrl = "Something").Callback(() => baseUrlSet = true);

            var loginFilterAttribute = new LoginFilterAttribute
                {
                    Logger = new Mock<IAppLogger>().Object,
                    Configuration = configurationMock.Object
                };

            var actionExecutingContext = new ActionExecutingContext();
            loginFilterAttribute.OnActionExecuting(actionExecutingContext);

            Assert.IsFalse(baseUrlSet);
            configurationMock.Verify();
        }

        [TestMethod, TestCategory("Unit")]
        public void LoginFilterAttribute_Works_When_BaseUrl_Not_In_Config()
        {
            const string baseUrl = "http://www.test.com/";

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.SetupGet(x => x.BaseUrl).Returns(string.Empty).Verifiable();
            configurationMock.SetupSet(x => x.BaseUrl = baseUrl).Verifiable();

            var loginFilterAttribute = new LoginFilterAttribute
                {
                    Logger = new Mock<IAppLogger>().Object,
                    Configuration = configurationMock.Object
                };

            RouteTable.Routes.MapRoute(
                GeneralConstant.DefaultBaseRouteName, "{action}",
                new {controller = "Login", action = LoginController.ActionNames.Main});

            var route = new Route(string.Empty, new MvcRouteHandler());
            var httpContext = new HttpContextWrapper(
                new HttpContext(new HttpRequest(string.Empty, baseUrl, string.Empty), new HttpResponse(null)));

            var actionExecutingContext = new ActionExecutingContext
                {
                    RequestContext = new RequestContext
                        {
                            HttpContext = httpContext,
                            RouteData = new RouteData {Route = route}
                        }
                };

            loginFilterAttribute.OnActionExecuting(actionExecutingContext);
            configurationMock.Verify();
        }
    }
}