using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// A simple representation of an Umbraco property
    /// </summary>
    public class UmbracoProperty
    {
        public string Alias { get; set; }
        public object Value { get; set; }
        public string Name { get; set; }

    }
}
