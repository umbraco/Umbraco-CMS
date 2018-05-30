using System.Configuration;

namespace Umbraco.Examine.Config
{
    public sealed class IndexSets : ConfigurationSection
    {
        #region Singleton definition

        private IndexSets() { }

        public static IndexSets Instance { get; } = ConfigurationManager.GetSection(SectionName) as IndexSets;

        #endregion

        private const string SectionName = "ExamineLuceneIndexSets";

        [ConfigurationCollection(typeof(IndexSetCollection))]
        [ConfigurationProperty("", IsDefaultCollection = true, IsRequired = true)]
        public IndexSetCollection Sets => (IndexSetCollection)base[""];
    }
}
