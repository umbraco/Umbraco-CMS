using System;
using System.Collections;
using System.Linq;

namespace umbraco.NodeFactory
{
	public class Properties : CollectionBase
    {
        public virtual void Add(Property NewProperty)
        {
            List.Add(NewProperty);
        }

        public virtual Property this[int Index]
        {
            get { return (Property)List[Index]; }
        }

        public virtual Property this[string alias]
        {
            get
            {
                return List.OfType<Property>()
                    .Where(x => x.Alias.Equals(alias, StringComparison.InvariantCultureIgnoreCase))
                    .FirstOrDefault();
            }
        }
    }
}