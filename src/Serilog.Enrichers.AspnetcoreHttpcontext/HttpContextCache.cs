using System.Collections.Generic;
using Serilog.Formatting.Json;

namespace Serilog.Enrichers.AspnetcoreHttpcontext
{
    public class HttpContextCache
    {
        public string IpAddress { get; set; }

        public string Host { get; set; }

        public string Path { get; set; }

        public string Method { get; set; }

        public string QueryString { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public string Body { get; set; }
    }
}