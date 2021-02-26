// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Infrastructure.Examine
{
    public interface ILuceneDirectoryFactory
    {
        Lucene.Net.Store.Directory CreateDirectory(string indexName);
    }
}
