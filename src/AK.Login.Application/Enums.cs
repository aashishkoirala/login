/*******************************************************************************************************************************
 * AK.Login.Application.Enums
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

using AK.Commons;

namespace AK.Login.Application
{
    /// <summary>
    /// Login request types.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public enum LoginRequestType
    {
        /// <summary>
        /// Sign-In request.
        /// </summary>
        SignIn,

        /// <summary>
        /// Sign-Out request.
        /// </summary>
        SignOut
    }

    /// <summary>
    /// Login flow stage.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public enum LoginStage
    {
        /// <summary>
        /// Initial endpoint hit.
        /// </summary>
        Initial,

        /// <summary>
        /// Login page being shown.
        /// </summary>
        ShowLoginPage,

        /// <summary>
        /// User authenticated.
        /// </summary>
        Authenticated
    }

    /// <summary>
    /// Error codes for LoginException.
    /// </summary>
    /// <author>Aashsih Koirala</author>
    public enum LoginErrorCodes
    {
        [EnumDescription("Cannot read session token cookie.")] CannotReadSessionTokenCookie,
        [EnumDescription("Unrecognized client secret path for Google.")] BadGoogleClientSecretPath,
        [EnumDescription("Cannot read user info, no cookie.")] UserInfoCookieNotFound,
        [EnumDescription("Invalid signature on user info cookie.")] UserInfoCookieBadSignature
    }
}