using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Umbraco.Core")]
[assembly: AssemblyDescription("Core assembly containing the new codebase foundation")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("Umbraco CMS")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("130a6b5c-50e7-4df3-a0dd-e9e7eb0b7c5c")]

[assembly: InternalsVisibleTo("umbraco")]
[assembly: InternalsVisibleTo("Umbraco.Tests")]
[assembly: InternalsVisibleTo("Umbraco.Extensions")]
[assembly: InternalsVisibleTo("businesslogic")]
[assembly: InternalsVisibleTo("cms")]
[assembly: InternalsVisibleTo("umbraco.editorControls")]
[assembly: InternalsVisibleTo("umbraco.webservices")]
[assembly: InternalsVisibleTo("umbraco.datalayer")]
[assembly: InternalsVisibleTo("umbraco.MacroEngines")]

[assembly: InternalsVisibleTo("umbraco.editorControls")]
[assembly: InternalsVisibleTo("Umbraco.Tests")]
[assembly: InternalsVisibleTo("Umbraco.Core")]
[assembly: InternalsVisibleTo("Umbraco.Web")]
[assembly: InternalsVisibleTo("Umbraco.Web.UI")]
[assembly: InternalsVisibleTo("UmbracoExamine")]

[assembly: InternalsVisibleTo("Concorde.Sync")]
[assembly: InternalsVisibleTo("Umbraco.Belle")]
[assembly: InternalsVisibleTo("Umbraco.VisualStudio")]
[assembly: InternalsVisibleTo("Umbraco.Courier.Core")]
[assembly: InternalsVisibleTo("umbraco.providers")]

//allow this to be mocked in our unit tests
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]