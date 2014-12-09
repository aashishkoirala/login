/*******************************************************************************************************************************
 * AK.Login.Application.LoginUserInfoCookieManager
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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Web;

#endregion

namespace AK.Login.Application
{
    /// <summary>
    /// Handles reading/writing the logged-in user-info structure from/to HTTP cookies.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface ILoginUserInfoCookieManager
    {
        /// <summary>
        /// Reads login user info from a cookie in the given HTTP request.
        /// </summary>
        /// <param name="request">HTTP request.</param>
        /// <returns>Login user info.</returns>
        LoginUserInfo Read(HttpRequestBase request);

        /// <summary>
        /// Writes a cookie based on the given login user info instance to the HTTP response.
        /// </summary>
        /// <param name="loginUserInfo">Login user info.</param>
        /// <param name="response">HTTP response to write to.</param>
        void Write(LoginUserInfo loginUserInfo, HttpResponseBase response);

        /// <summary>
        /// Clears the HTTP cookie associated with the login user info from the HTTP response.
        /// </summary>
        /// <param name="response">HTTP response to clear cookie from.</param>
        void Clear(HttpResponseBase response);
    }

    /// <summary>
    /// The one and only implementation of ILoginUserInfoCookieManager. The user info is
    /// binary-serialized, then its HMAC is calculated using the certificate provided.
    /// Both the values are then Base64-encoded and concatenated with a delimeter.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (ILoginUserInfoCookieManager)), PartCreationPolicy(CreationPolicy.Shared)]
    public class LoginUserInfoCookieManager : ILoginUserInfoCookieManager
    {
        private readonly BinaryFormatter formatter = new BinaryFormatter();
        private readonly X509Certificate2 certificate;

        [ImportingConstructor]
        public LoginUserInfoCookieManager([Import] ICertificateStore certificateStore)
        {
            this.certificate = certificateStore.Certificate;
        }

        public LoginUserInfo Read(HttpRequestBase request)
        {
            var cookie = request.Cookies[GeneralConstant.UserInfoCookieName];
            if (cookie == null) throw new LoginException(LoginErrorCodes.UserInfoCookieNotFound);

            var parts = cookie.Value.Split('.');
            var base64 = parts[0];
            var base64Hash = parts[1];

            var serialized = Convert.FromBase64String(base64);
            LoginUserInfo loginUserInfo;
            using (var stream = new MemoryStream(serialized))
                loginUserInfo = (LoginUserInfo) this.formatter.Deserialize(stream);

            byte[] hash;
            using (var hmac = new HMACSHA256(this.certificate.RawData))
                hash = hmac.ComputeHash(serialized);

            var computedBase64Hash = Convert.ToBase64String(hash);

            if (base64Hash != computedBase64Hash) throw new LoginException(LoginErrorCodes.UserInfoCookieBadSignature);

            return loginUserInfo;
        }

        public void Write(LoginUserInfo loginUserInfo, HttpResponseBase response)
        {
            byte[] serialized;
            using (var stream = new MemoryStream())
            {
                this.formatter.Serialize(stream, loginUserInfo);
                serialized = stream.ToArray();
            }
            var base64 = Convert.ToBase64String(serialized);

            byte[] hash;
            using (var hmac = new HMACSHA256(this.certificate.RawData))
                hash = hmac.ComputeHash(serialized);
            var base64Hash = Convert.ToBase64String(hash);

            var cookieValue = string.Format("{0}.{1}", base64, base64Hash);

            response.Cookies.Add(new HttpCookie(GeneralConstant.UserInfoCookieName, cookieValue)
                {
                    Expires = DateTime.Now.AddMinutes(5)
                });
        }

        public void Clear(HttpResponseBase response)
        {
            var cookie = new HttpCookie(GeneralConstant.UserInfoCookieName, string.Empty)
                {
                    Expires = DateTime.Now.AddHours(-1)
                };
            response.Cookies.Add(cookie);
        }
    }
}