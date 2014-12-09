/*******************************************************************************************************************************
 * AK.Login.Tests.Unit.Application.SessionTokenCookieWriterTests
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
using System.IdentityModel.Claims;
using System.IdentityModel.Tokens;
using System.Linq;

#endregion

namespace AK.Login.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for SessionTokenCookieWriter.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class SessionTokenCookieWriterTests
    {
        [TestMethod, TestCategory("Unit")]
        public void SessionTokenCookieWriter_Write_Works()
        {
            var sessionTokenCookieManagerMock = new Mock<ISessionTokenCookieManager>();
            sessionTokenCookieManagerMock
                .Setup(x => x.WriteSessionTokenCookie(It.Is<SessionSecurityToken>(y => IsExpectedToken(y))))
                .Verifiable();

            var sessionTokenCookieWriter = new SessionTokenCookieWriter(
                sessionTokenCookieManagerMock.Object, new Mock<IAppLogger>().Object);

            sessionTokenCookieWriter.Write(ExpectedValue.UserId, ExpectedValue.UserName, ExpectedValue.DisplayName);

            sessionTokenCookieManagerMock.Verify();
        }

        private static bool IsExpectedToken(SessionSecurityToken token)
        {
            var identity = token.ClaimsPrincipal.Identities.Single();

            var userId = identity.Claims.Single(x => x.Type == ClaimTypes.Sid).Value;
            var userName = identity.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            var displayName = identity.Claims.Single(x => x.Type == ClaimTypes.Name).Value;

            return userId == ExpectedValue.UserId && userName == ExpectedValue.UserName &&
                   displayName == ExpectedValue.DisplayName;
        }

        private static class ExpectedValue
        {
            public const string UserId = "UserId";
            public const string UserName = "UserName";
            public const string DisplayName = "DisplayName";
        }
    }
}