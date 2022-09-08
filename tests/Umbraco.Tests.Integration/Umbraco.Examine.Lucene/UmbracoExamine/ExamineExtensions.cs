using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using Examine;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Examine.Lucene.UmbracoExamine;

/// <summary>
///     LEGACY!! Static methods to help query umbraco xml
/// </summary>
/// <remarks>
///     This should be deleted when we remove the old xml published content with tests which should be replaced with
///     nucache tests
/// </remarks>
internal static class ExamineExtensions
{
    /// <summary>
    ///     Returns true if the XElement is recognized as an umbraco xml NODE (doc type)
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    internal static bool IsExamineElement(this XElement x)
    {
        var id = (string)x.Attribute("id");
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        if (int.TryParse(id, out int parsedId))
        {
            if (parsedId > 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     This takes into account both schemas and returns the node type alias.
    ///     If this isn't recognized as an element node, this returns an empty string
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    internal static string ExamineNodeTypeAlias(this XElement x) =>
        string.IsNullOrEmpty((string)x.Attribute("nodeTypeAlias"))
            ? x.Name.LocalName
            : (string)x.Attribute("nodeTypeAlias");

    /// <summary>
    ///     Returns umbraco value for a data element with the specified alias.
    /// </summary>
    /// <param name="xml"></param>
    /// <param name="alias"></param>
    /// <returns></returns>
    internal static string SelectExamineDataValue(this XElement xml, string alias)
    {
        XElement nodeData = null;

        //if there is data children with attributes, we're on the old
        if (xml.Elements("data").Any(x => x.HasAttributes))
        {
            nodeData = xml.Elements("data").SingleOrDefault(x =>
                string.Equals((string)x.Attribute("alias"), alias, StringComparison.InvariantCultureIgnoreCase));
        }
        else
        {
            nodeData = xml.Elements().FirstOrDefault(x =>
                string.Equals(x.Name.ToString(), alias, StringComparison.InvariantCultureIgnoreCase));
        }

        if (nodeData == null)
        {
            return string.Empty;
        }

        if (!nodeData.HasElements)
        {
            return nodeData.Value;
        }

        //it has sub elements so serialize them
        var reader = nodeData.CreateReader();
        reader.MoveToContent();
        return reader.ReadInnerXml();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ValueSet ConvertToValueSet(this XElement xml, string indexCategory)
    {
        if (!xml.IsExamineElement())
        {
            throw new InvalidOperationException("Not a supported Examine XML structure");
        }

        var allVals = xml.SelectExamineAllValues();
        var id = (string)xml.Attribute("id");
        //we will use this as the item type, but we also need to add this as the 'nodeTypeAlias' as part of the properties
        //since this is what Umbraco expects
        var nodeTypeAlias = xml.ExamineNodeTypeAlias();

        allVals["nodeTypeAlias"] = nodeTypeAlias;
        var set = new ValueSet(id, indexCategory, nodeTypeAlias, allVals);
        return set;
    }

    internal static Dictionary<string, object> SelectExamineAllValues(this XElement xml)
    {
        var attributeValues = xml.Attributes().ToDictionary(x => x.Name.LocalName, x => x.Value);
        var dataValues = xml.SelectExamineDataValues();
        foreach (var v in attributeValues)
        //override the data values with attribute values if they do match, otherwise add
        {
            dataValues[v.Key] = v.Value;
        }

        return dataValues;
    }

    internal static Dictionary<string, object> SelectExamineDataValues(this XElement xml)
    {
        //resolve all element data at once since it is much faster to do this than to relookup all of the XML data
        //using Linq and the node.Elements() methods re-gets all of them.
        var elementValues = new Dictionary<string, object>();
        foreach (var x in xml.Elements())
        {
            if (x.Attribute("id") != null)
            {
                continue;
            }

            string key;
            if (x.Name.LocalName == "data")
            //it's the legacy schema
            {
                key = (string)x.Attribute("alias");
            }
            else
            {
                key = x.Name.LocalName;
            }

            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            if (!x.HasElements)
            {
                elementValues[key] = x.Value;
            }
            else
            //it has sub elements so serialize them
            {
                using (var reader = x.CreateReader())
                {
                    reader.MoveToContent();
                    elementValues[key] = reader.ReadInnerXml();
                }
            }
        }

        return elementValues;
    }
}
