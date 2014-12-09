/*******************************************************************************************************************************
 * AK.Login.Application.Facebook.FacebookLoginRequest
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

namespace AK.Login.Application.Facebook
{
    /// <summary>
    /// Represents a Facebook login request with information sent over from Facebook's JS API.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class FacebookLoginRequest
    {
        /// <summary>
        /// Signed request payload string.
        /// </summary>
        public string SignedRequest { get; set; }

        /// <summary>
        /// User ID string.
        /// </summary>
        public string UserId { get; set; }
    }
}