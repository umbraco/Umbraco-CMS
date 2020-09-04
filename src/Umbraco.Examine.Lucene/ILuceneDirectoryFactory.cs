namespace Umbraco.Examine
{
    public interface ILuceneDirectoryFactory
    {
        Lucene.Net.Store.Directory CreateDirectory(string indexName);
    }
}
