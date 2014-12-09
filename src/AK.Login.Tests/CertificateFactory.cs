/*******************************************************************************************************************************
 * AK.Login.Tests.CertificateFactory
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

using System.IO;
using System.Security.Cryptography.X509Certificates;

#endregion

namespace AK.Login.Tests
{
    /// <summary>
    /// Provides a dummy certificate for testing.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public static class CertificateFactory
    {
        public static X509Certificate2 Create()
        {
            var type = typeof (CertificateFactory);
            var resourceName = type.Namespace + ".TestCertificate.pfx";

            byte[] certificateData;

            using (var outStream = new MemoryStream())
            using (var inStream = type.Assembly.GetManifestResourceStream(resourceName))
            {
                if (inStream != null) inStream.CopyTo(outStream);
                certificateData = outStream.ToArray();
            }

            return new X509Certificate2(certificateData, "b");
        }
    }
}