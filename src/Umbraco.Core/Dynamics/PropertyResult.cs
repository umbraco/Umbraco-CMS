using System;
using Umbraco.Core.Models;
using umbraco.interfaces;
using System.Web;

namespace Umbraco.Core.Dynamics
{
	internal class PropertyResult : IDocumentProperty, IHtmlString
    {
    	public PropertyResult(IDocumentProperty source)
        {
            if (source != null)
            {
                this.Alias = source.Alias;
                this.Value = source.Value;
                this.Version = source.Version;
            }
        }
        public PropertyResult(string alias, string value, Guid version)
        {
            this.Alias = alias;
            this.Value = value;
            this.Version = version;
        }

    	public string Alias { get; private set; }

    	public string Value { get; private set; }

    	public Guid Version { get; private set; }

    	public bool IsNull()
        {
            return Value == null;
        }
        public bool HasValue()
        {
            return !string.IsNullOrWhiteSpace(Value);
        }

        public int ContextId { get; set; }
        public string ContextAlias { get; set; }

        public string ToHtmlString()
        {
            return Value;
        }
    }
}
