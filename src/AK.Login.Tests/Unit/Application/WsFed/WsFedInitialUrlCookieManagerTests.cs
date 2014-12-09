/*******************************************************************************************************************************
 * AK.Login.Tests.Unit.Application.WsFed.WsFedInitialUrlCookieManagerTests
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

using AK.Login.Application.WsFed;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Web;

#endregion

namespace AK.Login.Tests.Unit.Application.WsFed
{
    /// <summary>
    /// Unit tests for WsFedInitialUrlCookieManager.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class WsFedInitialUrlCookieManagerTests
    {
        [TestMethod, TestCategory("Unit")]
        public void WsFedInitialUrlCookieManager_Read_Works()
        {
            const string expectedCookieValue = "TestValue";

            var request = new HttpRequest(string.Empty, "http://www.test.com", string.Empty);
            request.Cookies.Add(new HttpCookie(WsFedConstant.InitialUrlCookieName, expectedCookieValue));

            var wsFedInitialUrlCookieManager = new WsFedInitialUrlCookieManager();
            var cookieValue = wsFedInitialUrlCookieManager.Read(new HttpRequestWrapper(request));

            Assert.AreEqual(expectedCookieValue, cookieValue);
        }

        [TestMethod, TestCategory("Unit")]
        public void WsFedInitialUrlCookieManager_Write_Works()
        {
            const string expectedCookieValue = "TestValue";

            var cookies = new HttpCookieCollection();

            var responseMock = new Mock<HttpResponseBase>();
            responseMock.SetupGet(x => x.Cookies).Returns(cookies).Verifiable();

            var wsFedInitialUrlCookieManager = new WsFedInitialUrlCookieManager();
            wsFedInitialUrlCookieManager.Write(expectedCookieValue, responseMock.Object);

            var cookie = cookies[WsFedConstant.InitialUrlCookieName];
            Assert.IsNotNull(cookie);
            Assert.AreEqual(expectedCookieValue, cookie.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WsFedInitialUrlCookieManager_Clear_Works()
        {
            var cookies = new HttpCookieCollection();

            var responseMock = new Mock<HttpResponseBase>();
            responseMock.SetupGet(x => x.Cookies).Returns(cookies).Verifiable();

            var wsFedInitialUrlCookieManager = new WsFedInitialUrlCookieManager();
            wsFedInitialUrlCookieManager.Clear(responseMock.Object);

            var cookie = cookies[WsFedConstant.InitialUrlCookieName];
            Assert.IsNotNull(cookie);
            Assert.IsTrue(cookie.Expires < DateTime.Now);
        }
    }
}