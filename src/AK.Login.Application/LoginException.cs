/*******************************************************************************************************************************
 * AK.Login.Application.LoginException
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

using AK.Commons.Exceptions;
using System;
using System.Runtime.Serialization;

#endregion

namespace AK.Login.Application
{
    /// <summary>
    /// Represents an exception within the AK-Login application.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class LoginException : ReasonedException<LoginErrorCodes>
    {
        public LoginException(LoginErrorCodes reason) : base(reason) { }
        public LoginException(LoginErrorCodes reason, string message) : base(reason, message) { }
        public LoginException(LoginErrorCodes reason, Exception innerException) : base(reason, innerException) { }
        public LoginException(LoginErrorCodes reason, string message, Exception innerException)
            : base(reason, message, innerException) { }
        public LoginException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}