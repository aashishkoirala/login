/*******************************************************************************************************************************
 * AK.Login.Application.Constants
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

namespace AK.Login.Application
{
    /// <summary>
    /// Configuration key names.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public static class ConfigurationKey
    {
        public const string RelaxedSecurityMode = "IsRelaxedSecurityMode";
        public const string FacebookAppKey = "Facebook.AppKey";
        public const string GoogleClientSecretPath = "Google.ClientSecretPath";
        public const string GoogleApplicationName = "Google.ApplicationName";
        public const string RequireSsl = "RequireSsl";
        public const string BaseUrl = "BaseUrl";
        public const string LoginPath = "LoginPath";
        public const string ReplyToAddressFormat = "ReplyToParty.{0}.ReplyToAddress";
        public const string LoginServiceUrlFormat = "ReplyToParty.{0}.LoginServiceUrl";
    }

    /// <summary>
    /// Default values of configuration settings.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public static class ConfigurationDefault
    {
        public const bool RelaxedSecurityMode = false;
        public const bool RequireSsl = true;
        public const string LoginPath = "Login";
    }

    /// <summary>
    /// Constants pertaining to Facebook login.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public static class FacebookConstant
    {
        public const string UserIdPrefix = "FACEBOOK:";
    }

    /// <summary>
    /// Constaints pertaining to Google login.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public static class GoogleConstant
    {
        public const string UserSessionKey = "Google.User";
        public const string EmailScope = "email";
        public const string OpenIdScope = "openid";
        public const string CallbackRouteName = "AuthCallback";
        public const string CallbackControllerName = "AuthCallback";
        public const string CallbackActionName = "IndexAsync";
        public const string UserIdPrefix = "GOOGLE:";
        public const string PlusPersonName = "me";
        public const string EmailAddressType = "account";
    }

    /// <summary>
    /// Login protocol names.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public static class ProtocolName
    {
        public const string WsFed = "WSFed";
    }

    /// <summary>
    /// General purpose application level constants.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public static class GeneralConstant
    {
        public const string DefaultBaseRouteName = "Default";
        public const string DummyAddress = "http://localhost";
        public const string UserInfoCookieName = "AKLogin-UserInfo";
        public const string TokenIssuerName = "urn:AKLogin";
        public const string CookieHandlerPath = "/";
        public const string CookieHandlerName = "FedAuth";
    }

    namespace WsFed
    {
        /// <summary>
        /// Constants pertaining to WS-Fed protocol.
        /// </summary>
        /// <author>Aashish Koirala</author>
        public static class WsFedConstant
        {
            public const string InitialUrlCookieName = "AKLogin-WsFed-InitialUrl";
            public const string HttpPost = "POST";
            public const string ActionParam = "wa";
            public const string SignInAction = "wsignin1.0";
            public const string SignOutAction = "wsignout1.0";
        }
    }
}