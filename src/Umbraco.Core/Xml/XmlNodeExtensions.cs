using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

// source: mvpxml.codeplex.com

namespace Umbraco.Core.Xml
{
	internal static class XmlNodeExtensions
	{
        static XPathNodeIterator Select(string expression, XPathNavigator source, params XPathVariable[] variables)
		{
			var expr = source.Compile(expression);
			var context = new DynamicContext();
			foreach (var variable in variables)
				context.AddVariable(variable.Name, variable.Value);
			expr.SetContext(context);
			return source.Select(expr);
		}

        public static XmlNodeList SelectNodes(this XmlNode source, string expression, IEnumerable<XPathVariable> variables)
        {
            var av = variables == null ? null : variables.ToArray();
            return SelectNodes(source, expression, av);
        }

        public static XmlNodeList SelectNodes(this XmlNode source, string expression, params XPathVariable[] variables)
        {
            if (variables == null || variables.Length == 0 || variables[0] == null)
                return source.SelectNodes(expression);

			var iterator = Select(expression, source.CreateNavigator(), variables);
			return XmlNodeListFactory.CreateNodeList(iterator);
		}

        public static XmlNode SelectSingleNode(this XmlNode source, string expression, IEnumerable<XPathVariable> variables)
        {
            var av = variables == null ? null : variables.ToArray();
            return SelectSingleNode(source, expression, av);
        }
        
        public static XmlNode SelectSingleNode(this XmlNode source, string expression, params XPathVariable[] variables)
		{
            if (variables == null || variables.Length == 0 || variables[0] == null)
                return source.SelectSingleNode(expression);
            
            return SelectNodes(source, expression, variables).Cast<XmlNode>().FirstOrDefault();
		}
	}
}
