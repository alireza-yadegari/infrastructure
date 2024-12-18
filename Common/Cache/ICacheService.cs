namespace Common.Cache;

public interface ICacheService
{
  Task<T> GetAsync<T>(string key);
  Task SetAsync<T>(string key, T value, int expirationDurationMinutes = 60);
  Task RemoveAsync(string key);
}