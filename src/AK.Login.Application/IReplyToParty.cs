/*******************************************************************************************************************************
 * AK.Login.Application.IReplyToParty
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

namespace AK.Login.Application
{
    /// <summary>
    /// Contains information about a reply-to party, along with a way to access its ILoginService endpoint.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IReplyToParty
    {
        /// <summary>
        /// The realm for this reply-to-party.
        /// </summary>
        string Realm { get; }

        /// <summary>
        /// The URL to redirect to in order to get to the reply-to-party after login.
        /// </summary>
        string ReplyToAddress { get; }

        /// <summary>
        /// Lets you call the WCF ILoginService endpoint for this reply-to-party.
        /// </summary>
        ILoginService LoginService { get; }
    }
}