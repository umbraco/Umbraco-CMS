﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Infrastructure.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Logging.Viewer
{
    // ReSharper disable once UnusedMember.Global
    public class LogViewerComposer : ICoreComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddUnique<ILogViewerConfig, LogViewerConfig>();
            builder.SetLogViewer<SerilogJsonLogViewer>();
            builder.Services.AddUnique<ILogViewer>(factory =>
            {

                return new SerilogJsonLogViewer(factory.GetRequiredService<ILogger<SerilogJsonLogViewer>>(),
                    factory.GetRequiredService<ILogViewerConfig>(),
                    factory.GetRequiredService<ILoggingConfiguration>(),
                    Log.Logger);
            } );
        }
    }
}
