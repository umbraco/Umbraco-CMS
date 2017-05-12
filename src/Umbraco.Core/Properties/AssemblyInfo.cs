using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

// Umbraco Cms
[assembly: InternalsVisibleTo("umbraco")]
[assembly: InternalsVisibleTo("Umbraco.Tests")]
[assembly: InternalsVisibleTo("Umbraco.Extensions")] // fixme ?
[assembly: InternalsVisibleTo("businesslogic")] // fixme ?
[assembly: InternalsVisibleTo("cms")] // fixme ?
[assembly: InternalsVisibleTo("umbraco.webservices")] // fixme ?
[assembly: InternalsVisibleTo("umbraco.datalayer")]
[assembly: InternalsVisibleTo("Umbraco.Tests")]
[assembly: InternalsVisibleTo("Umbraco.Tests.Benchmarks")]
[assembly: InternalsVisibleTo("Umbraco.Web")]
[assembly: InternalsVisibleTo("Umbraco.Web.UI")]
[assembly: InternalsVisibleTo("UmbracoExamine")]

// Umbraco Deploy
[assembly: InternalsVisibleTo("Umbraco.Deploy")]
[assembly: InternalsVisibleTo("Umbraco.Deploy.UI")]
[assembly: InternalsVisibleTo("Umbraco.Deploy.Cloud")]

// Umbraco Forms
[assembly: InternalsVisibleTo("Umbraco.Forms.Core")]
[assembly: InternalsVisibleTo("Umbraco.Forms.Core.Providers")]
[assembly: InternalsVisibleTo("Umbraco.Forms.Web")]

//allow this to be mocked in our unit tests
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

// v8
[assembly: InternalsVisibleTo("Umbraco.Compat7")]