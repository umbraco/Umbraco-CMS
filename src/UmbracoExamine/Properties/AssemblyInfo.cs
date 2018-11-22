using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Security;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyCompany("umbraco")]
[assembly: AssemblyCopyright("Copyright © Umbraco 2012")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyTitle("UmbracoExamine")]
[assembly: AssemblyDescription("Umbraco index & search providers based on the Examine model using Lucene.NET 2.9.2")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("UmbracoExamine")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("31c5b048-cfa8-49b4-8983-bdba0f99eef5")]

[assembly: NeutralResourcesLanguage("en-US")]

//NOTE: WE cannot make change the major version to be the same as Umbraco because of backwards compatibility, however we 
// will make the minor version the same as the umbraco version 
[assembly: AssemblyVersion("0.7.0.*")]
[assembly: AssemblyFileVersion("0.7.0.*")]

[assembly: InternalsVisibleTo("Umbraco.Tests")]
