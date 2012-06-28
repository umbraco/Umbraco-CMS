using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using umbraco.BusinessLogic;
using umbraco.BusinessLogic.Utils;
using umbraco.interfaces;

namespace umbraco.businesslogic
{
    /// <summary>
    /// ApplicationStartupHandler provides an easy to use base class to install event handlers in umbraco.
    /// Class inhiriting from ApplicationStartupHandler are automaticly registered and instantiated by umbraco on application start.
    /// To use, inhirite the ApplicationStartupHandler Class and add an empty constructor. 
    /// </summary>
    public abstract class ApplicationStartupHandler : IApplicationStartupHandler
    {
        private static readonly IList<IApplicationStartupHandler> _handlers = new List<IApplicationStartupHandler>(); 

        public static void RegisterHandlers()
        {
            if (!GlobalSettings.Configured)
                return;

            var types = TypeFinder.FindClassesOfType<IApplicationStartupHandler>();

            foreach (var t in types)
            {
                try
                {
                    var typeInstance = Activator.CreateInstance(t) as IApplicationStartupHandler;
                    if (typeInstance != null)
                    {
                        _handlers.Add(typeInstance);

                        if (HttpContext.Current != null)
                            HttpContext.Current.Trace.Write("registerApplicationStartupHandlers",
                                                            " + Adding application startup handler '" +
                                                            t.FullName);
                    }
                }
                catch (Exception ee)
                {
                    Log.Add(LogTypes.Error, -1, "Error loading application startup handler: " + ee.ToString());
                }
            }
        }
    }

}
