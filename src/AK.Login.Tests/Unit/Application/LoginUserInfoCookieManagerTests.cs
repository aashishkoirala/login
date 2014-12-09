/*******************************************************************************************************************************
 * AK.Login.Tests.Unit.Application.LoginUserInfoCookieManagerTests
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

using AK.Commons.Security;
using AK.Login.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Web;

#endregion

namespace AK.Login.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for LoginUserInfoCookieManager.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class LoginUserInfoCookieManagerTests
    {
        [TestMethod, TestCategory("Unit")]
        public void LoginUserInfoCookieManager_Read_Works()
        {
            const string cookieValue =
                "AAEAAAD/////AQAAAAAAAAAMAgAAAE1BSy5Db21tb25zLCBWZXJzaW9uPTEuMC4yLjAsIEN1bHR1cmU9bmV1d" +
                "HJhbCwgUHVibGljS2V5VG9rZW49ZDFiN2Q5YzA1OWU3YzZiZQUBAAAAIUFLLkNvbW1vbnMuU2VjdXJpdHkuTG" +
                "9naW5Vc2VySW5mbwQAAAAbPFVzZXJFeGlzdHM+a19fQmFja2luZ0ZpZWxkFzxVc2VySWQ+a19fQmFja2luZ0Z" +
                "pZWxkGTxVc2VyTmFtZT5rX19CYWNraW5nRmllbGQcPERpc3BsYXlOYW1lPmtfX0JhY2tpbmdGaWVsZAABAQEB" +
                "AgAAAAAGAwAAACQwMDAwMDAwMC0wMDAwLTAwMDAtMDAwMC0wMDAwMDAwMDAwMDAKCgs=.MSw3sU2O7qsqSGQD" +
                "jzDz7YDwi8MZudCHuM0td9BawX8=";

            var certificate = CertificateFactory.Create();

            var certificateStoreMock = new Mock<ICertificateStore>();
            certificateStoreMock.SetupGet(x => x.Certificate).Returns(certificate).Verifiable();

            var loginUserInfoCookieManager = new LoginUserInfoCookieManager(certificateStoreMock.Object);

            var request = new HttpRequest(string.Empty, "http://www.test.com", string.Empty);
            request.Cookies.Add(new HttpCookie(GeneralConstant.UserInfoCookieName, cookieValue));

            var loginUserInfo = loginUserInfoCookieManager.Read(new HttpRequestWrapper(request));

            Assert.AreEqual(loginUserInfo.UserId, Guid.Empty.ToString());
            certificateStoreMock.Verify();
        }

        [TestMethod, TestCategory("Unit")]
        public void LoginUserInfoCookieManager_Write_Works()
        {
            const string expectedCookieValue =
                "AAEAAAD/////AQAAAAAAAAAMAgAAAE1BSy5Db21tb25zLCBWZXJzaW9uPTEuMC4yLjAsIEN1bHR1cmU9bmV1d" +
                "HJhbCwgUHVibGljS2V5VG9rZW49ZDFiN2Q5YzA1OWU3YzZiZQUBAAAAIUFLLkNvbW1vbnMuU2VjdXJpdHkuTG" +
                "9naW5Vc2VySW5mbwQAAAAbPFVzZXJFeGlzdHM+a19fQmFja2luZ0ZpZWxkFzxVc2VySWQ+a19fQmFja2luZ0Z" +
                "pZWxkGTxVc2VyTmFtZT5rX19CYWNraW5nRmllbGQcPERpc3BsYXlOYW1lPmtfX0JhY2tpbmdGaWVsZAABAQEB" +
                "AgAAAAAGAwAAACQwMDAwMDAwMC0wMDAwLTAwMDAtMDAwMC0wMDAwMDAwMDAwMDAKCgs=.MSw3sU2O7qsqSGQD" +
                "jzDz7YDwi8MZudCHuM0td9BawX8=";

            var certificate = CertificateFactory.Create();
            var loginUserInfo = new LoginUserInfo {UserId = Guid.Empty.ToString()};

            var certificateStoreMock = new Mock<ICertificateStore>();
            certificateStoreMock.SetupGet(x => x.Certificate).Returns(certificate).Verifiable();

            var loginUserInfoCookieManager = new LoginUserInfoCookieManager(certificateStoreMock.Object);
            var cookies = new HttpCookieCollection();

            var responseMock = new Mock<HttpResponseBase>();
            responseMock.SetupGet(x => x.Cookies).Returns(cookies).Verifiable();

            loginUserInfoCookieManager.Write(loginUserInfo, responseMock.Object);

            Assert.IsNotNull(cookies[GeneralConstant.UserInfoCookieName]);
            Assert.AreEqual(expectedCookieValue, cookies[GeneralConstant.UserInfoCookieName].Value);

            certificateStoreMock.Verify();
            responseMock.Verify();
        }

        [TestMethod, TestCategory("Unit")]
        public void LoginUserInfoCookieManager_Clear_Works()
        {
            var certificate = CertificateFactory.Create();
            var certificateStoreMock = new Mock<ICertificateStore>();
            certificateStoreMock.SetupGet(x => x.Certificate).Returns(certificate).Verifiable();

            var loginUserInfoCookieManager = new LoginUserInfoCookieManager(certificateStoreMock.Object);
            var cookies = new HttpCookieCollection();

            var responseMock = new Mock<HttpResponseBase>();
            responseMock.SetupGet(x => x.Cookies).Returns(cookies).Verifiable();

            loginUserInfoCookieManager.Clear(responseMock.Object);

            var cookie = cookies[GeneralConstant.UserInfoCookieName];
            Assert.IsNotNull(cookie);
            Assert.IsTrue(cookie.Expires < DateTime.Now);

            certificateStoreMock.Verify();
            responseMock.Verify();
        }
    }
}