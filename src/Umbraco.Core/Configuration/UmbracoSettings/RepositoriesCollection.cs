using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class RepositoriesCollection : ConfigurationElementCollection, IEnumerable<RepositoryElement>
    {
        internal void Add(RepositoryElement item)
        {
            BaseAdd(item);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new RepositoryElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RepositoryElement)element).Id;
        }

        IEnumerator<RepositoryElement> IEnumerable<RepositoryElement>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as RepositoryElement;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}