using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FaunaDB.Driver
{
    public interface IClientIO
    {
        Task<RequestResult> DoRequest(HttpMethod method, string path, string data, IReadOnlyDictionary<string, string> query = null);
    }
}