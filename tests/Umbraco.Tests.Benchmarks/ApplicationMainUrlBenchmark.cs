using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks
{
    [QuickRunWithMemoryDiagnoserConfig]
    public class ApplicationMainUrlBenchmark
    {
        private readonly HttpRequest _request;

        public ApplicationMainUrlBenchmark()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = "GET";
            httpContext.Request.Scheme = "https";
            httpContext.Request.Host = new HostString("example.com");

            _request = httpContext.Request;
        }

        [Benchmark]
        public Uri GetApplicationUrlFromCurrentRequestString()
        {
            if (_request.Method == "GET" || _request.Method == "POST")
            {
                return new Uri($"{_request.Scheme}://{_request.Host}{_request.PathBase}", UriKind.Absolute);
            }

            return null;
        }

        [Benchmark]
        public Uri GetApplicationUrlFromCurrentRequestUriHelper()
        {
            if (_request.Method == "GET" || _request.Method == "POST")
            {
                var url = UriHelper.BuildAbsolute(_request.Scheme, _request.Host, _request.PathBase);

                return new Uri(url, UriKind.Absolute);
            }

            return null;
        }
    }
}
