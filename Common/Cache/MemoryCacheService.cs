namespace Common.Cache;

using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

public class MemoryCacheService : ICacheService
{
  private readonly IMemoryCache memoryCache;

    public MemoryCacheService(IMemoryCache memoryCache, IServiceProvider serviceProvider)
    {
      this.memoryCache = memoryCache;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
      return memoryCache.Get<T>(key);
    }
    public async Task SetAsync<T>(string key, T value, int expirationDurationMinutes = 60)
    {
        memoryCache.Set(key, value, DateTimeOffset.Now.AddMinutes(expirationDurationMinutes));
    }

    public async Task RemoveAsync(string key)
    {
      memoryCache.Remove(key);
    }  
}
