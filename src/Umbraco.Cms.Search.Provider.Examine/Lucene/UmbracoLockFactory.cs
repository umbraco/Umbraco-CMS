// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine.Lucene.Directories;
using Lucene.Net.Store;

namespace Umbraco.Cms.Search.Provider.Examine.Lucene;

public class UmbracoLockFactory : ILockFactory
{
    public LockFactory GetLockFactory(DirectoryInfo directory)
        => new NoPrefixSimpleFsLockFactory(directory);
}
