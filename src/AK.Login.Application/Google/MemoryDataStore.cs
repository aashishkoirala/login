/*******************************************************************************************************************************
 * AK.Login.Application.Google.MemoryDataStore
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

using Google.Apis.Util.Store;
using System.Collections.Generic;
using System.Threading.Tasks;

#endregion

namespace AK.Login.Application.Google
{
    /// <summary>
    /// Implementation of Google's IDataStore for our purposes. We just use memory (i.e. a static dictionary) because
    /// we don't really rely on all this magic stuff in our case.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class MemoryDataStore : IDataStore
    {
        private static readonly IDictionary<string, object> store = new Dictionary<string, object>();

        public async Task StoreAsync<T>(string key, T value)
        {
            await Task.Factory.StartNew(() => store[key] = value);
        }

        public async Task DeleteAsync<T>(string key)
        {
            await Task.Factory.StartNew(() => store.Remove(key));
        }

        public async Task<T> GetAsync<T>(string key)
        {
            return await Task.Factory.StartNew(() =>
                {
                    object value;
                    if (store.TryGetValue(key, out value))
                        return (T) value;
                    return default(T);
                });
        }

        public async Task ClearAsync()
        {
            await Task.Factory.StartNew(() => store.Clear());
        }
    }
}