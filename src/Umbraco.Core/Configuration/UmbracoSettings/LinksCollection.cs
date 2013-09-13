using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class LinksCollection : ConfigurationElementCollection, IEnumerable<ILink>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new LinkElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((LinkElement)element).Application 
                   + ((LinkElement)element).ApplicationUrl 
                   + ((LinkElement)element).Language
                   + ((LinkElement)element).UserType;
        }

        IEnumerator<ILink> IEnumerable<ILink>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as ILink;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}