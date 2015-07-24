using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTitle("umbraco.presentation")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("Umbraco CMS")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("ce9d3539-299e-40d3-b605-42ac423e24fa")]

//This is required so that Medium trust works and this is because of this class:
// umbraco.presentation.templateControls.ItemDesigner since this class cannot inherit from
// the System.Web.UI.Design.ControlDesigner in partial trust (or something along those lines)
// if we remove this class then we won't need to do this.
[assembly: System.Security.SecurityRules(System.Security.SecurityRuleSet.Level1)]

[assembly: InternalsVisibleTo("Umbraco.Tests")]
[assembly: InternalsVisibleTo("umbraco.MacroEngines")]
[assembly: InternalsVisibleTo("Umbraco.Web.UI")]
[assembly: InternalsVisibleTo("umbraco.webservices")]
[assembly: InternalsVisibleTo("Concorde.Sync")]
[assembly: InternalsVisibleTo("Umbraco.Courier.Core")]
[assembly: InternalsVisibleTo("Umbraco.Belle")]

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]