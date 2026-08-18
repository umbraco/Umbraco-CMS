// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine.Lucene.Directories;
using Lucene.Net.Store;

namespace Umbraco.Cms.Search.Provider.Examine.Lucene;

/// <summary>
/// Supplies <see cref="NoPrefixSimpleFsLockFactory"/> as the Lucene lock factory for a directory.
/// </summary>
public class UmbracoLockFactory : ILockFactory
{
    /// <inheritdoc />
    public LockFactory GetLockFactory(DirectoryInfo directory)
        => new NoPrefixSimpleFsLockFactory(directory);
}
