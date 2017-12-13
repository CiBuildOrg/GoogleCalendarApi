using Google.Apis.Util.Store;
using Mvc.Server.Database;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using Google.Apis.Json;
using MvcServer.Entities;

namespace Mvc.Server.Services.Utils
{
    public class DatabaseDataStore : IDataStore
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public DatabaseDataStore(ApplicationDbContext context)
        {
            _applicationDbContext = context;
        }

        /// <summary>
        /// Stores the given value for the given key. It creates a new file (named <see cref="GenerateStoredKey"/>) in 
        /// <see cref="FolderPath"/>.
        /// </summary>
        /// <typeparam name="T">The type to store in the data store</typeparam>
        /// <param name="key">The key</param>
        /// <param name="value">The value to store in the data store</param>
        public async Task StoreAsync<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            var serialized = NewtonsoftJsonSerializer.Instance.Serialize(value);
            var entity = await _applicationDbContext.GoogleUsers.SingleOrDefaultAsync(x => x.UserName == key);

            if (entity != null)
            {
                entity.RefreshToken = serialized;
                _applicationDbContext.Update(entity);
            }
            else
            {
                var insert = new GoogleUser
                {
                    Id = Guid.NewGuid(),
                    UserName = key,
                    RefreshToken = serialized
                };

                await _applicationDbContext.AddAsync(insert);
            }

            await _applicationDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes the given key. It deletes the <see cref="GenerateStoredKey"/> named file in <see cref="FolderPath"/>.
        /// </summary>
        /// <param name="key">The key to delete from the data store</param>
        public async Task DeleteAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            var entity = await _applicationDbContext.GoogleUsers.SingleOrDefaultAsync(x => x.UserName == key);
            if(entity != null)
            {
                _applicationDbContext.Remove(entity);
                await _applicationDbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Returns the stored value for the given key or <c>null</c> if the matching file (<see cref="GenerateStoredKey"/>
        /// in <see cref="FolderPath"/> doesn't exist.
        /// </summary>
        /// <typeparam name="T">The type to retrieve</typeparam>
        /// <param name="key">The key to retrieve from the data store</param>
        /// <returns>The stored object</returns>
        public async Task<T> GetAsync<T>(string key)
        {
            //Key is the user string sent with AuthorizeAsync
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            var entity = await _applicationDbContext.GoogleUsers.SingleOrDefaultAsync(x => x.UserName == key);

            if (entity != null)
            {
                // we have it we use that.
                tcs.SetResult(NewtonsoftJsonSerializer.Instance.Deserialize<T>(entity.RefreshToken));
            }
            else
            {
                tcs.SetResult(default(T));
            }

            return await tcs.Task;
        }

        /// <summary>
        /// Clears all values in the data store. This method deletes all files in <see cref="FolderPath"/>.
        /// </summary>
        public async Task ClearAsync()
        {
            _applicationDbContext.GoogleUsers.RemoveRange(_applicationDbContext.GoogleUsers);
            await _applicationDbContext.SaveChangesAsync();
        }
    }
}
