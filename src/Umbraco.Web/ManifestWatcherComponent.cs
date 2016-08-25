using System.IO;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.IO;
using Umbraco.Core.Manifest;

namespace Umbraco.Web
{
    //[RequireComponent(typeof(object))] // fixme - the one that ensures that runtime.Something is ok
    public class ManifestWatcherComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        // if configured and in debug mode, a ManifestWatcher watches App_Plugins folders for
        // package.manifest chances and restarts the application on any change
        private ManifestWatcher _mw;

        public void Initialize(RuntimeState runtime)
        {
            // fixme
            // if this is a core component it cannot depend on UmbracoCoreComponent - indicates that everything is OK
            // so what-if UmbracoCoreComponent ... aha ... need another one? UmbracoRuntimeComponent?
            // or should we have it in IRuntime AND inject IRuntime? IRuntimeInternal vs IRuntime?

            // runtime should be INJECTED! aha!
            // BUT how can we tell that runtime is "ready enough"? need to depend on some sort of UmbracoRuntimeComponent?
            // and... will this kind of dependency issue be repro everywhere?!
            if (runtime.Something < RuntimeSomething.Run || runtime.Debug == false) return;

            //if (ApplicationContext.Current.IsConfigured == false || GlobalSettings.DebugMode == false)
            //    return;

            var appPlugins = IOHelper.MapPath("~/App_Plugins/");
            if (Directory.Exists(appPlugins) == false) return;

            _mw = new ManifestWatcher(Current.Logger);
            _mw.Start(Directory.GetDirectories(appPlugins));
        }

        public override void Terminate()
        {
            _mw?.Dispose();
            _mw = null;
        }
    }
}