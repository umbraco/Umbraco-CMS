namespace Umbraco.Cms.Api.Management.Mapping;

/// <summary>
/// Maps from the old section aliases to the new section names.
/// This is static since it's expected to be removed, so might as well make the clean up work as easy as possible.
/// FIXME: This is a temporary thing until permissions is fleshed out and section is either migrated to some form of permission
/// </summary>
public static class SectionMapper
{
    private static readonly List<SectionMapping> _sectionMappings = new()
    {
        new SectionMapping { Alias = "content", Name = "Umb.Section.Content" },
        new SectionMapping { Alias = "media", Name = "Umb.Section.Media" },
        new SectionMapping { Alias = "member", Name = "Umb.Section.Members" },
        new SectionMapping { Alias = "settings", Name = "Umb.Section.Settings" },
        new SectionMapping { Alias = "packages", Name = "Umb.Section.Packages" },
        new SectionMapping { Alias = "translation", Name = "Umb.Section.Translation" },
        new SectionMapping { Alias = "users", Name = "Umb.Section.Users" },
        new SectionMapping { Alias = "forms", Name = "Umb.Section.Forms" },
    };

    /// <summary>
    /// Gets the display name for a given section alias.
    /// </summary>
    /// <param name="alias">The alias of the section.</param>
    /// <returns>The display name of the section if found; otherwise, returns the alias.</returns>
    public static string GetName(string alias)
    {
        SectionMapping? mapping = _sectionMappings.FirstOrDefault(x => x.Alias == alias);

        if (mapping is not null)
        {
            return mapping.Name;
        }

        // If we can't find it we just fall back to the alias
        return alias;
    }

    /// <summary>
    /// Gets the alias for the given section name.
    /// </summary>
    /// <param name="name">The name of the section to get the alias for.</param>
    /// <returns>The alias corresponding to the specified section name, or the name itself if no alias is found.</returns>
    public static string GetAlias(string name)
    {
        SectionMapping? mapping = _sectionMappings.FirstOrDefault(x => x.Name == name);

        if (mapping is not null)
        {
            return mapping.Alias;
        }

        // If we can't find it we just fall back to the name
        return name;
    }

    private sealed class SectionMapping
    {
    /// <summary>Gets or sets the alias of the section mapping.</summary>
        public required string Alias { get; init; }

    /// <summary>
    /// Gets or sets the name of the section.
    /// </summary>
        public required string Name { get; init; }
    }
}
