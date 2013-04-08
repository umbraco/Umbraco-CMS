using System;
using System.Linq;
using System.Xml;

namespace Umbraco.Core
{

	/// <summary>
	/// Extension methods for xml objects
	/// </summary>
	internal static class XmlExtensions
	{
        public static bool HasAttribute(this XmlAttributeCollection attributes, string attributeName)
        {
            return attributes.Cast<XmlAttribute>().Any(x => x.Name == attributeName);
        }

		public static T AttributeValue<T>(this XmlNode xml, string attributeName)
		{
			if (xml == null) throw new ArgumentNullException("xml");
			if (xml.Attributes == null) return default(T);

			if (xml.Attributes[attributeName] == null)
				return default(T);

			var val = xml.Attributes[attributeName].Value;
			var result = val.TryConvertTo<T>();
			if (result.Success)
				return result.Result;

			return default(T);
		}

	}
}