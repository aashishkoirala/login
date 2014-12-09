/*******************************************************************************************************************************
 * AK.Login.Tests.Unit.Application.CertificateStoreTests
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

using AK.Login.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace AK.Login.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for CertificateStore.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class CertificateStoreTests
    {
        [TestMethod, TestCategory("Unit")]
        public void CertificateStore_Works()
        {
            var expectedCertificate = CertificateFactory.Create();

            var certificateStore = new CertificateStore
                {
                    CertificateAccessor = () => expectedCertificate
                };

            var actualCertificate = certificateStore.Certificate;
            Assert.AreSame(expectedCertificate, actualCertificate);
        }
    }
}