/*******************************************************************************************************************************
 * AK.Login.Presentation.LoginController
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
using AK.Commons.Security;
using AK.Commons.Web.Filters;
using AK.Login.Application;
using AK.Login.Application.Facebook;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

#endregion

namespace AK.Login.Presentation
{
    /// <summary>
    /// MVC controller that handles all endpoints for the STS.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [LoginFilter, MvcLogExceptionFilter]
    public class LoginController : Controller
    {
        private IApplicationContainer container;

        /// <summary>
        /// Assign this to override the IApplicationContainer instance for testing.
        /// </summary>
        public IApplicationContainer ContainerOverride { get; set; }

        /// <summary>
        /// Assign this to override the Request for testing.
        /// </summary>
        public HttpRequestBase RequestOverride { get; set; }

        /// <summary>
        /// Assign this to override the Response for testing.
        /// </summary>
        public HttpResponseBase ResponseOverride { get; set; }

        /// <summary>
        /// Assign this to override the Google Authcallback value for testing.
        /// </summary>
        public string GoogleAuthCallbackOverride { get; set; }

        private IApplicationContainer Container
        {
            get
            {
                return this.ContainerOverride ??
                       this.container ?? (this.container = AppEnvironment.Composer.Resolve<IApplicationContainer>());
            }
        }

        private HttpRequestBase RequestOrOverride
        {
            get { return this.RequestOverride ?? this.Request; }
        }

        private HttpResponseBase ResponseOrOverride
        {
            get { return this.ResponseOverride ?? this.Response; }
        }

        public ActionResult Main()
        {
            this.Container.Logger.Verbose("Entered action Main.");

            var requestInfo = this.Container.RequestHandler.Parse(LoginStage.Initial, this.RequestOrOverride);
            return (requestInfo != null && requestInfo.Parsed)
                       ? this.Container.RequestHandler.Execute(requestInfo, this.ResponseOrOverride)
                       : this.View();
        }

        public ActionResult Login()
        {
            this.Container.Logger.Verbose("Entered action Login.");

            var requestInfo = this.Container.RequestHandler.Parse(LoginStage.ShowLoginPage, this.RequestOrOverride);

            this.Container.Logger.Information("Getting splash information from relying party...");
            var loginSplashInfo = requestInfo.ReplyToParty.LoginService.GetLoginSplashInfo();

            this.Container.RequestHandler.Execute(requestInfo, this.ResponseOrOverride);

            var domain = this.RequestOrOverride.Url != null ? this.RequestOrOverride.Url.Host : string.Empty;
            var model = new LoginViewModel
                {
                    Splash = loginSplashInfo,
                    Domain = domain,
                    ShowIeWarning = this.RequestOrOverride.Browser.Browser == "IE"
                };

            return this.View(model);
        }

        public ActionResult Tos()
        {
            this.Container.Logger.Verbose("Entered action Tos.");

            return this.View();
        }

        public ActionResult Privacy()
        {
            this.Container.Logger.Verbose("Entered action Privacy.");

            return this.View();
        }

        [HttpPost]
        public ActionResult Facebook(FacebookLoginRequest request)
        {
            this.Container.Logger.Verbose(
                string.Format("Entered action Facebook - UserId: {0}, SignedRequest: {1}.",
                              request.UserId, request.SignedRequest));

            var result = this.Container.FacebookAuthenticator.Login(request);
            if (!result.IsLoggedIn)
            {
                this.Container.Logger.Error("Facebook login failed.");
                return new HttpUnauthorizedResult();
            }

            this.Container.Logger.Information(
                string.Format("Facebook authenticated as {0}.", result.LoginUserInfo.UserId));

            this.Container.UserInfoCookieManager.Write(result.LoginUserInfo, this.ResponseOrOverride);
            return this.RedirectToAction(ActionNames.Authenticated);
        }

        public async Task<ActionResult> Google(CancellationToken cancellationToken)
        {
            this.Container.Logger.Verbose("Entered action Google.");

            var authCallback = this.GoogleAuthCallbackOverride ??
                               this.Url.RouteUrl(GoogleConstant.CallbackRouteName,
                                                 new {action = GoogleConstant.CallbackActionName});

            var result = await this.Container.GoogleAuthenticator.Login(this, authCallback, cancellationToken);
            if (!result.IsLoggedIn)
            {
                this.Container.Logger.Information(string.Format(
                    "Challenge received from Google, redirecting to {0}...", result.RedirectUrl));

                return this.Redirect(result.RedirectUrl);
            }

            this.Container.Logger.Information(
                string.Format("Google authenticated as {0}.", result.LoginUserInfo.UserId));

            this.Container.UserInfoCookieManager.Write(result.LoginUserInfo, this.ResponseOrOverride);
            return this.RedirectToAction(ActionNames.Authenticated);
        }

        public ActionResult Authenticated()
        {
            this.Container.Logger.Verbose("Entered action Authenticated.");

            var requestInfo = this.Container.RequestHandler.Parse(LoginStage.Authenticated, this.RequestOrOverride);

            var loginUserInfo = this.Container.UserInfoCookieManager.Read(this.RequestOrOverride);
            var userName = loginUserInfo.UserName;

            if (string.IsNullOrWhiteSpace(userName))
            {
                this.Container.Logger.Error("User info from cookie has no user name.");
                return new HttpUnauthorizedResult();
            }

            this.Container.Logger.Information("Getting user information from relying party...");
            loginUserInfo = requestInfo.ReplyToParty.LoginService.GetUser(userName);

            if (!loginUserInfo.UserExists)
            {
                this.Container.Logger.Information("User does not exist, prompt for display name.");
                return this.InterceptForUserDetails(requestInfo, loginUserInfo);
            }

            this.Container.UserInfoCookieManager.Clear(this.ResponseOrOverride);

            return this.WriteSessionTokenCookieAndRedirect(loginUserInfo, requestInfo);
        }

        [HttpPost]
        public ActionResult UserDetails(LoginViewModel model)
        {
            this.Container.Logger.Verbose("Entered action UserDetails.");

            if (string.IsNullOrWhiteSpace(model.User.DisplayName))
            {
                model.User.DisplayName = "Must enter something!";
                return this.View(model);
            }

            var loginUserInfo = this.Container.UserInfoCookieManager.Read(this.RequestOrOverride);
            loginUserInfo.DisplayName = model.User.DisplayName;

            var requestInfo = this.Container.RequestHandler.Parse(LoginStage.Authenticated, this.RequestOrOverride);

            this.Container.Logger.Information(
                string.Format("Creating user {0} on relying party...", loginUserInfo.UserId));

            loginUserInfo = requestInfo.ReplyToParty.LoginService.CreateUser(loginUserInfo);

            return this.WriteSessionTokenCookieAndRedirect(loginUserInfo, requestInfo);
        }

        public ActionResult Error()
        {
            this.Container.Logger.Verbose("Entered action Error.");

            return this.View();
        }

        private ActionResult InterceptForUserDetails(LoginRequestInfo requestInfo, LoginUserInfo loginUserInfo)
        {
            this.Container.Logger.Verbose("Entered action InterceptForUserDetails.");

            this.Container.Logger.Information("Getting splash information from relying party...");
            var loginService = requestInfo.ReplyToParty.LoginService;
            var loginSplashInfo = loginService.GetLoginSplashInfo();

            var domain = this.RequestOrOverride.Url != null ? this.RequestOrOverride.Url.Host : string.Empty;
            var model = new LoginViewModel {Splash = loginSplashInfo, User = loginUserInfo, Domain = domain};

            return this.View(ActionNames.UserDetails, model);
        }

        private ActionResult WriteSessionTokenCookieAndRedirect(
            LoginUserInfo loginUserInfo, LoginRequestInfo requestInfo)
        {
            this.Container.UserInfoCookieManager.Clear(this.ResponseOrOverride);

            this.Container.SessionTokenCookieWriter.Write(
                loginUserInfo.UserId, loginUserInfo.UserName, loginUserInfo.DisplayName);

            return this.Container.RequestHandler.Execute(requestInfo, this.ResponseOrOverride);
        }

        public static class ActionNames
        {
            public const string Main = "Main";
            public const string Authenticated = "Authenticated";
            public const string UserDetails = "UserDetails";
        }
    }
}