using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.MacroEngines
{
    public static class DynamicNodeListExtensionMethods
    {
        public static DynamicNodeList Random(this DynamicNodeList all, int Min, int Max)
        {
            //get a random number generator
            Random r = new Random();
            //choose the number of elements to be returned between Min and Max
            int Number = r.Next(Min, Max);
            //Call the other method
            return Random(all, Number);
        }
        public static DynamicNodeList Random(this DynamicNodeList all, int Max)
        {
            //Randomly order the items in the set by a Guid, Take the correct number, and return this wrapped in a new DynamicNodeList
            return new DynamicNodeList(all.Items.OrderBy(x => Guid.NewGuid()).Take(Max));
        }
    }
}