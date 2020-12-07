using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Serilog.Context;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Logging.Serilog;
using Umbraco.Core.Logging.Serilog.Enrichers;
using Umbraco.Net;
using Umbraco.Web.Hosting;
using ConnectionStrings = Umbraco.Core.Configuration.Models.ConnectionStrings;
using Current = Umbraco.Web.Composing.Current;
using GlobalSettings = Umbraco.Core.Configuration.Models.GlobalSettings;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Umbraco.Web
{
    /// <summary>
    /// Provides an abstract base class for the Umbraco HttpApplication.
    /// </summary>
    public abstract class UmbracoApplicationBase : HttpApplication
    {
        private readonly ILogger<UmbracoApplicationBase> _logger;
        private readonly SecuritySettings _securitySettings;
        private readonly GlobalSettings _globalSettings;
        private readonly ConnectionStrings _connectionStrings;
        private readonly IIOHelper _ioHelper;
        private IRuntime _runtime;
        private IServiceProvider _factory;
        private ILoggerFactory _loggerFactory;

        protected UmbracoApplicationBase()
        {
       
                HostingSettings hostingSettings = null;
                GlobalSettings globalSettings = null;
                SecuritySettings securitySettings = null;
                WebRoutingSettings webRoutingSettings = null;

                var hostingEnvironment = new AspNetHostingEnvironment(Options.Create(hostingSettings));
                var loggingConfiguration = new LoggingConfiguration(
                    Path.Combine(hostingEnvironment.ApplicationPhysicalPath, "App_Data\\Logs"));
                var ioHelper = new IOHelperWindows(hostingEnvironment);
                var logger = SerilogLogger.CreateWithDefaultConfiguration(hostingEnvironment, loggingConfiguration, new ConfigurationRoot(new List<IConfigurationProvider>()));

                var backOfficeInfo = new AspNetBackOfficeInfo(globalSettings, ioHelper, _loggerFactory.CreateLogger<AspNetBackOfficeInfo>(), Options.Create(webRoutingSettings));
                var profiler = GetWebProfiler(hostingEnvironment);
                StaticApplicationLogging.Initialize(_loggerFactory);
                Logger = NullLogger<UmbracoApplicationBase>.Instance;
        }

        private IProfiler GetWebProfiler(IHostingEnvironment hostingEnvironment)
        {
            // create and start asap to profile boot
            if (!hostingEnvironment.IsDebugMode)
            {
                // should let it be null, that's how MiniProfiler is meant to work,
                // but our own IProfiler expects an instance so let's get one
                return new VoidProfiler();
            }

            return new VoidProfiler();
        }

        protected UmbracoApplicationBase(ILogger<UmbracoApplicationBase> logger, ILoggerFactory loggerFactory, SecuritySettings securitySettings, GlobalSettings globalSettings, ConnectionStrings connectionStrings, IIOHelper ioHelper, IProfiler profiler, IHostingEnvironment hostingEnvironment, IBackOfficeInfo backOfficeInfo)
        {
            _logger = logger;
            _securitySettings = securitySettings;
            _globalSettings = globalSettings;
            _connectionStrings = connectionStrings;
            _ioHelper = ioHelper;
            _loggerFactory = loggerFactory;

            Logger = logger;
            StaticApplicationLogging.Initialize(_loggerFactory);
        }

        protected ILogger<UmbracoApplicationBase> Logger { get; }

        /// <summary>
        /// Gets a <see cref="ITypeFinder"/>
        /// </summary>
        /// <param name="hostingEnvironment"></param>
        /// <param name="logger"></param>
        /// <param name="profiler"></param>
        /// <returns></returns>
        protected virtual ITypeFinder GetTypeFinder(IHostingEnvironment hostingEnvironment, ILogger logger, IProfiler profiler)
        {
            // TODO: Currently we are not passing in any TypeFinderConfig (with ITypeFinderSettings) which we should do, however
            // this is not critical right now and would require loading in some config before boot time so just leaving this as-is for now.
            var runtimeHashPaths = new RuntimeHashPaths();
            // the bin folder and everything in it
            runtimeHashPaths.AddFolder(new DirectoryInfo(hostingEnvironment.MapPathContentRoot("~/bin")));
            // the app code folder and everything in it
            runtimeHashPaths.AddFile(new FileInfo(hostingEnvironment.MapPathContentRoot("~/App_Code")));
            // global.asax (the app domain also monitors this, if it changes will do a full restart)
            runtimeHashPaths.AddFile(new FileInfo(hostingEnvironment.MapPathContentRoot("~/global.asax")));
            var runtimeHash = new RuntimeHash(new ProfilingLogger(_loggerFactory.CreateLogger<ProfilingLogger>(), profiler), runtimeHashPaths);
            return new TypeFinder(_loggerFactory.CreateLogger<TypeFinder>(), new DefaultUmbracoAssemblyProvider(
                // GetEntryAssembly was actually an exposed API by request of the aspnetcore team which works in aspnet core because a website
                // in that case is essentially an exe. However in netframework there is no entry assembly, things don't really work that way since
                // the process that is running the site is iisexpress, so this returns null. The best we can do is fallback to GetExecutingAssembly()
                // which will just return Umbraco.Infrastructure (currently with netframework) and for our purposes that is OK.
                // If you are curious... There is really no way to get the entry assembly in netframework without the hosting website having it's own
                // code compiled for the global.asax which is the entry point. Because the default global.asax for umbraco websites is just a file inheriting
                // from Umbraco.Web.UmbracoApplication, the global.asax file gets dynamically compiled into a DLL in the dynamic folder (we can get an instance
                // of that, but this doesn't really help us) but the actually entry execution is still Umbraco.Web. So that is the 'highest' level entry point
                // assembly we can get and we can only get that if we put this code into the WebRuntime since the executing assembly is the 'current' one.
                // For this purpose, it doesn't matter if it's Umbraco.Web or Umbraco.Infrastructure since all assemblies are in that same path and we are
                // getting rid of netframework.
                Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()), runtimeHash);
        }

        /// <summary>
        /// Gets the application register.
        /// </summary>
        protected virtual IServiceCollection GetRegister(GlobalSettings globalSettings)
        {
            return new ServiceCollection();
        }

        // events - in the order they trigger

        // were part of the BootManager architecture, would trigger only for the initial
        // application, so they need not be static, and they would let ppl hook into the
        // boot process... but I believe this can be achieved with components as well and
        // we don't need these events.
        //public event EventHandler ApplicationStarting;
        //public event EventHandler ApplicationStarted;

        // this event can only be static since there will be several instances of this class
        // triggers for each application instance, ie many times per lifetime of the application
        public static event EventHandler ApplicationInit;

        // this event can only be static since there will be several instances of this class
        // triggers once per error
        public static event EventHandler ApplicationError;

        // this event can only be static since there will be several instances of this class
        // triggers once per lifetime of the application, before it is unloaded
        public static event EventHandler ApplicationEnd;

        #region Start

        // internal for tests
        internal void HandleApplicationStart(object sender, EventArgs evargs)
        {
            // ******** THIS IS WHERE EVERYTHING BEGINS ********


            var globalSettings =  _globalSettings;
            var umbracoVersion = new UmbracoVersion();

            // create the register for the application, and boot
            // the boot manager is responsible for registrations
            var register = GetRegister(globalSettings);
            //var boostrapper = GetRuntime(
            //    _globalSettings,
            //    _connectionStrings,
            //    umbracoVersion,
            //    _ioHelper,
            //    _logger,
            //    _loggerFactory,
            //    null, // TODO get from somewhere that isn't Current.
            //    null, // TODO get from somewhere that isn't Current.
            //    null // TODO get from somewhere that isn't Current.
            //    );
            //_factory = Current.Factory = _runtime.Configure(register);


            // now we can add our request based logging enrichers (globally, which is what we were doing in netframework before)
            LogContext.Push(new HttpSessionIdEnricher(_factory.GetRequiredService<ISessionIdResolver>()));
            LogContext.Push(new HttpRequestNumberEnricher(_factory.GetRequiredService<IRequestCache>()));
            LogContext.Push(new HttpRequestIdEnricher(_factory.GetRequiredService<IRequestCache>()));

            _runtime = _factory.GetRequiredService<IRuntime>();
            _runtime.Start();
        }

        // called by ASP.NET (auto event wireup) once per app domain
        // do NOT set instance data here - only static (see docs)
        // sender is System.Web.HttpApplicationFactory, evargs is EventArgs.Empty
        protected void Application_Start(object sender, EventArgs evargs)
        {
            Thread.CurrentThread.SanitizeThreadCulture();
            HandleApplicationStart(sender, evargs);
        }

        #endregion

        #region Init

        private void OnApplicationInit(object sender, EventArgs evargs)
        {
            TryInvoke(ApplicationInit, "ApplicationInit", sender, evargs);
        }

        // called by ASP.NET for every HttpApplication instance after all modules have been created
        // which means that this will be called *many* times for different apps when Umbraco runs
        public override void Init()
        {
            // note: base.Init() is what initializes all of the http modules, ties up a bunch of stuff with IIS, etc...
            // therefore, since OWIN is an HttpModule when running in IIS/ASP.Net the OWIN startup is not executed
            // until this method fires and by that time - Umbraco has booted already

            base.Init();
            OnApplicationInit(this, new EventArgs());
        }


        #endregion

        #region End

        protected virtual void OnApplicationEnd(object sender, EventArgs evargs)
        {
            ApplicationEnd?.Invoke(this, EventArgs.Empty);
        }

        // internal for tests
        internal void HandleApplicationEnd()
        {
            if (_runtime != null)
            {
                _runtime.Terminate();
                _runtime.DisposeIfDisposable();

                _runtime = null;
            }

            // try to log the detailed shutdown message (typical asp.net hack: http://weblogs.asp.net/scottgu/433194)
            try
            {
                var runtime = (HttpRuntime) typeof(HttpRuntime).InvokeMember("_theRuntime",
                    BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField,
                    null, null, null);
                if (runtime == null)
                    return;

                var shutDownMessage = (string)runtime.GetType().InvokeMember("_shutDownMessage",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                    null, runtime, null);

                var shutDownStack = (string)runtime.GetType().InvokeMember("_shutDownStack",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                    null, runtime, null);

                Current.Logger.LogInformation("Application shutdown. Details: {ShutdownReason}\r\n\r\n_shutDownMessage={ShutdownMessage}\r\n\r\n_shutDownStack={ShutdownStack}",
                    HostingEnvironment.ShutdownReason,
                    shutDownMessage,
                    shutDownStack);
            }
            catch (Exception)
            {
                //if for some reason that fails, then log the normal output
                Current.Logger.LogInformation("Application shutdown. Reason: {ShutdownReason}", HostingEnvironment.ShutdownReason);
            }

            Current.Logger.DisposeIfDisposable();
        }

        // called by ASP.NET (auto event wireup) once per app domain
        // sender is System.Web.HttpApplicationFactory, evargs is EventArgs.Empty
        protected void Application_End(object sender, EventArgs evargs)
        {
            OnApplicationEnd(sender, evargs);
            HandleApplicationEnd();
        }

        #endregion

        #region Error

        protected virtual void OnApplicationError(object sender, EventArgs evargs)
        {
            ApplicationError?.Invoke(this, EventArgs.Empty);
        }

        private void HandleApplicationError()
        {
            var exception = Server.GetLastError();

            // ignore HTTP errors
            if (exception.GetType() == typeof(HttpException)) return;

            Current.Logger.LogError(exception, "An unhandled exception occurred");
        }

        // called by ASP.NET (auto event wireup) at any phase in the application life cycle
        protected void Application_Error(object sender, EventArgs e)
        {
            // when unhandled errors occur
            HandleApplicationError();
            OnApplicationError(sender, e);
        }

        #endregion

        #region Utilities

        private static void TryInvoke(EventHandler handler, string name, object sender, EventArgs evargs)
        {
            try
            {
                handler?.Invoke(sender, evargs);
            }
            catch (Exception ex)
            {
                Current.Logger.LogError(ex, "Error in {Name} handler.", name);
                throw;
            }
        }

        #endregion
    }
}
