using System;
using System.Collections.Generic;
using LightInject;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Logging;
using Umbraco.Core.Plugins;

namespace Umbraco.Compat7
{
    public class Compat7Component : UmbracoComponentBase, IUmbracoUserComponent
    {
        private List<IApplicationEventHandler> _handlers;
        private UmbracoApplicationBase _app;

        // these events replace the UmbracoApplicationBase corresponding events
        public static event EventHandler ApplicationStarting;
        public static event EventHandler ApplicationStarted;

        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            var container = composition.Container;
            _app = container.GetInstance<UmbracoApplicationBase>();
            var logger = container.GetInstance<ILogger>();

            var pluginManager = container.GetInstance<PluginManager>();
            var handlerTypes = pluginManager.ResolveTypes<IApplicationEventHandler>();

            _handlers = handlerTypes.Select(Activator.CreateInstance).Cast<IApplicationEventHandler>().ToList();

            foreach (var handler in _handlers)
                logger.Debug<Compat7Component>($"Adding ApplicationEventHandler {handler.GetType().FullName}.");

            foreach (var handler in _handlers)
                handler.OnApplicationInitialized(_app, ApplicationContext.Current);

            foreach (var handler in _handlers)
                handler.OnApplicationStarting(_app, ApplicationContext.Current);

            try
            {
                ApplicationStarting?.Invoke(_app, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                logger.Error<Compat7Component>("An error occurred in an ApplicationStarting event handler", ex);
                throw;
            }
        }

        public void Initialize(ILogger logger)
        {
            foreach (var handler in _handlers)
                handler.OnApplicationStarted(_app, ApplicationContext.Current);

            try
            {
                ApplicationStarted?.Invoke(_app, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                logger.Error<Compat7Component>("An error occurred in an ApplicationStarting event handler", ex);
                throw;
            }
        }
    }
}
