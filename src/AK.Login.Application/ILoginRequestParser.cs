/*******************************************************************************************************************************
 * AK.Login.Application.ILoginRequestParser
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
    /// Parses the given login request.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface ILoginRequestParser
    {
        /// <summary>
        /// Parses the given request for the given stage.
        /// </summary>
        /// <param name="request">HTTP request.</param>
        /// <param name="stage">Login stage.</param>
        /// <returns>Information about the request.</returns>
        LoginRequestInfo Parse(HttpRequestBase request, LoginStage stage);
    }
}