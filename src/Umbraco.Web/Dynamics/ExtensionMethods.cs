using System;
using System.Linq;
using Umbraco.Web.Models;

namespace Umbraco.Web.Dynamics
{
    internal static class ExtensionMethods
    {
        

        public static DynamicPublishedContentList Random(this DynamicPublishedContentList all, int min, int max)
        {
            //get a random number generator
            Random r = new Random();
            //choose the number of elements to be returned between Min and Max
            int Number = r.Next(min, max);
            //Call the other method
            return Random(all, Number);
        }
        public static DynamicPublishedContentList Random(this DynamicPublishedContentList all, int max)
        {
            //Randomly order the items in the set by a Guid, Take the correct number, and return this wrapped in a new DynamicNodeList
            return new DynamicPublishedContentList(all.Items.OrderBy(x => Guid.NewGuid()).Take(max));
        }

        public static DynamicPublishedContent Random(this DynamicPublishedContentList all)
        {
            return all.Items.OrderBy(x => Guid.NewGuid()).First();
        }

    }
}
