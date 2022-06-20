// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Web.BackOffice.Authorization;

/// <summary>
///     Authorization requirements for <see cref="SectionHandler" />
/// </summary>
public class SectionRequirement : IAuthorizationRequirement
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SectionRequirement" /> class.
    /// </summary>
    /// <param name="aliases">Aliases for sections that the user will need access to.</param>
    public SectionRequirement(params string[] aliases) => SectionAliases = aliases;

    /// <summary>
    ///     Gets the aliases for sections that the user will need access to.
    /// </summary>
    public IReadOnlyCollection<string> SectionAliases { get; }
}
