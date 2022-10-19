// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection.PortableExecutable;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Web.Common.ModelsBuilder.InMemoryAuto;


/*
 * This is a clone'n'own of Microsofts RazorReferenceManager, please check if there's been any updates to the file on their end
 * before trying to mock about fixing issues:
 * https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Razor.RuntimeCompilation/src/RazorReferenceManager.cs
 *
 * This class is used by the the ViewCompiler (CollectibleRuntimeViewCompiler) to find the required references when compiling views.
 */
internal class UmbracoRazorReferenceManager
{
    private readonly ApplicationPartManager _partManager;
    private readonly MvcRazorRuntimeCompilationOptions _options;
    private object _compilationReferencesLock = new object();
    private bool _compilationReferencesInitialized;
    private IReadOnlyList<MetadataReference>? _compilationReferences;

    public UmbracoRazorReferenceManager(
        ApplicationPartManager partManager,
        IOptions<MvcRazorRuntimeCompilationOptions> options)
    {
        _partManager = partManager;
        _options = options.Value;
    }

    public virtual IReadOnlyList<MetadataReference> CompilationReferences
    {
        get
        {
            return LazyInitializer.EnsureInitialized(
                ref _compilationReferences,
                ref _compilationReferencesInitialized,
                ref _compilationReferencesLock,
                GetCompilationReferences)!;
        }
    }

    private IReadOnlyList<MetadataReference> GetCompilationReferences()
    {
        var referencePaths = GetReferencePaths();

        return referencePaths
            .Select(CreateMetadataReference)
            .ToList();
    }

    // For unit testing
    internal IEnumerable<string> GetReferencePaths()
    {
        var referencePaths = new List<string>(_options.AdditionalReferencePaths.Count);

        foreach (var part in _partManager.ApplicationParts)
        {
            if (part is ICompilationReferencesProvider compilationReferenceProvider)
            {
                referencePaths.AddRange(compilationReferenceProvider.GetReferencePaths());
            }
            else if (part is AssemblyPart assemblyPart)
            {
                referencePaths.AddRange(assemblyPart.GetReferencePaths());
            }
        }

        referencePaths.AddRange(_options.AdditionalReferencePaths);

        return referencePaths;
    }

    private static MetadataReference CreateMetadataReference(string path)
    {
        using (var stream = File.OpenRead(path))
        {
            var moduleMetadata = ModuleMetadata.CreateFromStream(stream, PEStreamOptions.PrefetchMetadata);
            var assemblyMetadata = AssemblyMetadata.Create(moduleMetadata);

            return assemblyMetadata.GetReference(filePath: path);
        }
    }
}
