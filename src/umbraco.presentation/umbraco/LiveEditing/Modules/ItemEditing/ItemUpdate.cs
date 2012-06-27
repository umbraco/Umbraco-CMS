using System;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.propertytype;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.presentation.LiveEditing.Updates;
using umbraco.BusinessLogic;
using umbraco.BasePages;
using System.Web;
using System.Collections.Generic;

namespace umbraco.presentation.LiveEditing.Modules.ItemEditing
{
    /// <summary>
    /// Class that holds information about an update to a certain field of a certain node.
    /// </summary>
    [Serializable]
    public class ItemUpdate : IUpdate
    {
        /// <summary>
        /// Gets or sets the node id.
        /// </summary>
        /// <value>The node id.</value>
        public int? NodeId { get; set; }

        /// <summary>
        /// Gets or sets the field name.
        /// </summary>
        /// <value>The field name.</value>
        public string Field { get; set; }

        /// <summary>
        /// Gets or sets the field data.
        /// </summary>
        /// <value>The field data.</value>
        public object Data { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldUpdate"/> class.
        /// </summary>
        /// <param name="nodeId">The node id.</param>
        /// <param name="field">The field.</param>
        /// <param name="data">The data.</param>
        public ItemUpdate(int? nodeId, string field, object data)
        {
            NodeId = nodeId;
            Field = field;
            Data = data;
        }

        #region IUpdate Members


        /// <summary>
        /// Saveses this instance.
        /// </summary>
        public void Save()
        {
            // get the data
            Document document = new Document(NodeId.Value);
            Property editProperty  = document.getProperty(Field);

            IDataType editDataType = null;
            if (editProperty != null)
            {
                // save the property
                PropertyType editPropertyType = editProperty.PropertyType;
                editDataType = editPropertyType.DataTypeDefinition.DataType;
                editDataType.Data.PropertyId = editProperty.Id;
                editDataType.Data.Value = Data;
            }
            else
            {
                if (Field == "pageName")
                {
                    document.Text = Data.ToString();
                }
            }

            document.Save();
        }

        /// <summary>
        /// Publishes the update.
        /// </summary>
        public void Publish()
        {
            // keep track of documents published in this request
            List<int> publishedDocuments = (List<int>)HttpContext.Current.Items["ItemUpdate_PublishedDocuments"];
            if (publishedDocuments == null)
            {
                HttpContext.Current.Items["ItemUpdate_PublishedDocuments"] = publishedDocuments = new List<int>();
            }

            // publish each modified document just once (e.g. several fields are updated)
            if(!publishedDocuments.Contains(NodeId.Value))
            {
                Document document = new Document(NodeId.Value);
                document.Publish(UmbracoEnsuredPage.CurrentUser);
                library.UpdateDocumentCache(NodeId.Value);

                publishedDocuments.Add(NodeId.Value);
            }
        }

        #endregion
    }
}
