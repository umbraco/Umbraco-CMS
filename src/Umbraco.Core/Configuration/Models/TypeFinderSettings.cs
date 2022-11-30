// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for type finder settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigTypeFinder)]
public class TypeFinderSettings
{
    /// <summary>
    ///     Gets or sets a value for the assemblies that accept load exceptions during type finder operations.
    /// </summary>
    [Required]
    public string AssembliesAcceptingLoadExceptions { get; set; } = null!;

    /// <summary>
    ///     By default the entry assemblies for scanning plugin types is the Umbraco DLLs. If you require
    ///     scanning for plugins based on different root referenced assemblies you can add the assembly name to this list.
    /// </summary>
    public IEnumerable<string>? AdditionalEntryAssemblies { get; set; }
}
