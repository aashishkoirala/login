/*******************************************************************************************************************************
 * AK.Login.Application.WsFed.WsFedLoginRequestProcessor
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

using System.Security.Claims;
using AK.Commons;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Web;

#endregion

namespace AK.Login.Application.WsFed
{
    /// <summary>
    /// Implementation of ILoginRequestProcessor for WS-Fed protocol.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(ProtocolName.WsFed, typeof (ILoginRequestProcessor))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class WsFedLoginRequestProcessor : ILoginRequestProcessor
    {
        private readonly IDictionary<LoginRequestType,
            Action<LoginRequestInfo, SessionSecurityToken, HttpResponseBase>> processActionMap;

        private readonly ISessionTokenCookieManager sessionTokenCookieManager;
        private readonly ISecurityTokenServiceFactory securityTokenServiceFactory;
        private readonly IWsFedInitialUrlCookieManager wsFedInitialUrlCookieManager;
        private readonly IConfiguration configuration;

        [ImportingConstructor]
        public WsFedLoginRequestProcessor(
            [Import] ISessionTokenCookieManager sessionTokenCookieManager,
            [Import] ISecurityTokenServiceFactory securityTokenServiceFactory,
            [Import] IWsFedInitialUrlCookieManager wsFedInitialUrlCookieManager,
            [Import] IConfiguration configuration)
        {
            this.sessionTokenCookieManager = sessionTokenCookieManager;
            this.securityTokenServiceFactory = securityTokenServiceFactory;
            this.wsFedInitialUrlCookieManager = wsFedInitialUrlCookieManager;
            this.configuration = configuration;

            this.processActionMap = new Dictionary
                <LoginRequestType, Action<LoginRequestInfo, SessionSecurityToken, HttpResponseBase>>
                {
                    {LoginRequestType.SignIn, this.ProcessSignIn},
                    {LoginRequestType.SignOut, this.ProcessSignOut}
                };
        }

        /// <summary>
        /// Set this action to detect ProcessSignOut being called for testing.
        /// </summary>
        public Action<FederationMessage, ClaimsPrincipal, string> ProcessSignOutRequestCalling { get; set; }

        public void Process(LoginRequestInfo requestInfo, HttpResponseBase response)
        {
            var securityTokenResult = this.sessionTokenCookieManager.ReadSessionTokenCookie();
            var securityToken = securityTokenResult.IsSuccess ? securityTokenResult.Result : null;

            this.processActionMap[requestInfo.Type](requestInfo, securityToken, response);
        }

        private void ProcessSignIn(
            LoginRequestInfo requestInfo, SessionSecurityToken sessionSecurityToken, HttpResponseBase response)
        {
            var signInRequestMessage = (SignInRequestMessage) requestInfo.Message;

            // Main endpoint hit, already logged in. This means we were already authenticated or just got
            // through with authentication. The course of action is to use the token that is available to
            // write the cookie and redirect back to the reply-to party.
            //
            if (requestInfo.Stage == LoginStage.Initial && sessionSecurityToken != null)
            {
                var securityTokenService = this.securityTokenServiceFactory.Create(
                    requestInfo.ReplyToParty.ReplyToAddress,
                    requestInfo.ReplyToParty.ReplyToAddress);

                var signInResponseMessage = FederatedPassiveSecurityTokenServiceOperations
                    .ProcessSignInRequest(signInRequestMessage, sessionSecurityToken.ClaimsPrincipal,
                                          securityTokenService);

                signInResponseMessage.BaseUri = new Uri(requestInfo.ReplyToParty.ReplyToAddress);

                ExecuteResponse(response, x => x.Write(signInResponseMessage.WriteFormPost()));
                return;
            }

            // Main endpoint hit, not logged in. This is the initial request to the STS. We preserve
            // the initial URL and move on to the Login stage by redirecting to the login endpoint, which
            // shows the login page, among other things.
            //
            if (requestInfo.Stage == LoginStage.Initial)
            {
                var url = requestInfo.Request.Url != null ? requestInfo.Request.Url.ToString() : string.Empty;
                this.wsFedInitialUrlCookieManager.Write(url, response);
                var baseUrl = new Uri(this.configuration.BaseUrl);
                signInRequestMessage.BaseUri = baseUrl.Append(this.configuration.LoginPath);
                ExecuteResponse(response, x => x.Redirect(signInRequestMessage.WriteQueryString(), true));
                return;
            }

            if (requestInfo.Stage != LoginStage.Authenticated) return;

            // We just got authenticated - redirect back to the main endpoint so the code above for the
            // logged in case can execute.
            //
            var initialUrl = this.wsFedInitialUrlCookieManager.Read(requestInfo.Request);
            this.wsFedInitialUrlCookieManager.Clear(response);
            ExecuteResponse(response, x => x.Redirect(initialUrl));
        }

        private void ProcessSignOut(
            LoginRequestInfo requestInfo, SessionSecurityToken sessionSecurityToken, HttpResponseBase response)
        {
            // On Sign-Out, clear the session token cookie and redirect back to the reply-to-party.
            // That will usually lead to a sequence of events that lands us back on the login page, which is
            // desired.

            this.sessionTokenCookieManager.DeleteSessionTokenCookie();
            var url = requestInfo.ReplyToParty.ReplyToAddress;
            var signOutRequestMessage = (FederationMessage) requestInfo.Message;

            if (this.ProcessSignOutRequestCalling != null)
                this.ProcessSignOutRequestCalling(signOutRequestMessage, sessionSecurityToken.ClaimsPrincipal, url);

            FederatedPassiveSecurityTokenServiceOperations.ProcessSignOutRequest(
                signOutRequestMessage, sessionSecurityToken.ClaimsPrincipal, url, HttpContext.Current.Response);
        }

        private static void ExecuteResponse(HttpResponseBase response, Action<HttpResponseBase> action)
        {
            // We always write a non-cacheable response for security.

            response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            response.Cache.SetValidUntilExpires(false);
            response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetNoStore();
            action(response);
            response.Flush();
            response.End();
        }
    }
}