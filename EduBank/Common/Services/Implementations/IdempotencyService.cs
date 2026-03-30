using Microsoft.Extensions.Caching.Memory;

namespace Common.Services
{
    public class IdempotencyCacheService : IIdempotencyService
    {
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

        public IdempotencyCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<IdempotentResponse?> GetResponseAsync(string key, string path, string method)
        {
            var cacheKey = GetCacheKey(key, path, method);
            if (_cache.TryGetValue(cacheKey, out IdempotentResponse? response))
            {
                return Task.FromResult(response);
            }
            return Task.FromResult<IdempotentResponse?>(null);
        }

        public Task StoreResponseAsync(string key, string path, string method, int statusCode, string responseBody)
        {
            var cacheKey = GetCacheKey(key, path, method);
            var response = new IdempotentResponse
            {
                StatusCode = statusCode,
                ResponseBody = responseBody
            };
            _cache.Set(cacheKey, response, CacheDuration);
            return Task.CompletedTask;
        }

        private static string GetCacheKey(string key, string path, string method)
            => $"idemp_{method}_{path}_{key}";
    }
}