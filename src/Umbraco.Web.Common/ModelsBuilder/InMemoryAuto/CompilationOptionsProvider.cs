using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using DependencyContextCompilationOptions = Microsoft.Extensions.DependencyModel.CompilationOptions;

namespace Umbraco.Cms.Web.Common.ModelsBuilder.InMemoryAuto;

/*
 * This is a partial Clone'n'Own of microsofts CSharpCompiler, this is just the parts relevant for getting the CompilationOptions
 * Essentially, what this does is that it looks at the compilation options of the Dotnet project and "copies" that
 * https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Razor.RuntimeCompilation/src/CSharpCompiler.cs
 */
internal class CompilationOptionsProvider
{
    private readonly IWebHostEnvironment _hostingEnvironment;
    private CSharpParseOptions? _parseOptions;
    private CSharpCompilationOptions? _compilationOptions;
    private bool _emitPdb;
    private EmitOptions? _emitOptions;
    private bool _optionsInitialized;

    public CompilationOptionsProvider(IWebHostEnvironment hostingEnvironment) =>
        _hostingEnvironment = hostingEnvironment;

    public virtual CSharpParseOptions ParseOptions
    {
        get
        {
            EnsureOptions();
            return _parseOptions;
        }
    }

    public virtual CSharpCompilationOptions CSharpCompilationOptions
    {
        get
        {
            EnsureOptions();
            return _compilationOptions;
        }
    }

    public virtual bool EmitPdb
    {
        get
        {
            EnsureOptions();
            return _emitPdb;
        }
    }

    public virtual EmitOptions EmitOptions
    {
        get
        {
            EnsureOptions();
            return _emitOptions;
        }
    }

    [MemberNotNull(nameof(_emitOptions), nameof(_parseOptions), nameof(_compilationOptions))]
    private void EnsureOptions()
    {
        if (!_optionsInitialized)
        {
            var dependencyContextOptions = GetDependencyContextCompilationOptions();
            _parseOptions = GetParseOptions(_hostingEnvironment, dependencyContextOptions);
            _compilationOptions = GetCompilationOptions(_hostingEnvironment, dependencyContextOptions);
            _emitOptions = GetEmitOptions(dependencyContextOptions);

            _optionsInitialized = true;
        }

        Debug.Assert(_parseOptions is not null);
        Debug.Assert(_compilationOptions is not null);
        Debug.Assert(_emitOptions is not null);
    }

    private DependencyContextCompilationOptions GetDependencyContextCompilationOptions()
    {
        if (!string.IsNullOrEmpty(_hostingEnvironment.ApplicationName))
        {
            var applicationAssembly = Assembly.Load(new AssemblyName(_hostingEnvironment.ApplicationName));
            var dependencyContext = DependencyContext.Load(applicationAssembly);
            if (dependencyContext?.CompilationOptions != null)
            {
                return dependencyContext.CompilationOptions;
            }
        }

        return DependencyContextCompilationOptions.Default;
    }

    private EmitOptions GetEmitOptions(DependencyContextCompilationOptions dependencyContextOptions)
    {
        // Assume we're always producing pdbs unless DebugType = none
        _emitPdb = true;
        DebugInformationFormat debugInformationFormat;
        if (string.IsNullOrEmpty(dependencyContextOptions.DebugType))
        {
            debugInformationFormat = DebugInformationFormat.PortablePdb;
        }
        else
        {
            // Based on https://github.com/dotnet/roslyn/blob/1d28ff9ba248b332de3c84d23194a1d7bde07e4d/src/Compilers/CSharp/Portable/CommandLine/CSharpCommandLineParser.cs#L624-L640
            switch (dependencyContextOptions.DebugType.ToLowerInvariant())
            {
                case "none":
                    // There isn't a way to represent none in DebugInformationFormat.
                    // We'll set EmitPdb to false and let callers handle it by setting a null pdb-stream.
                    _emitPdb = false;
                    return new EmitOptions();
                case "portable":
                    debugInformationFormat = DebugInformationFormat.PortablePdb;
                    break;
                case "embedded":
                    // Roslyn does not expose enough public APIs to produce a binary with embedded pdbs.
                    // We'll produce PortablePdb instead to continue providing a reasonable user experience.
                    debugInformationFormat = DebugInformationFormat.PortablePdb;
                    break;
                case "full":
                case "pdbonly":
                    debugInformationFormat = DebugInformationFormat.PortablePdb;
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        var emitOptions = new EmitOptions(debugInformationFormat: debugInformationFormat);
        return emitOptions;
    }

    private static CSharpCompilationOptions GetCompilationOptions(
        IWebHostEnvironment hostingEnvironment,
        DependencyContextCompilationOptions dependencyContextOptions)
    {
        var csharpCompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        // Disable 1702 until roslyn turns this off by default
        csharpCompilationOptions = csharpCompilationOptions.WithSpecificDiagnosticOptions(
            new Dictionary<string, ReportDiagnostic>
            {
                    {"CS1701", ReportDiagnostic.Suppress}, // Binding redirects
                    {"CS1702", ReportDiagnostic.Suppress},
                    {"CS1705", ReportDiagnostic.Suppress}
            });

        if (dependencyContextOptions.AllowUnsafe.HasValue)
        {
            csharpCompilationOptions = csharpCompilationOptions.WithAllowUnsafe(
                dependencyContextOptions.AllowUnsafe.Value);
        }

        OptimizationLevel optimizationLevel;
        if (dependencyContextOptions.Optimize.HasValue)
        {
            optimizationLevel = dependencyContextOptions.Optimize.Value ?
                OptimizationLevel.Release :
                OptimizationLevel.Debug;
        }
        else
        {
            optimizationLevel = hostingEnvironment.IsDevelopment() ?
                OptimizationLevel.Debug :
                OptimizationLevel.Release;
        }
        csharpCompilationOptions = csharpCompilationOptions.WithOptimizationLevel(optimizationLevel);

        if (dependencyContextOptions.WarningsAsErrors.HasValue)
        {
            var reportDiagnostic = dependencyContextOptions.WarningsAsErrors.Value ?
                ReportDiagnostic.Error :
                ReportDiagnostic.Default;
            csharpCompilationOptions = csharpCompilationOptions.WithGeneralDiagnosticOption(reportDiagnostic);
        }

        return csharpCompilationOptions;
    }

    private static CSharpParseOptions GetParseOptions(
        IWebHostEnvironment hostingEnvironment,
        DependencyContextCompilationOptions dependencyContextOptions)
    {
        var configurationSymbol = hostingEnvironment.IsDevelopment() ? "DEBUG" : "RELEASE";
        var defines = dependencyContextOptions.Defines.Concat(new[] { configurationSymbol }).Where(define => define != null);

        var parseOptions = new CSharpParseOptions(preprocessorSymbols: (IEnumerable<string>)defines);

        parseOptions = parseOptions.WithLanguageVersion(LanguageVersion.Latest);

        return parseOptions;
    }
}
