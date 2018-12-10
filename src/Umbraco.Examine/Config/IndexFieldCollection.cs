using System.Configuration;

namespace Umbraco.Examine.Config
{
    public sealed class IndexFieldCollection : ConfigurationElementCollection
    {
        #region Overridden methods to define collection
        protected override ConfigurationElement CreateNewElement()
        {
            return new ConfigIndexField();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            ConfigIndexField field = (ConfigIndexField)element;
            return field.Name;
        }

        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion

        /// <summary>
        /// Adds an index field to the collection
        /// </summary>
        /// <param name="field"></param>
        public void Add(ConfigIndexField field)
        {
            BaseAdd(field, true);
        }

        /// <summary>
        /// Default property for accessing an IndexField definition
        /// </summary>
        /// <value>Field Name</value>
        /// <returns></returns>
        public new ConfigIndexField this[string name]
        {
            get
            {
                return (ConfigIndexField)this.BaseGet(name);
            }
        }

    }
}