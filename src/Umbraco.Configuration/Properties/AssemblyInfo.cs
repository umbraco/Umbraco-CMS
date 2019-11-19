using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Umbraco Cms
[assembly: InternalsVisibleTo("Umbraco.Tests")]
[assembly: InternalsVisibleTo("Umbraco.Tests.Benchmarks")]

// Allow this to be mocked in our unit tests
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
