/*******************************************************************************************************************************
 * AK.Login.Presentation.Global
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
using AK.Commons.Configuration;
using AK.Commons.Security;
using AK.Commons.Services;
using AK.Login.Application;
using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

#endregion

namespace AK.Login.Presentation
{
    /// <summary>
    /// The main web application.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class LoginApplication : HttpApplication
    {
        private static readonly object initializationLock = new object();

        protected void Application_Start(object sender, EventArgs e)
        {
            lock (initializationLock)
            {
                var isInitializedObject = this.Application[Constant.IsInitializedKey];
                var isInitialized = isInitializedObject != null && ((bool) isInitializedObject);

                if (isInitialized) return;

                Initialize(this.Server);

                this.Application[Constant.IsInitializedKey] = true;
            }
        }

        protected void Application_End(object sender, EventArgs e)
        {
            lock (initializationLock)
            {
                var isInitializedObject = this.Application[Constant.IsInitializedKey];
                var isInitialized = isInitializedObject != null && ((bool)isInitializedObject);

                if (!isInitialized) return;

                AppEnvironment.ShutDown();

                this.Application[Constant.IsInitializedKey] = true;
            }            
        }

        private static void Initialize(HttpServerUtility server)
        {
            var configPath = server.MapPath(Constant.WebConfigPath);
            var configMap = new ExeConfigurationFileMap {ExeConfigFilename = configPath};

            var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

            AppEnvironment.Initialize(Constant.ApplicationName, new InitializationOptions
                {
                    ConfigStore = config.GetConfigStore(),
                    EnableLogging = true,
                    GenerateServiceClients = false
                });

            AreaRegistration.RegisterAllAreas();
            GlobalFilters.Filters.Add(new HandleErrorAttribute());

            RouteTable.Routes.IgnoreRoute(Constant.IgnoreRoute);

            RouteTable.Routes.MapRoute(
                GoogleConstant.CallbackRouteName,
                GoogleConstant.CallbackControllerName + "/{action}",
                new {controller = GoogleConstant.CallbackControllerName});

            RouteTable.Routes.MapRoute(
                GeneralConstant.DefaultBaseRouteName, "{action}",
                new {controller = Constant.ControllerName, action = LoginController.ActionNames.Main});

            ServiceCallerFactory.ServiceEndpointAccessor = type =>
                {
                    if (!typeof (ILoginService).IsAssignableFrom(type))
                        throw new NotSupportedException();

                    return AppEnvironment.Composer.Resolve<ILoginServiceEndpointFactory>().Create();
                };
        }

        private static class Constant
        {
            public const string ApplicationName = "AKLogin";
            public const string IsInitializedKey = ApplicationName + ".IsInitialized";
            public const string WebConfigPath = "~/Web.config";
            public const string IgnoreRoute = "{resource}.axd/{*pathInfo}";
            public const string ControllerName = "Login";
        }
    }
}