/*******************************************************************************************************************************
 * AK.Login.Application.LoginFilterAttribute
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
using AK.Commons.Logging;
using System.Web.Mvc;

#endregion

namespace AK.Login.Application
{
    /// <summary>
    /// MVC action filter to be applied to the login endpoint controller actions.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class LoginFilterAttribute : ActionFilterAttribute
    {
        private readonly string baseRouteName;

        /// <summary>
        /// Creates a new instance with the given parameters.
        /// </summary>
        /// <param name="baseRouteName">The name of the base route.</param>
        public LoginFilterAttribute(string baseRouteName = GeneralConstant.DefaultBaseRouteName)
        {
            this.baseRouteName = baseRouteName;
        }

        /// <summary>
        /// Set this to override the instance of IConfiguration to use for testing.
        /// </summary>
        public IConfiguration Configuration { get; set; }

        /// <summary>
        /// Set this to override the instance of IAppLogger to use for testing.
        /// </summary>
        public IAppLogger Logger { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // If a BaseUrl is configured, we use that. Otherwise, we extract it from
            // the request. The BaseUrl needs to be configured in certain situations such
            // as when hosting with AppHarbor because of their load-balancing approach.
            // We can't rely on the request for URLs. Even though we set the ASP.NET setting
            // for that thing, it does not work for this particular case.

            var configuration = this.Configuration ?? AppEnvironment.Composer.Resolve<IConfiguration>();
            if (!string.IsNullOrWhiteSpace(configuration.BaseUrl)) return;

            var url = filterContext.RequestContext.HttpContext.Request.Url;
            if (url == null) return;

            url = url.GetSchemeAndHost();
            var urlHelper = new UrlHelper(filterContext.RequestContext);
            url = url.Append(urlHelper.RouteUrl(this.baseRouteName));

            configuration.BaseUrl = url.ToString();

            var logger = this.Logger ?? AppEnvironment.Logger;
            logger.Information(string.Format("BaseUrl set to {0} from request.", configuration.BaseUrl));
        }
    }
}