/*******************************************************************************************************************************
 * AK.Login.Application.Facebook.FacebookAuthenticator
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
using System;
using System.ComponentModel.Composition;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace AK.Login.Application.Facebook
{
    /// <summary>
    /// Handles Facebook login.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IFacebookAuthenticator
    {
        /// <summary>
        /// Logs in using Facebook.
        /// </summary>
        /// <param name="request">Login request (contains information sent by Facebook's JS API).</param>
        /// <returns>
        /// Login response with information extracted by parsing the signed request sent over in the request object.
        /// </returns>
        FacebookLoginResult Login(FacebookLoginRequest request);
    }

    /// <summary>
    /// The one and only implementation of IFacebookAuthenticator.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (IFacebookAuthenticator))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class FacebookAuthenticator : IFacebookAuthenticator
    {
        private readonly string keyString;

        [ImportingConstructor]
        public FacebookAuthenticator([Import] IConfiguration config)
        {
            this.keyString = config.FacebookAppKey;
        }

        public FacebookLoginResult Login(FacebookLoginRequest request)
        {
            // The following algorithm follows guidelines published by Facebook
            // on how to parse its "signedRequest" value.

            var key = Encoding.UTF8.GetBytes(keyString);

            var signedRequestParts = request.SignedRequest.Split('.');

            var signatureBase64 = signedRequestParts[0];
            signatureBase64 = signatureBase64.Replace("-", "").Replace("_", "");

            var payloadBase64Bytes = Encoding.UTF8.GetBytes(signedRequestParts[1]);

            byte[] payloadHash;
            using (var hmac = new HMACSHA256(key))
                payloadHash = hmac.ComputeHash(payloadBase64Bytes);

            var payloadHashBase64 = Convert.ToBase64String(payloadHash);
            payloadHashBase64 = payloadHashBase64.TrimEnd('=').Replace("/", "").Replace("+", "");

            return payloadHashBase64 != signatureBase64
                       ? new FacebookLoginResult {IsLoggedIn = false}
                       : new FacebookLoginResult
                           {
                               IsLoggedIn = true,
                               LoginUserInfo =
                                   new LoginUserInfo {UserName = FacebookConstant.UserIdPrefix + request.UserId}
                           };
        }
    }
}