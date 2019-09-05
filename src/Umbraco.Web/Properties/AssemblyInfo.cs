using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Umbraco.Web")]
[assembly: AssemblyDescription("Umbraco Web")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("Umbraco CMS")]

[assembly: ComVisible(false)]
[assembly: Guid("ce9d3539-299e-40d3-b605-42ac423e24fa")]

// Umbraco Cms
[assembly: InternalsVisibleTo("Umbraco.Web.UI")]

[assembly: InternalsVisibleTo("Umbraco.Tests")]
[assembly: InternalsVisibleTo("Umbraco.Tests.Benchmarks")]

// Allow this to be mocked in our unit tests
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

// Umbraco Deploy
[assembly: InternalsVisibleTo("Umbraco.Deploy")]
[assembly: InternalsVisibleTo("Umbraco.Deploy.UI")]
[assembly: InternalsVisibleTo("Umbraco.Deploy.Cloud")]

// Umbraco Forms
[assembly: InternalsVisibleTo("Umbraco.Forms.Core")]
[assembly: InternalsVisibleTo("Umbraco.Forms.Core.Providers")]
[assembly: InternalsVisibleTo("Umbraco.Forms.Web")]

// Umbraco Headless
[assembly: InternalsVisibleTo("Umbraco.Headless")]

// code analysis
// IDE1006 is broken, wants _value syntax for consts, etc - and it's even confusing ppl at MS, kill it
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "~_~")]
