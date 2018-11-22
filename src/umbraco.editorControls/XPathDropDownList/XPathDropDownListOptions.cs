using System;
using System.ComponentModel;

namespace umbraco.editorControls.XPathDropDownList
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    internal class XPathDropDownListOptions : AbstractOptions
    {
        private string type = null;

        /// <summary>
        /// 
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
        /// XPath string used to get Nodes to be used as CheckBox options in a CheckBoxList
        /// </summary>     
        [DefaultValue("//*")]  
        public string XPath { get; set; }

        /// <summary>
        /// Defaults to true, where property value is a csv of NodeIds, else if false, then csv of Node names is stored
        /// </summary>
        [DefaultValue(true)]
        public bool UseId { get; set; }
        
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

        /// <summary>
        /// Initializes an instance of XPathDropDownListOptions
        /// </summary>
        public XPathDropDownListOptions() : base(true)
        {
        }        
    }
}
