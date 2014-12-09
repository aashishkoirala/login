/*******************************************************************************************************************************
 * AK.Login.Application.WsFed.WsFedLoginRequestParser
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

using System;
using System.ComponentModel.Composition;
using System.IdentityModel.Services;
using System.Web;

#endregion

namespace AK.Login.Application.WsFed
{
    /// <summary>
    /// Implementation of ILoginRequestParser for WS-Fed protocol.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (ILoginRequestParser))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class WsFedLoginRequestParser : ILoginRequestParser
    {
        private readonly IReplyToPartyFactory replyToPartyFactory;
        private readonly IWsFedInitialUrlCookieManager wsFedInitialUrlCookieManager;

        [ImportingConstructor]
        public WsFedLoginRequestParser(
            [Import] IReplyToPartyFactory replyToPartyFactory,
            [Import] IWsFedInitialUrlCookieManager wsFedInitialUrlCookieManager)
        {
            this.replyToPartyFactory = replyToPartyFactory;
            this.wsFedInitialUrlCookieManager = wsFedInitialUrlCookieManager;
        }

        public LoginRequestInfo Parse(HttpRequestBase request, LoginStage stage)
        {
            SignInRequestMessage signInRequestMessage;
            SignOutRequestMessage signOutRequestMessage;

            var requestInfo = new LoginRequestInfo {Protocol = ProtocolName.WsFed, Request = request};

            // First, see if it is a WS-Fed sign-in message by inspecting the request parameters.
            // If not, see if it is a WS-Fed sign-out message. If neither, then as far as we are
            // concerned, it is not a WS-Fed message. If it is, then we parse the message into
            // a SignInRequestMessage or SignOutRequestMessage as needed.

            if (TryGetSignInRequestMessage(request, stage, out signInRequestMessage))
            {
                requestInfo.Message = signInRequestMessage;
                requestInfo.Type = LoginRequestType.SignIn;
                requestInfo.Realm = signInRequestMessage.Realm;
                requestInfo.Parsed = true;
            }
            else if (TryGetSignOutRequestMessage(request, out signOutRequestMessage))
            {
                requestInfo.Message = signOutRequestMessage;
                requestInfo.Type = LoginRequestType.SignOut;
                requestInfo.Realm = signOutRequestMessage.Reply;
                requestInfo.Parsed = true;
            }

            if (requestInfo.Parsed && !string.IsNullOrWhiteSpace(requestInfo.Realm))
            {
                requestInfo.ReplyToParty = this.replyToPartyFactory.Create(requestInfo.Realm);
            }

            return requestInfo;
        }

        private static bool TryGetSignOutRequestMessage(
            HttpRequestBase request, out SignOutRequestMessage signOutRequestMessage)
        {
            signOutRequestMessage = null;

            if (IsSignOutRequest(request))
            {
                signOutRequestMessage = request.HttpMethod == WsFedConstant.HttpPost
                                            ? WSFederationMessage.CreateFromFormPost(request) as SignOutRequestMessage
                                            : WSFederationMessage.CreateFromUri(request.Url) as SignOutRequestMessage;
            }

            return signOutRequestMessage != null;
        }

        private bool TryGetSignInRequestMessage(
            HttpRequestBase request, LoginStage stage, out SignInRequestMessage signInRequestMessage)
        {
            signInRequestMessage = null;

            if (stage == LoginStage.Authenticated)
            {
                var initialUrl = new Uri(this.wsFedInitialUrlCookieManager.Read(request));
                signInRequestMessage = WSFederationMessage.CreateFromUri(initialUrl) as SignInRequestMessage;
                return true;
            }

            if (IsSignInRequest(request))
            {
                signInRequestMessage = request.HttpMethod == WsFedConstant.HttpPost
                                           ? WSFederationMessage.CreateFromFormPost(request) as SignInRequestMessage
                                           : WSFederationMessage.CreateFromUri(request.Url) as SignInRequestMessage;
            }

            return signInRequestMessage != null;
        }

        private static bool IsSignInRequest(HttpRequestBase request)
        {
            var action = request.HttpMethod == WsFedConstant.HttpPost
                             ? request.Form[WsFedConstant.ActionParam]
                             : request.QueryString[WsFedConstant.ActionParam];

            return (!string.IsNullOrEmpty(action) && action == WsFedConstant.SignInAction);
        }

        private static bool IsSignOutRequest(HttpRequestBase request)
        {
            var action = request.HttpMethod == WsFedConstant.HttpPost
                             ? request.Form[WsFedConstant.ActionParam]
                             : request.QueryString[WsFedConstant.ActionParam];

            return (!string.IsNullOrEmpty(action) && action == WsFedConstant.SignOutAction);
        }
    }
}