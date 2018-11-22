using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This is no longer used and will be removed in future versions")]
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