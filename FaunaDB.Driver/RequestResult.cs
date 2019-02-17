using System;
using System.Collections.Generic;
using System.Net.Http;

namespace FaunaDB.Driver
{
    public class RequestResult
    {
        public HttpMethod Method { get; }
        public string Path { get; }
        public IReadOnlyDictionary<string, string> Query { get; }
        public string Data { get; }
        public string ResponseContent { get; }
        public int StatusCode { get; }
        public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; }
        public DateTime StartTime { get; }
        public DateTime EndTime { get; }

        public RequestResult(HttpMethod method, string path, IReadOnlyDictionary<string, string> query,
            string data, string responseContent, int statusCode, IReadOnlyDictionary<string, IEnumerable<string>> headers,
            DateTime startTime, DateTime endTime)
        {
            Method = method;
            Path = path;
            Query = query;
            Data = data;
            ResponseContent = responseContent;
            StatusCode = statusCode;
            Headers = headers;
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}