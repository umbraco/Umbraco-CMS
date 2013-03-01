// source: mvpxml.codeplex.com

namespace Umbraco.Core.Xml
{
	internal class XPathVariable
	{
		public string Name { get; set; }
		public string Value { get; set; }

		public XPathVariable(string name, string value)
		{
			Name = name;
			Value = value;
		}
	}
}
