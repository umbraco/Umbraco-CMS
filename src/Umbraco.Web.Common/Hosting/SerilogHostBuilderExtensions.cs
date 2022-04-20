// Copyright 2020 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Hosting;
using Serilog.Extensions.Logging;
using ILogger = Serilog.ILogger;

namespace Umbraco.Cms.Web.Common.Hosting
{
    /// <summary>
    /// Inlined from Serilog.Extensions.Hosting as we can't configure the initial bootstrap logger without
    /// an IHostEnvironment.
    /// <br/>
    /// <a href="https://github.com/serilog/serilog-extensions-hosting/pull/60">Upstream GH PR</a>
    /// </summary>
    internal static class SerilogHostBuilderExtensions
    {
        /// <summary>Sets Serilog as the logging provider.</summary>
        /// <remarks>
        /// A <see cref="HostBuilderContext"/> is supplied so that configuration and hosting information can be used.
        /// The logger will be shut down when application services are disposed.
        /// </remarks>
        /// <param name="builder">The host builder to configure.</param>
        /// <param name="configureLogger">The delegate for configuring the <see cref="Serilog.LoggerConfiguration" /> that will be used to construct a <see cref="Serilog.Core.Logger" />.</param>
        /// <param name="preserveStaticLogger">Indicates whether to preserve the value of <see cref="Serilog.Log.Logger"/>.</param>
        /// <param name="writeToProviders">By default, Serilog does not write events to <see cref="Microsoft.Extensions.Logging.ILoggerProvider"/>s registered through
        /// the Microsoft.Extensions.Logging API. Normally, equivalent Serilog sinks are used in place of providers. Specify
        /// <c>true</c> to write events to all providers.</param>
        /// <remarks>If the static <see cref="Serilog.Log.Logger"/> is a bootstrap logger (see
        /// <c>LoggerConfigurationExtensions.CreateBootstrapLogger()</c>), and <paramref name="preserveStaticLogger"/> is
        /// not specified, the the bootstrap logger will be reconfigured through the supplied delegate, rather than being
        /// replaced entirely or ignored.</remarks>
        /// <returns>The host builder.</returns>
        public static IHostBuilder UseSerilog(
            this IHostBuilder builder,
            Action<HostBuilderContext, IServiceProvider, LoggerConfiguration> configureLogger,
            bool preserveStaticLogger = false,
            bool writeToProviders = false)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (configureLogger == null)
                throw new ArgumentNullException(nameof(configureLogger));

            builder.ConfigureServices((context, collection) =>
            {
                var reloadable = Log.Logger as ReloadableLogger;
                var useReload = reloadable != null && !preserveStaticLogger;

                LoggerProviderCollection loggerProviders = null;
                if (writeToProviders)
                {
                    loggerProviders = new LoggerProviderCollection();
                }

                collection.AddSingleton(services =>
                {
                    ILogger logger;

                    if (useReload)
                    {
                        reloadable!.Reload(cfg =>
                        {
                            if (loggerProviders != null)
                                cfg.WriteTo.Providers(loggerProviders);

                            configureLogger(context, services, cfg);
                            return cfg;
                        });

                        logger = reloadable.Freeze();
                    }
                    else
                    {
                        var loggerConfiguration = new LoggerConfiguration();

                        if (loggerProviders != null)
                            loggerConfiguration.WriteTo.Providers(loggerProviders);

                        configureLogger(context, services, loggerConfiguration);
                        logger = loggerConfiguration.CreateLogger();
                    }

                    return new RegisteredLogger(logger);
                });

                collection.AddSingleton(services =>
                {
                    // How can we register the logger, here, but not have MEDI dispose it?
                    // Using the `NullEnricher` hack to prevent disposal.
                    var logger = services.GetRequiredService<RegisteredLogger>().Logger;
                    return logger.ForContext(new NullEnricher());
                });

                collection.AddSingleton<ILoggerFactory>(services =>
                {
                    var logger = services.GetRequiredService<RegisteredLogger>().Logger;

                    Serilog.ILogger registeredLogger = null;
                    if (preserveStaticLogger)
                    {
                        registeredLogger = logger;
                    }
                    else
                    {
                        // Passing a `null` logger to `SerilogLoggerFactory` results in disposal via
                        // `Log.CloseAndFlush()`, which additionally replaces the static logger with a no-op.
                        Log.Logger = logger;
                    }

                    var factory = new SerilogLoggerFactory(registeredLogger, !useReload, loggerProviders);

                    if (writeToProviders)
                    {
                        foreach (var provider in services.GetServices<ILoggerProvider>())
                            factory.AddProvider(provider);
                    }

                    return factory;
                });

                // Null is passed here because we've already (lazily) registered `ILogger`
                ConfigureServices(collection, null);
            });

            return builder;
        }

        static void ConfigureServices(IServiceCollection collection, Serilog.ILogger logger)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (logger != null)
            {
                // This won't (and shouldn't) take ownership of the logger.
                collection.AddSingleton(logger);
            }

            // Registered to provide two services...
            var diagnosticContext = new DiagnosticContext(logger);

            // Consumed by e.g. middleware
            collection.AddSingleton(diagnosticContext);

            // Consumed by user code
            collection.AddSingleton<IDiagnosticContext>(diagnosticContext);
        }

        class RegisteredLogger
        {
            public RegisteredLogger(ILogger logger)
            {
                Logger = logger;
            }

            public ILogger Logger { get; }
        }

        class NullEnricher : ILogEventEnricher
        {
            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
            {
            }
        }
    }
}
