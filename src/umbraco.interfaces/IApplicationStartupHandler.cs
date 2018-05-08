using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.interfaces
{

	/// <summary>
	/// Any class that implements this interface will be instantiated at application startup
	/// </summary>
	/// <remarks>
	/// NOTE: It is not recommended to use this interface and instead use IApplicationEventHandler
	/// and bind to any custom events in the OnApplicationInitialized method.
	/// </remarks>
	[Obsolete("This interface is obsolete, use IApplicationEventHandler or ApplicationEventHandler instead")]
	public interface IApplicationStartupHandler : IDiscoverable
    { }
}
