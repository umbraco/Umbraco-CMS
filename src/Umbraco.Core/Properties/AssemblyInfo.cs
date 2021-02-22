using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: Guid("DA322714-FB89-4943-92BD-BB122B82F66B")]

// Umbraco Cms
[assembly: InternalsVisibleTo("Umbraco.Web")]
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
[assembly: InternalsVisibleTo("Umbraco.Cloud.Headless")]
