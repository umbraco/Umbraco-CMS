using System.Reflection;
using System.Resources;

[assembly: AssemblyCompany("Umbraco")]
[assembly: AssemblyCopyright("Copyright © Umbraco 2022")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: NeutralResourcesLanguage("en-US")]

// versions
// read https://stackoverflow.com/questions/64602/what-are-differences-between-assemblyversion-assemblyfileversion-and-assemblyin

// note: do NOT change anything here manually, use the build scripts

// this is the ONLY ONE the CLR cares about for compatibility
// should change ONLY when "hard" breaking compatibility (manual change)
[assembly: AssemblyVersion("8.0.0")]

// these are FYI and changed automatically
[assembly: AssemblyFileVersion("8.18.0")]
[assembly: AssemblyInformationalVersion("8.18.0")]
