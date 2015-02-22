using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BasePages;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Web.UI.Umbraco.Dialogs
{
    public partial class ChangeDocType : UmbracoEnsuredPage
    {
        class PropertyMapping
        {
            public string FromName { get; set; }
            public string ToName { get; set; }
            public string ToAlias { get; set; }
            public object Value { get; set; }
        }

        private IContent _content;

        protected void Page_Load(object sender, EventArgs e)
        {
            var contentNodeId = int.Parse(Request.QueryString["id"]);
            _content = ApplicationContext.Current.Services.ContentService.GetById(contentNodeId);

            LocalizeTexts();

            if (!Page.IsPostBack)
            {
                DisplayContentDetails();
                if (PopulateListOfValidAlternateDocumentTypes())
                {
                    PopulateListOfTemplates();
                    PopulatePropertyMappingWithSources();
                    PopulatePropertyMappingWithDestinations();
                }
                else
                {
                    DisplayNotAvailable();
                }
            }
        }

        private void LocalizeTexts()
        {
            ChangeDocTypePane.Text = global::umbraco.ui.Text("changeDocType", "selectNewDocType");
            ContentNamePropertyPanel.Text = global::umbraco.ui.Text("changeDocType", "selectedContent");
            CurrentTypePropertyPanel.Text = global::umbraco.ui.Text("changeDocType", "currentType");
            NewTypePropertyPanel.Text = global::umbraco.ui.Text("changeDocType", "newType");
            NewTemplatePropertyPanel.Text = global::umbraco.ui.Text("changeDocType", "newTemplate");
            ChangeDocTypePropertyMappingPane.Text = global::umbraco.ui.Text("changeDocType", "mapProperties");
            ValidateAndSave.Text = global::umbraco.ui.Text("buttons", "save");
        }

        private void DisplayContentDetails()
        {
            ContentNameLabel.Text = _content.Name;
            CurrentTypeLabel.Text = _content.ContentType.Name;
        }

        private bool PopulateListOfValidAlternateDocumentTypes()
        {
            // Start with all content types
            var documentTypes = ApplicationContext.Current.Services.ContentTypeService.GetAllContentTypes();

            // Remove invalid ones from list of potential alternatives
            documentTypes = RemoveCurrentDocumentTypeFromAlternatives(documentTypes);
            documentTypes = RemoveInvalidByParentDocumentTypesFromAlternatives(documentTypes);
            documentTypes = RemoveInvalidByChildrenDocumentTypesFromAlternatives(documentTypes);

            // If we have at least one, bind to list and return true
            if (documentTypes.Any())
            {
                NewDocumentTypeList.DataSource = documentTypes.OrderBy(x => x.Name);
                NewDocumentTypeList.DataValueField = "Id";
                NewDocumentTypeList.DataTextField = "Name";
                NewDocumentTypeList.DataBind();
                return true;
            }

            return false;
        }

        private IEnumerable<IContentType> RemoveCurrentDocumentTypeFromAlternatives(IEnumerable<IContentType> documentTypes)
        {
            return documentTypes
                .Where(x => x.Id != _content.ContentType.Id);
        }

        private IEnumerable<IContentType> RemoveInvalidByParentDocumentTypesFromAlternatives(IEnumerable<IContentType> documentTypes)
        {
            if (_content.ParentId == -1)
            {
                // Root content, only include those that have been selected as allowed at root
                return documentTypes
                    .Where(x => x.AllowedAsRoot);
            }
            else
            {
                // Below root, so only include those allowed as sub-nodes for the parent
                var parentNode = ApplicationContext.Current.Services.ContentService.GetById(_content.ParentId);
                return documentTypes
                    .Where(x => parentNode.ContentType.AllowedContentTypes
                        .Select(y => y.Id.Value)
                        .Contains(x.Id));
            }
        }

        private IEnumerable<IContentType> RemoveInvalidByChildrenDocumentTypesFromAlternatives(IEnumerable<IContentType> documentTypes)
        {
            var docTypeIdsOfChildren = _content.Children()
                .Select(x => x.ContentType.Id)
                .Distinct()
                .ToList();
            return documentTypes
                .Where(x => x.AllowedContentTypes
                    .Select(y => y.Id.Value)
                    .ContainsAll(docTypeIdsOfChildren));
        }

        private void PopulateListOfTemplates()
        {
            // Get selected new document type
            var contentType = GetSelectedDocumentType();

            // Populate template list
            NewTemplateList.DataSource = contentType.AllowedTemplates;
            NewTemplateList.DataValueField = "Id";
            NewTemplateList.DataTextField = "Name";
            NewTemplateList.DataBind();
            NewTemplateList.Items.Add(new ListItem("<" + global::umbraco.ui.Text("changeDocType", "none") + ">", "0"));

            // Set default template
            if (contentType.DefaultTemplate != null)
            {
                var itemToSelect = NewTemplateList.Items.FindByValue(contentType.DefaultTemplate.Id.ToString());
                if (itemToSelect != null)
                {
                    itemToSelect.Selected = true;
                }
            }
        }

        private void PopulatePropertyMappingWithSources()
        {
            PropertyMappingRepeater.DataSource = GetPropertiesOfContentType(_content.ContentType);
            PropertyMappingRepeater.DataBind();
        }

        private void PopulatePropertyMappingWithDestinations()
        {
            // Get selected new document type
            var contentType = GetSelectedDocumentType();

            // Get properties of new document type (including any from parent types)
            var properties = GetPropertiesOfContentType(contentType);

            // Loop through list of source properties and populate destination options with all those of same property type
            foreach (RepeaterItem ri in PropertyMappingRepeater.Items)
            {
                if (ri.ItemType == ListItemType.Item || ri.ItemType == ListItemType.AlternatingItem)
                {
                    // Get data type from hidden field
                    var propEdAlias = ((HiddenField)ri.FindControl("PropertyEditorAlias")).Value;

                    // Bind destination list with properties that match data type
                    var ddl = (DropDownList)ri.FindControl("DestinationProperty");
                    ddl.DataSource = properties.Where(x => x.PropertyEditorAlias == propEdAlias);
                    ddl.DataValueField = "Alias";
                    ddl.DataTextField = "Name";
                    ddl.DataBind();
                    ddl.Items.Insert(0, new ListItem("<" + global::umbraco.ui.Text("changeDocType", "none") + ">", string.Empty));

                    // Set default selection to be one with matching alias
                    var alias = ((HiddenField)ri.FindControl("Alias")).Value;
                    var item = ddl.Items.FindByValue(alias);
                    if (item != null)
                    {
                        item.Selected = true;
                    }
                }
            }
        }

        private IContentType GetSelectedDocumentType()
        {
            return ApplicationContext.Current.Services.ContentTypeService.GetContentType(int.Parse(NewDocumentTypeList.SelectedItem.Value));
        }

        private IEnumerable<PropertyType> GetPropertiesOfContentType(IContentType contentType)
        {
            var properties = contentType.PropertyTypes.ToList();
            while (contentType.ParentId > -1)
            {
                contentType = ApplicationContext.Current.Services.ContentTypeService.GetContentType(contentType.ParentId);
                properties.AddRange(contentType.PropertyTypes);
            }

            return properties.OrderBy(x => x.Name);
        }

        private void DisplayNotAvailable()
        {
            NewTypePropertyPanel.Visible = false;
            NewTemplatePropertyPanel.Visible = false;
            SavePlaceholder.Visible = false;
            NotAvailablePlaceholder.Visible = true;
            ChangeDocTypePropertyMappingPane.Visible = false;
        }

        protected void NewDocumentTypeList_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateListOfTemplates();
            PopulatePropertyMappingWithDestinations();
        }

        protected void ValidateAndSave_Click(object sender, EventArgs e)
        {
            if (IsPropertyMappingValid())
            {
                // For all properties to be mapped, save the values to a temporary list
                var propertyMappings = SavePropertyMappings();

                // Get flag for if content already published
                var wasPublished = _content.Published;

                // Change the document type passing flag to clear the properties
                var newContentType = GetSelectedDocumentType();
                _content.ChangeContentType(newContentType, true);

                // Set the template if one has been selected
                if (NewTemplateList.SelectedItem != null)
                {
                    var templateId = int.Parse(NewTemplateList.SelectedItem.Value);
                    _content.Template = templateId > 0 ? ApplicationContext.Current.Services.FileService.GetTemplate(templateId) : null;
                }

                // Set the property values
                var propertiesMappedMessageBuilder = new StringBuilder("<ul>");
                foreach (var propertyMapping in propertyMappings)
                {
                    propertiesMappedMessageBuilder.AppendFormat("<li>{0} {1} {2}</li>",
                        propertyMapping.FromName, global::umbraco.ui.Text("changeDocType", "to"), propertyMapping.ToName);
                    _content.SetValue(propertyMapping.ToAlias, propertyMapping.Value);
                }
                propertiesMappedMessageBuilder.Append("</ul>");

                // Save
                var user = global::umbraco.BusinessLogic.User.GetCurrent();
                ApplicationContext.Current.Services.ContentService.Save(_content, user.Id);

                // Publish if the content was already published
                if (wasPublished)
                {
                    ApplicationContext.Current.Services.ContentService.Publish(_content, user.Id);
                }

                // Sync the tree
                ClientTools.SyncTree(_content.Path, true);

                // Reload the page if the content was already being viewed
                ClientTools.ReloadContentFrameUrlIfPathLoaded("/editContent.aspx?id=" + _content.Id);

                // Display success message
                SuccessMessage.Text = global::umbraco.ui.Text("changeDocType", "successMessage").Replace("[new type]", "<strong>" + newContentType.Name + "</strong>");
                PropertiesMappedMessage.Text = propertiesMappedMessageBuilder.ToString();
                if (wasPublished)
                {
                    ContentPublishedMessage.Text = global::umbraco.ui.Text("changeDocType", "contentRepublished");
                    ContentPublishedMessage.Visible = true;
                }
                else
                {
                    ContentPublishedMessage.Visible = false;
                }
                SuccessPlaceholder.Visible = true;
                SaveAndCancelPlaceholder.Visible = false;
                ValidationPlaceholder.Visible = false;
                ChangeDocTypePane.Visible = false;
                ChangeDocTypePropertyMappingPane.Visible = false;
            }
            else
            {
                ValidationPlaceholder.Visible = true;
            }
        }

        private bool IsPropertyMappingValid()
        {
            // Check whether any properties have been mapped to more than once
            var mappedPropertyAliases = new List<string>();
            foreach (RepeaterItem ri in PropertyMappingRepeater.Items)
            {
                if (ri.ItemType == ListItemType.Item || ri.ItemType == ListItemType.AlternatingItem)
                {
                    var ddl = (DropDownList)ri.FindControl("DestinationProperty");
                    var mappedPropertyAlias = ddl.SelectedItem.Value;
                    if (!string.IsNullOrEmpty(mappedPropertyAlias))
                    {
                        if (mappedPropertyAliases.Contains(mappedPropertyAlias))
                        {
                            ValidationError.Text = global::umbraco.ui.Text("changeDocType", "validationErrorPropertyWithMoreThanOneMapping");
                            return false;
                        }

                        mappedPropertyAliases.Add(mappedPropertyAlias);
                    }
                }
            }

            return true;
        }

        private IList<PropertyMapping> SavePropertyMappings()
        {
            // Create list of mapped property values for assignment after the document type is changed
            var mappedPropertyValues = new List<PropertyMapping>();
            foreach (RepeaterItem ri in PropertyMappingRepeater.Items)
            {
                if (ri.ItemType == ListItemType.Item || ri.ItemType == ListItemType.AlternatingItem)
                {
                    // Get property alias to map to
                    var ddl = (DropDownList)ri.FindControl("DestinationProperty");
                    var mappedAlias = ddl.SelectedItem.Value;
                    if (!string.IsNullOrEmpty(mappedAlias))
                    {
                        // If mapping property, get current property value from alias
                        var sourceAlias = ((HiddenField)ri.FindControl("Alias")).Value;
                        var sourcePropertyValue = _content.GetValue(sourceAlias);

                        // Add to list
                        mappedPropertyValues.Add(new PropertyMapping
                            {
                                FromName = ((HiddenField)ri.FindControl("Name")).Value,
                                ToName = ddl.SelectedItem.Text,
                                ToAlias = mappedAlias,
                                Value = sourcePropertyValue
                            });
                    }
                }
            }

            return mappedPropertyValues;
        }
    }
}