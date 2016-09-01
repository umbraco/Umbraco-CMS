using System;
using System.Linq;
using LightInject;
using Umbraco.Core.Components;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Plugins;
using Umbraco.Core.Services;

namespace Umbraco.Core
{
    /// <summary>
    /// A plugin type that allows developers to execute code during the Umbraco bootup process
    /// </summary>
    /// <remarks>
    /// Allows you to override the methods that you would like to execute code for: ApplicationInitialized, ApplicationStarting, ApplicationStarted.
    ///
    /// By default none of these methods will execute if the Umbraco application is not configured or if the Umbraco database is not configured, however
    /// if you need these methods to execute even if either of these are not configured you can override the properties:
    /// ExecuteWhenApplicationNotConfigured and ExecuteWhenDatabaseNotConfigured
    /// </remarks>
    // fixme - kill.kill.kill
    public abstract class ApplicationEventHandler : IApplicationEventHandler
    {
        public void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication)
        {
            if (ShouldExecute()) ApplicationInitialized(umbracoApplication);
        }

        public void OnApplicationStarting(UmbracoApplicationBase umbracoApplication)
        {
            if (ShouldExecute()) ApplicationStarting(umbracoApplication);
        }

        public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication)
        {
            if (ShouldExecute()) ApplicationStarted(umbracoApplication);
        }

        /// <summary>
        /// Overridable method to execute when the ApplicationContext is created and other static objects that require initialization have been setup
        /// </summary>
        /// <param name="umbracoApplication"></param>
        protected virtual void ApplicationInitialized(UmbracoApplicationBase umbracoApplication)
        { }

        /// <summary>
        /// Overridable method to execute when All resolvers have been initialized but resolution is not frozen so they can be modified in this method
        /// </summary>
        /// <param name="umbracoApplication"></param>
        protected virtual void ApplicationStarting(UmbracoApplicationBase umbracoApplication)
        { }

        /// <summary>
        /// Overridable method to execute when Bootup is completed, this allows you to perform any other bootup logic required for the application.
        /// Resolution is frozen so now they can be used to resolve instances.
        /// </summary>
        /// <param name="umbracoApplication"></param>
        protected virtual void ApplicationStarted(UmbracoApplicationBase umbracoApplication)
        { }

        private bool ShouldExecute()
        {
            var level = Current.RuntimeState.Level;

            switch (Current.RuntimeState.Level)
            {
                case RuntimeLevel.Unknown:
                case RuntimeLevel.Failed:
                case RuntimeLevel.Boot:
                    return false;
                case RuntimeLevel.Install:
                case RuntimeLevel.Upgrade:
                    // sort-of equivalent I assume?
                    //if (!applicationContext.IsConfigured && ExecuteWhenApplicationNotConfigured)
                    if (ExecuteWhenApplicationNotConfigured)
                        return true;
                    //if (!applicationContext.DatabaseContext.IsDatabaseConfigured && ExecuteWhenDatabaseNotConfigured)
                    if (level == RuntimeLevel.Install && ExecuteWhenDatabaseNotConfigured)
                        return true;
                    return false;
                case RuntimeLevel.Run:
                    //if (applicationContext.IsConfigured && applicationContext.DatabaseContext.IsDatabaseConfigured)
                    return true;
                default:
                    throw new NotSupportedException($"Invalid runtime level {level}");
            }
        }

        /// <summary>
        /// A flag to determine if the overridable methods for this class will execute even if the
        /// Umbraco application is not configured
        /// </summary>
        /// <remarks>
        /// An Umbraco Application is not configured when it requires a new install or upgrade. When the latest version in the
        /// assembly does not match the version in the config.
        /// </remarks>
        protected virtual bool ExecuteWhenApplicationNotConfigured => false;

        /// <summary>
        /// A flag to determine if the overridable methods for this class will execute even if the
        /// Umbraco database is not configured
        /// </summary>
        /// <remarks>
        /// The Umbraco database is not configured when we cannot connect to the database or when the database tables are not installed.
        /// </remarks>
        protected virtual bool ExecuteWhenDatabaseNotConfigured => false;
    }

    // that *could* replace CoreRuntime mess almost entirely
    // but what about what's in WebRuntime?

    [DisableComponent] // disabled for now, breaks
    public class ApplicationEventHandlerComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        public override void Compose(ServiceContainer container)
        {
            base.Compose(container);

            // assuming we don't do anything in Compose? or is this where we should create the handlers?
        }

        public void Initialize(PluginManager pluginManager, UmbracoApplicationBase umbracoApplication)
        {
            var startupHandlerTypes = pluginManager.ResolveApplicationStartupHandlers();
            var handlers = startupHandlerTypes.Select(x =>
            {
                try
                {
                    return Activator.CreateInstance(x);
                }
                catch
                {
                    // fixme - breaks here!
                    // cannot unstanciate GridPropertyEditor and other oddities BUT they have been refactored in 7.6?
                    // well, no, so NOW is the time... bah... first we need to figure out STATES
                    throw new Exception("cannot instanciate " + x.FullName);
                }
            }).Cast<IApplicationEventHandler>().ToArray();

            // just a test at that point
            return;

            foreach (var handler in handlers)
                handler.OnApplicationInitialized(umbracoApplication);
            foreach (var handler in handlers)
                handler.OnApplicationStarting(umbracoApplication);

            //This is a special case for the user service, we need to tell it if it's an upgrade, if so we need to ensure that
            // exceptions are bubbled up if a user is attempted to be persisted during an upgrade (i.e. when they auth to login)
            // fixme - wtf? never going back to FALSE !
            ((UserService) Current.Services.UserService).IsUpgrading = true;

            foreach (var handler in handlers)
                handler.OnApplicationStarted(umbracoApplication);

            //applicationContext.IsReady = true;
        }
    }
}