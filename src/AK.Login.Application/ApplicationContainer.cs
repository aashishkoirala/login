/*******************************************************************************************************************************
 * AK.Login.Application.ApplicationContainer
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

using AK.Commons.Logging;
using AK.Login.Application.Facebook;
using AK.Login.Application.Google;
using System;
using System.ComponentModel.Composition;

#endregion

namespace AK.Login.Application
{
    /// <summary>
    /// Provides access to common services throughout the application.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IApplicationContainer
    {
        IAppLogger Logger { get; }
        ILoginRequestHandler RequestHandler { get; }
        ILoginUserInfoCookieManager UserInfoCookieManager { get; }
        ISessionTokenCookieWriter SessionTokenCookieWriter { get; }
        IGoogleAuthenticator GoogleAuthenticator { get; }
        IFacebookAuthenticator FacebookAuthenticator { get; }
    }

    /// <summary>
    /// The one and only implementation of IApplicationContainer.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (IApplicationContainer))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ApplicationContainer : IApplicationContainer
    {
        private readonly Lazy<IAppLogger> logger;
        private readonly Lazy<ILoginRequestHandler> requestHandler;
        private readonly Lazy<ILoginUserInfoCookieManager> userInfoCookieManager;
        private readonly Lazy<ISessionTokenCookieWriter> sessionTokenCookieWriter;
        private readonly Lazy<IGoogleAuthenticator> googleAuthenticator;
        private readonly Lazy<IFacebookAuthenticator> facebookAuthenticator;

        [ImportingConstructor]
        public ApplicationContainer(
            [Import] Lazy<IAppLogger> logger,
            [Import] Lazy<ILoginRequestHandler> requestHandler,
            [Import] Lazy<ILoginUserInfoCookieManager> userInfoCookieManager,
            [Import] Lazy<ISessionTokenCookieWriter> sessionTokenCookieWriter,
            [Import] Lazy<IGoogleAuthenticator> googleAuthenticator,
            [Import] Lazy<IFacebookAuthenticator> facebookAuthenticator)
        {
            this.logger = logger;
            this.requestHandler = requestHandler;
            this.userInfoCookieManager = userInfoCookieManager;
            this.sessionTokenCookieWriter = sessionTokenCookieWriter;
            this.googleAuthenticator = googleAuthenticator;
            this.facebookAuthenticator = facebookAuthenticator;
        }

        public IAppLogger Logger
        {
            get { return this.logger.Value; }
        }

        public ILoginRequestHandler RequestHandler
        {
            get { return this.requestHandler.Value; }
        }

        public ILoginUserInfoCookieManager UserInfoCookieManager
        {
            get { return this.userInfoCookieManager.Value; }
        }

        public ISessionTokenCookieWriter SessionTokenCookieWriter
        {
            get { return this.sessionTokenCookieWriter.Value; }
        }

        public IGoogleAuthenticator GoogleAuthenticator
        {
            get { return this.googleAuthenticator.Value; }
        }

        public IFacebookAuthenticator FacebookAuthenticator
        {
            get { return this.facebookAuthenticator.Value; }
        }
    }
}