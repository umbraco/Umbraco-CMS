using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyCompany("umbraco")]
[assembly: AssemblyCopyright("Copyright © Umbraco 2012")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyTitle("UmbracoExamine.Azure")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("UmbracoExamine.Azure")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

//NOTE: WE cannot make change the major version to be the same as Umbraco because of backwards compatibility, however we 
// will make the minor version the same as the umbraco version 
[assembly: AssemblyVersion("0.6.0.*")]
[assembly: AssemblyFileVersion("0.6.0.*")]



[assembly: InternalsVisibleTo("Umbraco.Tests")]