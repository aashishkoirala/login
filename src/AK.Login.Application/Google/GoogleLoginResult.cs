/*******************************************************************************************************************************
 * AK.Login.Application.Google.GoogleLoginResult
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

using AK.Commons.Security;

namespace AK.Login.Application.Google
{
    /// <summary>
    /// Represents result of Google login.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class GoogleLoginResult
    {
        /// <summary>
        /// Whether we were logged in (as opposed to being challenged).
        /// </summary>
        public bool IsLoggedIn { get; set; }

        /// <summary>
        /// User information if we were logged in.
        /// </summary>
        public LoginUserInfo LoginUserInfo { get; set; }

        /// <summary>
        /// URL within Google to redirect to if challenged.
        /// </summary>
        public string RedirectUrl { get; set; }
    }
}