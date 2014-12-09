/*******************************************************************************************************************************
 * AK.Login.Application.WsFed.WsFedInitialUrlCookieManager
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
using System.Web;

#endregion

namespace AK.Login.Application.WsFed
{
    /// <summary>
    /// Handles reading/writing of the Ws-Fed "InitialUrl" value to/from HTTP cookies. "InitialUrl" isn't really a standard
    /// Ws-Fed construct - it is something our Ws-Fed processor uses to keep track of URL state so it is easy to redirect after
    /// the user is authenticated.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IWsFedInitialUrlCookieManager
    {
        /// <summary>
        /// Reads the value of Ws-Fed "InitialUrl" from the cookie in the HTTP request.
        /// </summary>
        /// <param name="request">HTTP request.</param>
        /// <returns>The initial URL.</returns>
        string Read(HttpRequestBase request);

        /// <summary>
        /// Writes the value of Ws-Fed "InitialUrl" to a cookie in the HTTP response.
        /// </summary>
        /// <param name="value">Initial URL value.</param>
        /// <param name="response">HTTP response.</param>
        void Write(string value, HttpResponseBase response);

        /// <summary>
        /// Clears the cookie with initial URL from the HTTP response.
        /// </summary>
        /// <param name="response">HTTP response.</param>
        void Clear(HttpResponseBase response);
    }

    /// <summary>
    /// The one and only implementation of IWsFedInitialUrlCookieManager.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (IWsFedInitialUrlCookieManager)), PartCreationPolicy(CreationPolicy.Shared)]
    public class WsFedInitialUrlCookieManager : IWsFedInitialUrlCookieManager
    {
        public string Read(HttpRequestBase request)
        {
            var cookie = request.Cookies[WsFedConstant.InitialUrlCookieName];
            return cookie != null ? cookie.Value : string.Empty;
        }

        public void Write(string value, HttpResponseBase response)
        {
            response.Cookies.Add(new HttpCookie(WsFedConstant.InitialUrlCookieName, value)
                {
                    Expires = DateTime.Now.AddMinutes(5)
                });
        }

        public void Clear(HttpResponseBase response)
        {
            response.Cookies.Add(new HttpCookie(WsFedConstant.InitialUrlCookieName, string.Empty)
                {
                    Expires = DateTime.Now.AddHours(-1)
                });
        }
    }
}