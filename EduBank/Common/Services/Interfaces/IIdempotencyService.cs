using System.Threading.Tasks;

namespace Common.Services
{
    public interface IIdempotencyService
    {
        Task<IdempotentResponse?> GetResponseAsync(string key, string path, string method);
        Task StoreResponseAsync(string key, string path, string method, int statusCode, string responseBody);
    }

    public class IdempotentResponse
    {
        public int StatusCode { get; set; }
        public string ResponseBody { get; set; }
    }
}