/*******************************************************************************************************************************
 * AK.Login.Application.Google.AppFlowMetadata
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

using System.Reflection;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using System;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Mvc;

#endregion

namespace AK.Login.Application.Google
{
    /// <summary>
    /// Implementation of Google's FlowMetadata class for our purposes.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class AppFlowMetadata : FlowMetadata
    {
        private static IAuthorizationCodeFlow flow;
        private static readonly object flowLock = new object();

        private readonly string clientSecretPath;
        private readonly string authCallback;

        public AppFlowMetadata(IConfiguration config, string authCallback)
        {
            this.clientSecretPath = config.GoogleClientSecretPath;
            this.authCallback = authCallback;
        }

        public override string AuthCallback
        {
            get { return this.authCallback; }
        }

        public override IAuthorizationCodeFlow Flow
        {
            get { return flow ?? this.CreateFlow(); }
        }

        public override string GetUserId(Controller controller)
        {
            // We just dump this in the session. This whole magic of GetUserId and
            // IDataStore is something I did not want to bother with. We are using Google
            // just for authentication. Beyond that, we have our own way of dealing with
            // the logged-in user info.

            var userId = (string) controller.Session[GoogleConstant.UserSessionKey];
            if (userId == null)
            {
                userId = Guid.NewGuid().ToString();
                controller.Session[GoogleConstant.UserSessionKey] = userId;
            }
            return userId;
        }

        private IAuthorizationCodeFlow CreateFlow()
        {
            ClientSecrets clientSecrets;
            using (var stream = new MemoryStream(GetClientSecretData(this.clientSecretPath)))
                clientSecrets = GoogleClientSecrets.Load(stream).Secrets;

            lock (flowLock)
            {
                if (flow != null) return flow;

                flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = clientSecrets,
                        Scopes = new[] {GoogleConstant.EmailScope, GoogleConstant.OpenIdScope},
                        DataStore = new MemoryDataStore(),
                    });
            }

            return flow;
        }

        private static byte[] GetClientSecretData(string path)
        {
            if (File.Exists(path)) return File.ReadAllBytes(path);

            HttpContext httpContext;
            if ((path.StartsWith("/") || path.StartsWith("~/")) && (httpContext = HttpContext.Current) != null)
            {
                var filePath = httpContext.Server.MapPath(path);
                return File.ReadAllBytes(filePath);
            }

            if (path.StartsWith("http://") || path.StartsWith("https://"))
            {
                using (var webClient = new WebClient())
                    return webClient.DownloadData(path);
            }

            if (!Path.IsPathRooted(path))
            {
                var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (directoryName != null) path = Path.Combine(directoryName, path);
            }

            if (File.Exists(path)) return File.ReadAllBytes(path);

            throw new LoginException(LoginErrorCodes.BadGoogleClientSecretPath);
        }
    }
}