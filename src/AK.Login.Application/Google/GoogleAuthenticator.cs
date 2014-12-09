/*******************************************************************************************************************************
 * AK.Login.Application.Google.GoogleAuthenticator
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

using AK.Commons.Security;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Plus.v1;
using Google.Apis.Services;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

#endregion

namespace AK.Login.Application.Google
{
    /// <summary>
    /// Handles Google login.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IGoogleAuthenticator
    {
        /// <summary>
        /// Asynchronously calls Google's login API.
        /// </summary>
        /// <param name="controller">Controller that is calling this.</param>
        /// <param name="authCallback">AuthCallback URL.</param>
        /// <param name="cancellationToken">Cancellation token for asynchrousity.</param>
        /// <returns>Task that when done returns login result.</returns>
        Task<GoogleLoginResult> Login(
            Controller controller, string authCallback, CancellationToken cancellationToken);
    }

    /// <summary>
    /// The one and only implementation of IGoogleAuthenticator.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (IGoogleAuthenticator))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GoogleAuthenticator : IGoogleAuthenticator
    {
        private readonly IConfiguration config;

        [ImportingConstructor]
        public GoogleAuthenticator([Import] IConfiguration config)
        {
            this.config = config;
        }

        public async Task<GoogleLoginResult> Login(
            Controller controller, string authCallback, CancellationToken cancellationToken)
        {
            // Execute Google login API.
            //
            var flowMetadata = new AppFlowMetadata(this.config, authCallback);
            var app = new AuthorizationCodeMvcApp(controller, flowMetadata);
            var result = await app.AuthorizeAsync(cancellationToken);

            // If challenged, redirect to the URL provided.
            //
            if (result.Credential == null) return new GoogleLoginResult {RedirectUrl = result.RedirectUri};

            // If authenticated, extract information from G+.

            var plusService =
                new PlusService(new BaseClientService.Initializer
                    {
                        HttpClientInitializer = result.Credential,
                        ApplicationName = config.GoogleApplicationName
                    });

            var request = plusService.People.Get(GoogleConstant.PlusPersonName);
            var me = request.Execute();

            var loginUserInfo = new LoginUserInfo
                {
                    UserName =
                        GoogleConstant.UserIdPrefix +
                        me.Emails.Single(x => x.Type == GoogleConstant.EmailAddressType).Value,
                    DisplayName = me.DisplayName
                };

            return new GoogleLoginResult
                {
                    IsLoggedIn = true,
                    LoginUserInfo = loginUserInfo
                };
        }
    }
}