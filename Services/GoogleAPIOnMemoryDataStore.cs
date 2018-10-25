using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Util.Store;

namespace CalMan.Services
{
    public class GoogleAPIOnMemoryDataStore : IDataStore
    {
        private IDictionary<string, dynamic> Data;
        public GoogleAPIOnMemoryDataStore(IDictionary<string, dynamic> initialDict)
        {
            this.Data = initialDict;
        }

        public Task StoreAsync<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key must have a value");
            }

            this.Data[key] = value;

            return Task.CompletedTask;
        }

        public Task DeleteAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key must have a value");
            }

            this.Data.Remove(key);

            return Task.CompletedTask;
        }

        public Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key must have a value");
            }

            if (this.Data.TryGetValue(key, out var val))
            {
                try
                {
                    return Task.FromResult((T)val);
                }
                catch (Exception)
                {
                    return Task.FromResult(default(T));
                }
            }
            else
            {
                return Task.FromResult(default(T));
            }
        }

        public Task ClearAsync()
        {
            this.Data.Clear();

            return Task.CompletedTask;
        }
    }
}