using System.Configuration;


namespace Umbraco.Examine.Config
{
    public sealed class IndexSetCollection : ConfigurationElementCollection
    {
        #region Overridden methods to define collection
        protected override ConfigurationElement CreateNewElement()
        {
            return new IndexSet();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((IndexSet)element).SetName;
        }
        public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMap;
        protected override string ElementName => "IndexSet";

        #endregion

        /// <summary>
        /// Default property for accessing Image Sets
        /// </summary>
        /// <param name="setName"></param>
        /// <returns></returns>
        public new IndexSet this[string setName] => (IndexSet)this.BaseGet(setName);
    }
}