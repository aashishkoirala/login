/*******************************************************************************************************************************
 * AK.Login.Application.LoginRequestHandler
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

using AK.Commons.Composition;
using AK.Commons.Logging;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web;
using System.Web.Mvc;

#endregion

namespace AK.Login.Application
{
    /// <summary>
    /// Parses and processes login requests.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface ILoginRequestHandler
    {
        /// <summary>
        /// Parses the given request to figure out what protocol is used, and then
        /// extracts information about the request.
        /// </summary>
        /// <param name="stage">Login stage.</param>
        /// <param name="request">HTTP request.</param>
        /// <returns>Parsed request info.</returns>
        LoginRequestInfo Parse(LoginStage stage, HttpRequestBase request);

        /// <summary>
        /// Processes the request based on the parsed protocol. Meant to be called from
        /// an MVC controller action.
        /// </summary>
        /// <param name="requestInfo">Parsed request info.</param>
        /// <param name="response">HTTP response to write to.</param>
        /// <returns>MVC action result to return.</returns>
        ActionResult Execute(LoginRequestInfo requestInfo, HttpResponseBase response);
    }

    /// <summary>
    /// The one and only implementation of ILoginRequestHandler.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (ILoginRequestHandler))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class LoginRequestHandler : ILoginRequestHandler
    {
        private readonly IComposer composer;
        private readonly IAppLogger logger;
        private readonly IEnumerable<ILoginRequestParser> parsers;

        [ImportingConstructor]
        public LoginRequestHandler(
            [Import] IComposer composer,
            [Import] IAppLogger logger,
            [ImportMany] IEnumerable<ILoginRequestParser> parsers)
        {
            this.composer = composer;
            this.logger = logger;
            this.parsers = parsers;
        }

        public LoginRequestInfo Parse(LoginStage stage, HttpRequestBase request)
        {
            // The first parser that can parse the request assigns its protocol.

            var requestInfo = this.parsers
                                  .Select(x => x.Parse(request, stage))
                                  .FirstOrDefault(x => x.Parsed);

            if (requestInfo == null)
            {
                this.logger.Error(
                    string.Format("Could not find any parser that could parse this request: {0}", request.Url));

                return null;
            }

            requestInfo.Stage = stage;

            this.logger.Information(string.Format("Parsed request as stage {0} and protocol {1}.",
                                                  requestInfo.Stage, requestInfo.Protocol));

            return requestInfo;
        }

        public ActionResult Execute(LoginRequestInfo requestInfo, HttpResponseBase response)
        {
            this.logger.Information(string.Format("Executing request (Stage = {0}, Protocol = {1}) {2}...",
                                                  requestInfo.Stage, requestInfo.Protocol, requestInfo.Request.Url));

            var processor = this.composer.Resolve<ILoginRequestProcessor>(requestInfo.Protocol);
            processor.Process(requestInfo, response);

            return new EmptyResult();
        }
    }
}