// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine.Lucene.Directories;
using Lucene.Net.Store;

namespace Umbraco.Cms.Infrastructure.Examine;
[Obsolete("This class will be removed in v14, please check documentation of specific search provider", true)]

public class UmbracoLockFactory : ILockFactory
{
    public LockFactory GetLockFactory(DirectoryInfo directory)
        => new NoPrefixSimpleFsLockFactory(directory);
}
