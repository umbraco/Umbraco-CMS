using System;
using Umbraco.Core.Models;
using umbraco.interfaces;
using System.Web;

namespace Umbraco.Core.Dynamics
{
	internal enum PropertyResultType
	{
		/// <summary>
		/// The property resolved was a normal document property
		/// </summary>
		NormalProperty,

		/// <summary>
		/// The property resolved was a property defined as a member on the document object (IDocument) itself
		/// </summary>
		ReflectedProperty
	}

	internal class PropertyResult : IDocumentProperty, IHtmlString
    {
		public PropertyResult(IDocumentProperty source)
        {
    		if (source == null) throw new ArgumentNullException("source");

    		Alias = source.Alias;
			Value = source.Value;
			Version = source.Version;
			PropertyType = PropertyResultType.NormalProperty;
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
