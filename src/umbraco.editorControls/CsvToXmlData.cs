using System;
using System.Xml;
using umbraco;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls
{
    /// <summary>
    /// Overrides the <see cref="umbraco.cms.businesslogic.datatype.DefaultData"/> object to return the value as XML.
    /// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class CsvToXmlData : umbraco.cms.businesslogic.datatype.DefaultData
    {
        /// <summary>
        /// The separators to split the delimited string.
        /// </summary>
        private string[] separator;

        /// <summary>
        /// Name for the root node.
        /// </summary>
        private string rootName;

        /// <summary>
        /// Name for the element/node that contains the value.
        /// </summary>
        private string elementName;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvToXmlData"/> class.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        public CsvToXmlData(umbraco.cms.businesslogic.datatype.BaseDataType dataType)
            : this(dataType, "values")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvToXmlData"/> class.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="rootName">Name of the root.</param>
        public CsvToXmlData(umbraco.cms.businesslogic.datatype.BaseDataType dataType, string rootName)
            : this(dataType, rootName, "value")
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CsvToXmlData"/> class.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="rootName">Name of the root.</param>
        /// <param name="elementName">Name of the element.</param>
        public CsvToXmlData(umbraco.cms.businesslogic.datatype.BaseDataType dataType, string rootName, string elementName)
            : this(dataType, rootName, elementName, new[] { "," })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvToXmlData"/> class.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="rootName">Name of the root.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="separator">The separator.</param>
        public CsvToXmlData(umbraco.cms.businesslogic.datatype.BaseDataType dataType, string rootName, string elementName, string[] separator)
            : base(dataType)
        {
            this.rootName = rootName;
            this.elementName = elementName;
            this.separator = separator;
        }

        /// <summary>
        /// Converts the data value to XML.
        /// </summary>
        /// <param name="data">The data to convert to XML.</param>
        /// <returns>Returns the data value as an XmlNode</returns>
        public override XmlNode ToXMl(XmlDocument data)
        {
            // check that the value isn't null
            if (this.Value != null)
            {
                // split the CSV data into an XML document.
                var xml = xmlHelper.Split(new XmlDocument(), this.Value.ToString(), this.separator, this.rootName, this.elementName);

                // return the XML node.
                return data.ImportNode(xml.DocumentElement, true);
            }
            else
            {
                // otherwise render the value as default (in CDATA)
                return base.ToXMl(data);
            }
        }
    }
}