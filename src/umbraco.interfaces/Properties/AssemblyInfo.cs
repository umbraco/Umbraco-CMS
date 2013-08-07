using System.Reflection;
using System.Runtime.CompilerServices;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTitle("umbraco.interfaces")]
[assembly: AssemblyDescription("Core assembly containing legacy interfaces")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("Umbraco CMS")]

[assembly: InternalsVisibleTo("cms")]
[assembly: InternalsVisibleTo("Umbraco.Core")]
[assembly: InternalsVisibleTo("Umbraco.Tests")]

//allow this to be mocked in our unit tests
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
