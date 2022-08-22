// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Web.BackOffice.Authorization;

/// <summary>
///     Authorization requirements for <see cref="TreeHandler" />
/// </summary>
public class TreeRequirement : IAuthorizationRequirement
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TreeRequirement" /> class.
    /// </summary>
    /// <param name="aliases">The aliases for trees that the user will need access to.</param>
    public TreeRequirement(params string[] aliases) => TreeAliases = aliases;

    /// <summary>
    ///     Gets the aliases for trees that the user will need access to.
    /// </summary>
    public IReadOnlyCollection<string> TreeAliases { get; }
}
