/*******************************************************************************************************************************
 * AK.Login.Application.SessionTokenCookieManager
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
using System;
using System.ComponentModel.Composition;
using System.IdentityModel.Services;
using System.IdentityModel.Services.Configuration;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;

#endregion

namespace AK.Login.Application
{
    /// <summary>
    /// Handles reading/writing etc. of the session-token cookie (i.e. the FedAuth cookie) from/to the HTTP request/response.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface ISessionTokenCookieManager
    {
        /// <summary>
        /// Writes a session token cookie to the HTTP response based on the given security token.
        /// </summary>
        /// <param name="securityToken">Security token.</param>
        void WriteSessionTokenCookie(SessionSecurityToken securityToken);

        /// <summary>
        /// Reads the security token from the session token cookie present in the HTTP request.
        /// </summary>
        /// <returns>Result enclosing the security token, or with errors indicating it could not be read.</returns>
        OperationResult<SessionSecurityToken> ReadSessionTokenCookie();

        /// <summary>
        /// Deletes the session token cookie from the HTTP response.
        /// </summary>
        void DeleteSessionTokenCookie();
    }

    /// <summary>
    /// The one and only implementation of ISessionTokenCookieManager.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (ISessionTokenCookieManager)), PartCreationPolicy(CreationPolicy.Shared)]
    public class SessionTokenCookieManager : ISessionTokenCookieManager
    {
        private readonly SessionAuthenticationModule sessionAuthenticationModule;

        [ImportingConstructor]
        public SessionTokenCookieManager(
            [Import] IConfiguration configuration,
            [Import] ICertificateStore certificateStore,
            [Import] IAppLogger logger)
        {
            this.sessionAuthenticationModule = CreateSessionAuthenticationModule(
                certificateStore.Certificate, configuration.RequireSsl, logger);
        }

        public void WriteSessionTokenCookie(SessionSecurityToken securityToken)
        {
            this.sessionAuthenticationModule.WriteSessionTokenToCookie(securityToken);
        }

        public OperationResult<SessionSecurityToken> ReadSessionTokenCookie()
        {
            SessionSecurityToken token;
            return this.sessionAuthenticationModule.TryReadSessionTokenFromCookie(out token)
                       ? new OperationResult<SessionSecurityToken>(token)
                       : new OperationResult<SessionSecurityToken>(LoginErrorCodes.CannotReadSessionTokenCookie);
        }

        public void DeleteSessionTokenCookie()
        {
            this.sessionAuthenticationModule.DeleteSessionTokenCookie();
        }

        private static SessionAuthenticationModule CreateSessionAuthenticationModule(
            X509Certificate2 certificate, bool requireSsl, IAppLogger logger)
        {
            logger.Information("Creating a new SessionAuthenticationModule instance.");

            var federationConfiguration = new FederationConfiguration {CookieHandler = new ChunkedCookieHandler()};
            AssignCookieHandlerProperties(federationConfiguration.CookieHandler, requireSsl);

            try
            {
                federationConfiguration.AssignSecurityTokenResolver(certificate);
            }
            catch (InvalidOperationException ex)
            {
                // Ignore the error, if one occurs, that says the certificate is already in the
                // trusted issuer list.
                //
                if (ex.Message.StartsWith("ID4265")) logger.Error(ex);
                else throw;
            }

            var sessionAuthenticationModule = new SessionAuthenticationModule
                {
                    FederationConfiguration = federationConfiguration
                };
            AssignCookieHandlerProperties(sessionAuthenticationModule.CookieHandler, requireSsl);

            return sessionAuthenticationModule;
        }

        private static void AssignCookieHandlerProperties(
            CookieHandler cookieHandler, bool requireSsl)
        {
            cookieHandler.RequireSsl = requireSsl;
            cookieHandler.Domain = string.Empty;
            cookieHandler.Path = GeneralConstant.CookieHandlerPath;
            cookieHandler.Name = GeneralConstant.CookieHandlerName;
        }
    }
}