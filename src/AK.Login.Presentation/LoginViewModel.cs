/*******************************************************************************************************************************
 * AK.Login.Presentation.LoginViewModel
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

namespace AK.Login.Presentation
{
    /// <summary>
    /// View model that serves the login page.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class LoginViewModel
    {
        /// <summary>
        /// The domain that the application is currently hosted at.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Whether to show warning related to the browser being IE.
        /// </summary>
        public bool ShowIeWarning { get; set; }

        /// <summary>
        /// Splash screen information obtained from relying party.
        /// </summary>
        public LoginSplashInfo Splash { get; set; }

        /// <summary>
        /// User information, if applicable.
        /// </summary>
        public LoginUserInfo User { get; set; }
    }
}