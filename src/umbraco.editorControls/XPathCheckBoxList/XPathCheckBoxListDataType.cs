using System;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;

namespace umbraco.editorControls.XPathCheckBoxList
{
    /// <summary>
    /// This datatype will render a CheckBoxList where the options are defined by an XPath expression,
    /// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class XPathCheckBoxListDataType : umbraco.cms.businesslogic.datatype.BaseDataType, IDataType
    {
        /// <summary>
        /// Field for the preValueEditor.
        /// </summary>
        private XPathCheckBoxListPreValueEditor preValueEditor;

        /// <summary>
        /// Field for the dataEditor.
        /// </summary>
        private IDataEditor dataEditor;

        /// <summary>
        /// Field for the data.
        /// </summary>
        private IData data;

        /// <summary>
        /// Field for the options.
        /// </summary>
        private XPathCheckBoxListOptions options;

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        public XPathCheckBoxListOptions Options
        {
            get
            {
                if (this.options == null)
                {
                    this.options = ((XPathCheckBoxListPreValueEditor)this.PrevalueEditor).Options;
                }

                return this.options;
            }
        }

        /// <summary>
        /// Gets the name of the data type.
        /// </summary>
        /// <value>The name of the data type.</value>
        public override string DataTypeName
        {
            get
            {
                return "XPath CheckBoxList";
            }
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        public override Guid Id
        {
            get
            {
                return new Guid(DataTypeGuids.XPathCheckBoxListId);
            }
        }

        /// <summary>
        /// Lazy load the associated PreValueEditor instance, 
        /// this is constructed supplying 'this'
        /// </summary>
        public override IDataPrevalue PrevalueEditor
        {
            get
            {
                if (this.preValueEditor == null)
                {
                    this.preValueEditor = new XPathCheckBoxListPreValueEditor(this);
                }

                return this.preValueEditor;
            }
        }

        /// <summary>
        /// Lazy load the assocated DataEditor, 
        /// this is constructed supplying the data value stored by the PreValueEditor, and also the configuration settings of the PreValueEditor 
        /// </summary>
        public override IDataEditor DataEditor
        {
            get
            {
                if (this.dataEditor == null)
                {
                    this.dataEditor = new XPathCheckBoxListDataEditor(this.Data, this.Options);
                }

                return this.dataEditor;
            }
        }

        /// <summary>
        /// Lazy load an empty DefaultData object, this is used to pass data between the PreValueEditor and the DataEditor
        /// </summary>
        public override IData Data
        {
            get
            {
                if (this.data == null)
                {
                    if (((XPathCheckBoxListPreValueEditor)this.PrevalueEditor).Options.UseXml)
                    {
                        // Storing an Xml fragment
                        this.data = new XmlData(this);
                    }
                    else
                    {
                        // Storing a Csv
                        this.data = new umbraco.cms.businesslogic.datatype.DefaultData(this);
                    }
                }

                return this.data;
            }
        }
    }
}