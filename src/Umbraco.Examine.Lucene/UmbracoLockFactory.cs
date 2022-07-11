// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine.Lucene.Directories;
using Lucene.Net.Store;

namespace Umbraco.Cms.Infrastructure.Examine;

public class UmbracoLockFactory : ILockFactory
{
    public LockFactory GetLockFactory(DirectoryInfo directory)
        => new NoPrefixSimpleFsLockFactory(directory);
}
