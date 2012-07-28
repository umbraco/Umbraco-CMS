using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Logging;
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

        public static void RegisterHandlers()
        {			
			if (ApplicationContext.Current == null || !ApplicationContext.Current.IsConfigured)
				return;

			var types = PluginTypeResolver.Current.ResolveApplicationStartupHandlers();

			foreach (var t in types)
			{
				try
				{
					//this creates the type instance which will trigger the constructor of the object
					//previously we stored these objects in a list but that just takes up memory for no reason.

					var typeInstance = (IApplicationStartupHandler)Activator.CreateInstance(t);
					
					if (HttpContext.Current != null)
						HttpContext.Current.Trace.Write("registerApplicationStartupHandlers",
														" + Adding application startup handler '" +
														t.FullName);
				}
				catch (Exception ee)
				{					
					LogHelper.Error<ApplicationStartupHandler>(
						string.Format("Error loading application startup handler: {0}", ee.ToString()), 
						ee);					
				}
			}
        }
    }

}
