/*******************************************************************************************************************************
 * AK.Login.Application.Google.AuthCallbackController
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

using AK.Commons;
using Google.Apis.Auth.OAuth2.Mvc;
using AuthCallbackControllerBase = Google.Apis.Auth.OAuth2.Mvc.Controllers.AuthCallbackController;

#endregion

namespace AK.Login.Application.Google
{
    /// <summary>
    /// Implementation of Google's AuthCallbackController class for our purposes - returns an instance of our
    /// FlowMetadata with the AuthCallback URL configured to point to our AuthCallback endpoint.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class AuthCallbackController : AuthCallbackControllerBase
    {
        public IConfiguration Configuration { get; set; }

        protected override FlowMetadata FlowData
        {
            get
            {
                var authCallback = this.Url.RouteUrl(
                    GoogleConstant.CallbackRouteName,
                    new {action = GoogleConstant.CallbackActionName});

                return new AppFlowMetadata(this.Configuration ?? AppEnvironment.Composer.Resolve<IConfiguration>(),
                                           authCallback);
            }
        }
    }
}