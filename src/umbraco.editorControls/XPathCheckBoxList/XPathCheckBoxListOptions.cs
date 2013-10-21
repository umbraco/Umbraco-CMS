using System;
using System.ComponentModel;

namespace umbraco.editorControls.XPathCheckBoxList
{

    /// <summary>
    /// DataType configuration options for the XPath CheckBoxList
    /// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class XPathCheckBoxListOptions : AbstractOptions
    {
        /// <summary>
        /// string guid for either a node, media or member
        /// </summary>
        private string type = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="XPathCheckBoxListOptions"/> class.
        /// </summary>
        public XPathCheckBoxListOptions()
            : base(true)
        {
        }

        /// <summary>
        /// Gets or sets the guid as a string representing either a Document, Media or Member
        /// </summary>
        public string Type
        {
            get
            {
                // null check for the type, as older versions of this data type won't have this value stored
                if (this.type == null)
                {
                    return uQuery.UmbracoObjectType.Document.GetGuid().ToString();
                }

                return this.type;
            }

            set
            {
                this.type = value;
            }
        }

        /// <summary>
        /// Gets or sets the XPath string used to get the Nodes, Media or Members
        /// </summary>     
        [DefaultValue("//*")]
        public string XPath { get; set; }

        /// <summary>
        /// Defaults to true, where the property value will be stored as an Xml Fragment, else if false, a Csv will be stored
        /// </summary>
        [DefaultValue(true)]
        public bool UseXml { get; set; }

        /// <summary>
        /// Defaults to true, where property value stored is NodeIds, else if false, then value stored is the Node Names
        /// </summary>
        [DefaultValue(true)]
        public bool UseIds { get; set; }

        /// <summary>
        /// Gets the UmbracoObjectType from the stored string guid
        /// </summary>
        /// <value>a Document, Media or Member</value>
        public uQuery.UmbracoObjectType UmbracoObjectType
        {
            get
            {
                return uQuery.GetUmbracoObjectType(new Guid(this.Type));
            }
        }
    }
}