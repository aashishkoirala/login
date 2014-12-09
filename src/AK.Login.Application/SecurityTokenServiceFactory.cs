/*******************************************************************************************************************************
 * AK.Login.Application.SecurityTokenServiceFactory
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

using System.ComponentModel.Composition;
using System.IdentityModel;
using System.IdentityModel.Configuration;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

#endregion

namespace AK.Login.Application
{
    /// <summary>
    /// Creates AK-Login specific instances of SecurityTokenService.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface ISecurityTokenServiceFactory
    {
        /// <summary>
        /// Creates an AK-Login specific instance of SecurityTokenService.
        /// </summary>
        /// <param name="appliesTo">The Applies-To URL.</param>
        /// <param name="replyTo">The Reply-To URL.</param>
        /// <returns>SecurityTokenService instance.</returns>
        SecurityTokenService Create(string appliesTo, string replyTo);
    }

    /// <summary>
    /// The one and only implementation of ISecurityTokenServiceFactory.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (ISecurityTokenServiceFactory)), PartCreationPolicy(CreationPolicy.Shared)]
    public class SecurityTokenServiceFactory : ISecurityTokenServiceFactory
    {
        private readonly X509Certificate2 certificate;

        [ImportingConstructor]
        public SecurityTokenServiceFactory([Import] ICertificateStore certificateStore)
        {
            this.certificate = certificateStore.Certificate;
        }

        public SecurityTokenService Create(string appliesTo, string replyTo)
        {
            var config = new AkSecurityTokenServiceConfiguration(this.certificate);

            return new AkSecurityTokenService(config, appliesTo, replyTo);
        }

        private class AkSecurityTokenServiceConfiguration : SecurityTokenServiceConfiguration
        {
            public AkSecurityTokenServiceConfiguration(X509Certificate2 certificate)
            {
                this.SecurityTokenService = typeof (AkSecurityTokenService);
                this.ServiceCertificate = certificate;
                this.SigningCredentials = new X509SigningCredentials(certificate);
                this.TokenIssuerName = GeneralConstant.TokenIssuerName;
            }
        }

        private class AkSecurityTokenService : SecurityTokenService
        {
            private readonly string appliesTo;
            private readonly string replyTo;

            public AkSecurityTokenService(
                SecurityTokenServiceConfiguration configuration, string appliesTo, string replyTo)
                : base(configuration)
            {
                this.appliesTo = appliesTo;
                this.replyTo = replyTo;
            }

            protected override Scope GetScope(ClaimsPrincipal principal, RequestSecurityToken request)
            {
                return new Scope(this.appliesTo, this.SecurityTokenServiceConfiguration.SigningCredentials)
                    {
                        ReplyToAddress = this.replyTo,
                        TokenEncryptionRequired = false
                    };
            }

            protected override ClaimsIdentity GetOutputClaimsIdentity(
                ClaimsPrincipal principal, RequestSecurityToken request, Scope scope)
            {
                return principal.Identities.Single();
            }
        }
    }
}