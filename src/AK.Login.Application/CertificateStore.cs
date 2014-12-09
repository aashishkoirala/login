/*******************************************************************************************************************************
 * AK.Login.Application.CertificateStore
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
using System;
using System.ComponentModel.Composition;
using System.Security.Cryptography.X509Certificates;

#endregion

namespace AK.Login.Application
{
    /// <summary>
    /// Provides access to a certificate to use.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface ICertificateStore
    {
        /// <summary>
        /// Set this to a custom accessor to override the default behavior for testing.
        /// </summary>
        Func<X509Certificate2> CertificateAccessor { get; set; }

        /// <summary>
        /// Gets the certificate.
        /// </summary>
        X509Certificate2 Certificate { get; }
    }

    /// <summary>
    /// The one and only implementation of ICertificateStore.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (ICertificateStore)), PartCreationPolicy(CreationPolicy.Shared)]
    public class CertificateStore : ICertificateStore
    {
        public Func<X509Certificate2> CertificateAccessor { get; set; }

        public X509Certificate2 Certificate
        {
            get
            {
                return this.CertificateAccessor != null
                           ? this.CertificateAccessor()
                           : AppEnvironment.CertificateProvider.Certificate;
            }
        }
    }
}