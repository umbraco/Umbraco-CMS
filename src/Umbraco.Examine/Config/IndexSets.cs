using System.Configuration;

namespace Umbraco.Examine.Config
{
    public sealed class IndexSets : ConfigurationSection
    {

        #region Singleton definition

        protected IndexSets() { }
        static IndexSets()
        {
            Instance = ConfigurationManager.GetSection(SectionName) as IndexSets;

        }
        public static IndexSets Instance { get; }

        #endregion

        private const string SectionName = "ExamineLuceneIndexSets";

        [ConfigurationCollection(typeof(IndexSetCollection))]
        [ConfigurationProperty("", IsDefaultCollection = true, IsRequired = true)]
        public IndexSetCollection Sets => (IndexSetCollection)base[""];
    }
}