using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyCompany("umbraco")]
[assembly: AssemblyCopyright("Copyright © Umbraco 2012")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyTitle("UmbracoExamine.PDF")]
[assembly: AssemblyDescription("Umbraco index providers for PDF based on the Examine model using Lucene.NET")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("UmbracoExamine.PDF")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("8933a78d-8414-4c72-a74d-76aa7fb0e9ad")]

//NOTE: WE cannot make change the major version to be the same as Umbraco because of backwards compatibility, however we 
// will make the minor version the same as the umbraco version 
[assembly: AssemblyVersion("0.6.0.*")]
[assembly: AssemblyFileVersion("0.6.0.*")]

//Unfortunately itextsharp does not natively support full trust
//[assembly: AllowPartiallyTrustedCallers]