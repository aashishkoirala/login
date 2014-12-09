/*******************************************************************************************************************************
 * AK.Login.Application.LoginRequestInfo
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

using System.Web;

namespace AK.Login.Application
{
    /// <summary>
    /// Contains information about a login request that is assigned after parsing.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class LoginRequestInfo
    {
        /// <summary>
        /// Whether the request was properly parsed.
        /// </summary>
        public bool Parsed { get; set; }

        /// <summary>
        /// Type of login request (i.e. sign-in, sign-out, etc.)
        /// </summary>
        public LoginRequestType Type { get; set; }

        /// <summary>
        /// The login stage.
        /// </summary>
        public LoginStage Stage { get; set; }

        /// <summary>
        /// The original HTTP request object.
        /// </summary>
        public HttpRequestBase Request { get; set; }

        /// <summary>
        /// The protocol-specific parsed message.
        /// </summary>
        public object Message { get; set; }

        /// <summary>
        /// The protocol name (WS-Fed, SAML, etc.)
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// The realm of the relying party as specified in the request.
        /// </summary>
        public string Realm { get; set; }

        /// <summary>
        /// Information about the relying/reply-to party as extracted based on the realm.
        /// </summary>
        public IReplyToParty ReplyToParty { get; set; }
    }
}