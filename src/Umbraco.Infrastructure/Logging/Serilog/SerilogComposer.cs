using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging.Serilog.Enrichers;
using Umbraco.Infrastructure.Logging.Serilog.Enrichers;

namespace Umbraco.Infrastructure.Logging.Serilog
{
    public class SerilogComposer : ICoreComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddUnique<ThreadAbortExceptionEnricher>();
            builder.Services.AddUnique<HttpSessionIdEnricher>();
            builder.Services.AddUnique<HttpRequestNumberEnricher>();
            builder.Services.AddUnique<HttpRequestIdEnricher>();
        }
    }    
}
