/*******************************************************************************************************************************
 * AK.Login.Tests.CertificateFactoryCertificateProvider
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
using AK.Commons.Security;
using System.ComponentModel.Composition;
using System.Security.Cryptography.X509Certificates;

#endregion

namespace AK.Login.Tests
{
    /// <summary>
    /// ICertificateProvider implementation that uses the test CertificateFactory class.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [ProviderMetadata("TestCertificateFactory"), Export(typeof (ICertificateProvider)),
     PartCreationPolicy(CreationPolicy.Shared)]
    public class CertificateFactoryCertificateProvider : ICertificateProvider
    {
        public CertificateFactoryCertificateProvider()
        {
            this.Certificate = CertificateFactory.Create();
        }

        public X509Certificate2 Certificate { get; private set; }
    }
}