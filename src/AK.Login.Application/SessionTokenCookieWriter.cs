/*******************************************************************************************************************************
 * AK.Login.Application.SessionTokenCookieWriter
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
using System.ComponentModel.Composition;
using System.IdentityModel.Tokens;
using System.Security.Claims;

#endregion

namespace AK.Login.Application
{
    /// <summary>
    /// Composes a WIF security token and writes it as a session-token cookie based on the given
    /// user information.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface ISessionTokenCookieWriter
    {
        /// <summary>
        /// Composes a WIF security token and writes it as a session-token cookie based on the given
        /// user information.
        /// </summary>
        /// <param name="userId">User ID (written as Sid claim).</param>
        /// <param name="userName">User name (written as NameIdentifier claim).</param>
        /// <param name="displayName">Display name (written as Name claim).</param>
        void Write(string userId, string userName, string displayName);
    }

    /// <summary>
    /// The one and only implementation of ISessionTokenCookieWriter.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (ISessionTokenCookieWriter)), PartCreationPolicy(CreationPolicy.Shared)]
    public class SessionTokenCookieWriter : ISessionTokenCookieWriter
    {
        private readonly ISessionTokenCookieManager sessionTokenCookieManager;
        private readonly IAppLogger logger;

        [ImportingConstructor]
        public SessionTokenCookieWriter(
            [Import] ISessionTokenCookieManager sessionTokenCookieManager,
            [Import] IAppLogger logger)
        {
            this.sessionTokenCookieManager = sessionTokenCookieManager;
            this.logger = logger;
        }

        public void Write(string userId, string userName, string displayName)
        {
            this.logger.Information(string.Format(
                "Writing security token for User [Id = {0}, Name = {1}, Display Name = {2}].", userId,
                userName, displayName));

            var claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userName));
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, displayName));
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Sid, userId));

            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(claimsIdentity);

            var securityToken = new SessionSecurityToken(claimsPrincipal);
            this.sessionTokenCookieManager.WriteSessionTokenCookie(securityToken);
        }
    }
}