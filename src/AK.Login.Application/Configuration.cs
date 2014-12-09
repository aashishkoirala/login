/*******************************************************************************************************************************
 * AK.Login.Application.Configuration
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

using AK.Commons.Configuration;
using AK.Commons.Logging;
using System;
using System.ComponentModel.Composition;

#endregion

namespace AK.Login.Application
{
    /// <summary>
    /// Provides access to application configuration settings.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IConfiguration
    {
        /// <summary>
        /// Whether we're in "Relaxed Security Mode" that has loose rules for certificate validation.
        /// </summary>
        bool IsRelaxedSecurityMode { get; }

        /// <summary>
        /// The developer's application key for Facebook login.
        /// </summary>
        string FacebookAppKey { get; }

        /// <summary>
        /// The client-secrets file path for Google login.
        /// </summary>
        string GoogleClientSecretPath { get; }

        /// <summary>
        /// The application name for Google login.
        /// </summary>
        string GoogleApplicationName { get; }

        /// <summary>
        /// Whether to require SSL.
        /// </summary>
        bool RequireSsl { get; }

        /// <summary>
        /// Base URL as configured. If not set, this can be assigned from request.
        /// </summary>
        string BaseUrl { get; set; }

        /// <summary>
        /// Path of the Login endpoint relative to the base URL.
        /// </summary>
        string LoginPath { get; }

        /// <summary>
        /// Gets the Reply-To party's address URL based on the given realm.
        /// </summary>
        /// <param name="realm">Realm.</param>
        /// <returns>Reply-TO party address URL.</returns>
        string GetReplyToAddress(string realm);

        /// <summary>
        /// Gets the ILoginService WCF endpoint URL based on the given realm.
        /// </summary>
        /// <param name="realm">Realm.</param>
        /// <returns>The ILoginService WCF endpoint URL</returns>
        string GetLoginServiceUrl(string realm);
    }

    /// <summary>
    /// The one and only implementation of IConfiguration.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (IConfiguration)), PartCreationPolicy(CreationPolicy.Shared)]
    public class Configuration : IConfiguration
    {
        private readonly IAppConfig config;
        private readonly IAppLogger logger;

        [ImportingConstructor]
        public Configuration([Import] IAppConfig config, [Import] IAppLogger logger)
        {
            this.config = config;
            this.logger = logger;

            this.LoadProperties();
        }

        public bool IsRelaxedSecurityMode { get; private set; }
        public string FacebookAppKey { get; private set; }
        public string GoogleClientSecretPath { get; private set; }
        public string GoogleApplicationName { get; private set; }
        public bool RequireSsl { get; private set; }
        public string BaseUrl { get; set; }
        public string LoginPath { get; private set; }

        public string GetReplyToAddress(string realm)
        {
            var configKey = string.Format(ConfigurationKey.ReplyToAddressFormat, realm);
            var value = this.config.Get<string>(configKey);

            var logMessage = string.Format("ReplyToAddress for {0} = {1}", realm, value);
            this.logger.Information(logMessage);

            return value;
        }

        public string GetLoginServiceUrl(string realm)
        {
            var configKey = string.Format(ConfigurationKey.LoginServiceUrlFormat, realm);
            var value = this.config.Get<string>(configKey);

            var logMessage = string.Format("LoginServiceUrl for {0} = {1}", realm, value);
            this.logger.Information(logMessage);

            return value;
        }

        private void LoadProperties()
        {
            this.IsRelaxedSecurityMode = this.config.Get(
                ConfigurationKey.RelaxedSecurityMode, ConfigurationDefault.RelaxedSecurityMode);
            this.FacebookAppKey = this.config.Get<string>(ConfigurationKey.FacebookAppKey);
            this.GoogleClientSecretPath = this.config.Get<string>(ConfigurationKey.GoogleClientSecretPath);
            this.GoogleApplicationName = this.config.Get<string>(ConfigurationKey.GoogleApplicationName);
            this.RequireSsl = this.config.Get(ConfigurationKey.RequireSsl, ConfigurationDefault.RequireSsl);
            this.BaseUrl = this.config.Get(ConfigurationKey.BaseUrl, string.Empty);
            this.LoginPath = this.config.Get(ConfigurationKey.LoginPath, ConfigurationDefault.LoginPath);

            const string logMessageFormat =
                "Loaded configuration as follows:{0}IsRelaxedSecurityMode = {1}{0}FacebookAppKey = {2}{0}" +
                "GoogleClientSecretPath = {3}{0}GoogleApplicationName = {4}{0}RequireSsl = {5}{0}BaseUrl = {6}{0}" +
                "LoginPath = {7}";

            var logMessage = string.Format(logMessageFormat, Environment.NewLine, this.IsRelaxedSecurityMode,
                                           this.FacebookAppKey, this.GoogleClientSecretPath, this.GoogleApplicationName,
                                           this.RequireSsl, this.BaseUrl, this.LoginPath);

            this.logger.Information(logMessage);
        }
    }
}