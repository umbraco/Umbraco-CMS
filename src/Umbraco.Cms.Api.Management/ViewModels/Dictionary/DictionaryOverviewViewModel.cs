﻿namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

public class DictionaryOverviewViewModel
{
    /// <summary>
    ///     Gets or sets the key.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the key.
    /// </summary>
    public Guid Key { get; set; }

    /// <summary>
    ///     Gets or sets the level.
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    ///     Sets the translations.
    /// </summary>
    public IEnumerable<string> TranslatedIsoCodes { get; set; } = Array.Empty<string>();
}
