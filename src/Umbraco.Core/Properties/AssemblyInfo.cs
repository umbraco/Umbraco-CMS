using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Umbraco.Core")]
[assembly: AssemblyDescription("Umbraco Core")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("Umbraco CMS")]

[assembly: ComVisible(false)]
[assembly: Guid("130a6b5c-50e7-4df3-a0dd-e9e7eb0b7c5c")]

// Umbraco Cms
[assembly: InternalsVisibleTo("Umbraco.Web")]
[assembly: InternalsVisibleTo("Umbraco.Web.UI")]
[assembly: InternalsVisibleTo("Umbraco.Examine")]

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
