using System;
using Umbraco.Core.Models;
using umbraco.interfaces;
using System.Web;

namespace Umbraco.Core.Dynamics
{
	internal class PropertyResult : IDocumentProperty, IHtmlString
    {
		public PropertyResult(IDocumentProperty source, PropertyResultType type)
        {
    		if (source == null) throw new ArgumentNullException("source");

    		Alias = source.Alias;
			Value = source.Value;
			Version = source.Version;
			PropertyType = type;
        }
        public PropertyResult(string alias, object value, Guid version, PropertyResultType type)
        {
        	if (alias == null) throw new ArgumentNullException("alias");
        	if (value == null) throw new ArgumentNullException("value");

        	Alias = alias;
			Value = value;
            Version = version;
        	PropertyType = type;
        }

		internal PropertyResultType PropertyType { get; private set; }
		
    	public string Alias { get; private set; }

    	public object Value { get; private set; }

		public string ValueAsString
		{
			get { return Value == null ? "" : Convert.ToString(Value); }
		}

    	public Guid Version { get; private set; }

    
        public bool HasValue()
        {
			return !ValueAsString.IsNullOrWhiteSpace();
        }

		/// <summary>
		/// The Id of the document for which this property belongs to
		/// </summary>
        public int DocumentId { get; set; }

		/// <summary>
		/// The alias of the document type alias for which this property belongs to
		/// </summary>
        public string DocumentTypeAlias { get; set; }

        public string ToHtmlString()
        {
			return ValueAsString;
        }
    }
}
