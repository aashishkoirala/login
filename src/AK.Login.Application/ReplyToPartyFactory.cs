/*******************************************************************************************************************************
 * AK.Login.Application.LoginConfiguration
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
using AK.Commons.Security;
using AK.Commons.Services;
using System;
using System.ComponentModel.Composition;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;

#endregion

namespace AK.Login.Application
{
    /// <summary>
    /// Creates objects with information about a Reply-To party based on realm.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IReplyToPartyFactory
    {
        /// <summary>
        /// Creates a Reply-To party information structure for the given realm.
        /// </summary>
        /// <param name="realm">Realm.</param>
        /// <returns>Reply-To party information.</returns>
        IReplyToParty Create(string realm);

        /// <summary>
        /// Set this to override the logic that gives you the ILoginService instance
        /// for testing. The delegate should accept a string which represents the
        /// realm and return an instance of ILoginService. Do NOT assign this in
        /// production code.
        /// </summary>
        Func<string, ILoginService> LoginServiceOverride { get; set; }
    }

    /// <summary>
    /// The one and only implementation of IReplyToPartyFactory.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (IReplyToPartyFactory)), PartCreationPolicy(CreationPolicy.Shared)]
    public class ReplyToPartyFactory : IReplyToPartyFactory
    {
        private readonly IConfiguration config;
        private readonly IAppLogger logger;
        private readonly X509Certificate2 certificate;

        [ImportingConstructor]
        public ReplyToPartyFactory(
            [Import] IConfiguration config,
            [Import] IAppLogger logger,
            [Import] ICertificateStore certificateStore)
        {
            this.config = config;
            this.logger = logger;
            this.certificate = certificateStore.Certificate;
        }

        public Func<string, ILoginService> LoginServiceOverride { get; set; }

        public IReplyToParty Create(string realm)
        {
            var replyToAddress = this.config.GetReplyToAddress(realm);
            var loginServiceUrl = this.config.GetLoginServiceUrl(realm);

            this.logger.Information(
                string.Format("Returning ReplyToParty for realm {0} with ReplyToAddress {1}, LoginServiceUrl {2}...",
                              realm, replyToAddress, loginServiceUrl));

            var loginService = this.LoginServiceOverride != null
                                   ? this.LoginServiceOverride(realm)
                                   : new ReplyToPartyLoginService(loginServiceUrl, this.certificate);

            return new ReplyToParty {Realm = realm, ReplyToAddress = replyToAddress, LoginService = loginService};
        }

        private class ReplyToParty : IReplyToParty
        {
            public string Realm { get; set; }

            public string ReplyToAddress { get; set; }

            public ILoginService LoginService { get; set; }
        }

        private class ReplyToPartyLoginService : ILoginService
        {
            private readonly IServiceCaller<ILoginService> caller;

            public ReplyToPartyLoginService(string loginServiceUrl, X509Certificate2 certificate)
            {
                var address = new EndpointAddress(
                    new Uri(loginServiceUrl), new X509CertificateEndpointIdentity(certificate));
                this.caller = ServiceCallerFactory.Create<ILoginService>(address);
            }

            public LoginSplashInfo GetLoginSplashInfo()
            {
                return this.caller.Call(x => x.GetLoginSplashInfo());
            }

            public LoginUserInfo GetUser(string userName)
            {
                return this.caller.Call(x => x.GetUser(userName));
            }

            public LoginUserInfo CreateUser(LoginUserInfo userInfo)
            {
                return this.caller.Call(x => x.CreateUser(userInfo));
            }
        }
    }
}