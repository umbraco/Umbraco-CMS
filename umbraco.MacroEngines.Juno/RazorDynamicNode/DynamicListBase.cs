using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Dynamic;

namespace umbraco.MacroEngines
{
    public class DynamicListBase : DynamicObject, IEnumerable
    {
        public List<DynamicBase> Items { get; set; }

        public IEnumerator GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}
