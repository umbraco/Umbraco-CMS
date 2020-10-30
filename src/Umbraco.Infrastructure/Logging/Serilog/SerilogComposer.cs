using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging.Serilog.Enrichers;
using Umbraco.Infrastructure.Logging.Serilog.Enrichers;

namespace Umbraco.Infrastructure.Logging.Serilog
{
    public class SerilogComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.Services.AddUnique<ThreadAbortExceptionEnricher>();
            composition.Services.AddUnique<HttpSessionIdEnricher>();
            composition.Services.AddUnique<HttpRequestNumberEnricher>();
            composition.Services.AddUnique<HttpRequestIdEnricher>();
        }
    }    
}
