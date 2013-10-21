using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using umbraco.cms.businesslogic.datatype; // DefaultData
using umbraco.cms.businesslogic.property; // Property
using umbraco.cms.businesslogic.relation; // RelationType
using umbraco.interfaces; // IDataEditor
using UmbracoContent = umbraco.cms.businesslogic.Content; // UmbracoContent defined here so as to differentiate with System.Web.UI.WebControls.Content

namespace umbraco.editorControls.PickerRelations
{
    /// <summary>
    /// 
    /// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class PickerRelationsDataEditor : CompositeControl, IDataEditor
    {
        /// <summary>
        /// value stored by a datatype instance
        /// </summary>
        private IData data;

        /// <summary>
        /// configuration options for this datatype, as defined by the PreValueEditor
        /// </summary>
        private PickerRelationsOptions options;

        /// <summary>
        /// Literal used to render status (Enabled || Disabled)
        /// this datatype is only enabled and active if it can find the property alias on the current node (as defined in PreValueEditor) and 
        /// the relation type parent object type (or child if reverse index used) matches the object type of the current node which this datatype is on
        /// </summary>
        private Literal statusLiteral = new Literal();


		private Literal jsLiteral = new Literal();

        /// <summary>
        /// Gets a value indicating whether this is an RTE - this Property expected by Umbraco
        /// </summary>
        public virtual bool TreatAsRichTextEditor 
        { 
            get { return false; } 
        }

        /// <summary>
        /// Gets a value indicating whether the label should be shown when editing - this Property exected by Umbraco
        /// </summary>
        public virtual bool ShowLabel 
        { 
            get { return true; } 
        }

        /// <summary>
        /// Gets the DataEditor - Property expected by Umbraco
        /// </summary>
        public Control Editor { get { return this; } }

        /// <summary>
        /// Gets the id of the current (content || media || member) node on which this datatype is a property
        /// </summary>
        private int CurrentContentId
        {
            get
            {
                return ((umbraco.cms.businesslogic.datatype.DefaultData)this.data).NodeId;
            }
        }

        /// <summary>
        /// Gets the UmbracoObjectType on which this datatype is a property of
        /// </summary>
        private uQuery.UmbracoObjectType CurrentContextObjectType
        {
            get
            {
                return uQuery.GetUmbracoObjectType(
                        uQuery.SqlHelper.ExecuteScalar<Guid>(
                            "SELECT nodeObjectType FROM umbracoNode WHERE id = @id",
                            uQuery.SqlHelper.CreateParameter("@id", this.CurrentContentId)));
            }
        }

        /////// NOT CURRENTLY USED, BUT MIGHT BE USEFUL TO MARK BIDIRECTIONAL RELATIONS
        /////// <summary>
        /////// string to identify a particular instance of this datatype
        /////// </summary>
        ////private string InstanceIdentifier
        ////{
        ////    get
        ////    {
        ////        Property pickerRelationsProperty = new Property(((DefaultData)this.data).PropertyId);
        ////        return "[" + pickerRelationsProperty.PropertyType.Id.ToString() + "]";
        ////    }
        ////}

        /// <summary>
        /// Initializes a new instance of PickerRelationsDataEditor
        /// </summary>
        /// <param name="data">data stored by this instance of this datatype (not currently used)</param>
        /// <param name="options">configuration options for this datatype as set by the PreValueEditor</param>
        internal PickerRelationsDataEditor(IData data, PickerRelationsOptions options)
        {
            this.data = data;
            this.options = options;
        }

        /// <summary>
        /// Creates the child controls
        /// </summary>
        protected override void CreateChildControls()
        {
			this.statusLiteral.ID = "pickerRelations";
            this.Controls.Add(this.statusLiteral); // displays if this datatype is valid in this context, and if so, which pickerProperty and relationtype it wires up			
			this.Controls.Add(this.jsLiteral);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.EnsureChildControls();

			this.statusLiteral.Text = "<span id=\"" + this.statusLiteral.ClientID + "\">";

            try
            {
                this.statusLiteral.Text += "Mapping: " + this.GetMappingDetails();
            }
            catch(Exception ex) 
            {
                this.statusLiteral.Text += "Error: " + ex.Message;
            }

			this.statusLiteral.Text += "</span>";

			if (this.options.HideDataEditor)
			{
				this.jsLiteral.Text = @"
				<script type=""text/javascript"">
					jQuery(document).ready(function () {
						jQuery('span#" + this.statusLiteral.ClientID + @"').parents('div.propertypane').first().hide();
					});
				</script>";
			}
        }

        /// <summary>
        /// Called by Umbraco when saving the node, this datatype doens't do anythign here, but with an event handler instead,
        /// as needs to know the saved values of a sibling pickerProperty
        /// </summary>
        public void Save()
        {
        }

        /// <summary>
        /// returns a string "Property '[propertyAlias]' with RelationType '[relationTypeName]"
        /// </summary>
        /// <returns></returns>
        private string GetMappingDetails()
        {
            string mappingDetails = string.Empty;

            UmbracoContent currentContentNode = new UmbracoContent(this.CurrentContentId);
            Property pickerProperty = currentContentNode.getProperty(this.options.PropertyAlias);

            if (pickerProperty != null)
            {
                RelationType relationType = new RelationType(this.options.RelationTypeId); // Does it still exist ? TODO: check

                if (this.IsContextUmbracoObjectTypeValid(this.CurrentContextObjectType, relationType))
                {
                    mappingDetails = "Property '<strong>" + pickerProperty.PropertyType.Name + "</strong>' with " + 
                                     "Relation Type '<strong>" + relationType.Name + "</strong>'";

                    if(this.options.ReverseIndexing)
                    {
                        mappingDetails += " <i>(Reverse Index)</i>";
                    }
                }
                else
                {
                    throw new Exception("Conflict with this Content Object Type and that expected by the Relation Type '" + relationType.Name  + "'");
                }
            }
            else
            {
                throw new Exception("Can't find a Property with the Alias '" + this.options.PropertyAlias + "'");
            }

            return mappingDetails;
        }

        /// <summary>
        /// returns the UmbracoObjectType associated as defined by the supplied relation type, and if reverse indexing has been enabled
        /// </summary>
        /// <param name="relationType">associated RealationType</param>
        /// <returns></returns>
        public uQuery.UmbracoObjectType GetPickerUmbracoObjectType(RelationType relationType)
        {
            uQuery.UmbracoObjectType pickerUmbracoObjectType = uQuery.UmbracoObjectType.Unknown;

            if (!relationType.Dual && this.options.ReverseIndexing)
            {
                pickerUmbracoObjectType = relationType.GetParentUmbracoObjectType();
            }
            else
            {
                pickerUmbracoObjectType = relationType.GetChildUmbracoObjectType();
            }

            return pickerUmbracoObjectType;
        }

		/// <summary>
		/// Check to see if the content id side of the relation type is valid (doesn't check the node picker id side)
		/// </summary>
        /// <param name="contextUmbracoObjectType">Type of the content object (content/ media or member)</param>
		/// <param name="relationType">Type of the relation.</param>
		/// <returns>
		/// 	<c>true</c> if [is content object type valid] [the specified content object type]; otherwise, <c>false</c>.
		/// </returns>
        public bool IsContextUmbracoObjectTypeValid(uQuery.UmbracoObjectType contextUmbracoObjectType, RelationType relationType)
        {
            bool isContextObjectTypeValid = false;

            if (!relationType.Dual && this.options.ReverseIndexing)
            {
                // expects the current context to be the child in the relation
                if (contextUmbracoObjectType == relationType.GetChildUmbracoObjectType())
                {
                    isContextObjectTypeValid = true;
                }
            }
            else
            {
                // expects the current context to be the parent in the relation
                if (contextUmbracoObjectType == relationType.GetParentUmbracoObjectType())
                {
                    isContextObjectTypeValid = true;
                }
            }

            return isContextObjectTypeValid;
        }
    }        
}
