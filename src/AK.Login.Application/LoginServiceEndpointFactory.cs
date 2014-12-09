/*******************************************************************************************************************************
 * AK.Login.Application.LoginServiceEndpointFactory
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

using AK.Commons.Security;
using System.ComponentModel.Composition;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;

#endregion

namespace AK.Login.Application
{
    /// <summary>
    /// Creates WCF service endpoint for ILoginService.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface ILoginServiceEndpointFactory
    {
        /// <summary>
        /// Creates a new WCF service endpoint for ILoginService using a dummy URL. Therefore the caller must set
        /// the actual URL to use.
        /// </summary>
        /// <returns></returns>
        ServiceEndpoint Create();
    }

    /// <summary>
    /// The one and only implementation of ILoginServiceEndpointFactory.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (ILoginServiceEndpointFactory)), PartCreationPolicy(CreationPolicy.Shared)]
    public class LoginServiceEndpointFactory : ILoginServiceEndpointFactory
    {
        private readonly IConfiguration configuration;
        private readonly X509Certificate2 certificate;

        [ImportingConstructor]
        public LoginServiceEndpointFactory(
            [Import] IConfiguration configuration,
            [Import] ICertificateStore certificateStore)
        {
            this.configuration = configuration;
            this.certificate = certificateStore.Certificate;
        }

        public ServiceEndpoint Create()
        {
            var binding = new WSHttpBinding(SecurityMode.Message);
            binding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;

            if (this.configuration.IsRelaxedSecurityMode)
            {
                binding.Security.Message.NegotiateServiceCredential = false;
                binding.Security.Message.EstablishSecurityContext = false;
            }

            var address = new EndpointAddress(GeneralConstant.DummyAddress);
            var contract = ContractDescription.GetContract(typeof (ILoginService));
            var endpoint = new ServiceEndpoint(contract, binding, address);

            endpoint.Behaviors.Remove<ClientCredentials>();

            var credentials = new ClientCredentials();
            credentials.ClientCertificate.Certificate = this.certificate;
            credentials.ServiceCertificate.DefaultCertificate = this.certificate;

            if (this.configuration.IsRelaxedSecurityMode)
            {
                credentials.ServiceCertificate.Authentication.CertificateValidationMode =
                    X509CertificateValidationMode.None;
                credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;
            }

            endpoint.Behaviors.Add(credentials);

            return endpoint;
        }
    }
}