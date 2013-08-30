using System.Web.Mvc;
using Umbraco.Core;

namespace Umbraco.Web.Mvc
{
    internal static class ViewDataDictionaryExtensions
    {
        /// <summary>
        /// Merges the source view data into the destination view data
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        public static void MergeViewDataFrom(this ViewDataDictionary destination, ViewDataDictionary source)
        {
            destination.MergeLeft(source);
        }
    }
}