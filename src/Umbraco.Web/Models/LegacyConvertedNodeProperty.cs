using System;
using System.Web;
using Umbraco.Core.Models;
using umbraco.interfaces;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// A legacy IProperty that wraps IPublishedProperty
    /// </summary>
    internal class LegacyConvertedNodeProperty : IProperty, IHtmlString
    {
        public IPublishedProperty PublishedProperty { get; private set; }

        public LegacyConvertedNodeProperty(IPublishedProperty prop)
        {
            PublishedProperty = prop;
        }

        public string Alias
        {
            get { return PublishedProperty.PropertyTypeAlias; }
        }

        public string Value
        {
            get { return PublishedProperty.DataValue == null ? null : PublishedProperty.DataValue.ToString(); }
        }

        public Guid Version
        {
            get { return Guid.Empty; }
        }

        public bool IsNull()
        {
            return Value == null;
        }

        public bool HasValue()
        {
            return PublishedProperty.HasValue;
        }

        public int ContextId { get; set; }
        public string ContextAlias { get; set; }

        // implements IHtmlString.ToHtmlString
        public string ToHtmlString()
        {
            return Value;
        }
    }
}