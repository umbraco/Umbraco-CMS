using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BasePages;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Web.UI.Umbraco.Dialogs
{
    public partial class ChangeDocType : UmbracoEnsuredPage
    {
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
            // Get all content types
            var documentTypes = ApplicationContext.Current.Services.ContentTypeService.GetAllContentTypes();

            // Save a flag if the allowed at root option has been set for any document types (if not, then all are allowed there)
            var haveTypesAllowedAtRootBeenDefined = documentTypes.Any(x => x.AllowedAsRoot);

            // Remove current one
            documentTypes = documentTypes.Where(x => x.Id != _content.ContentType.Id);

            // Remove any not valid for current location
            if (_content.ParentId == -1 && haveTypesAllowedAtRootBeenDefined)
            {
                // Root content, and at least one type has been defined as allowed at root, so only include those that have
                // been selected as allowed
                documentTypes = documentTypes.Where(x => x.AllowedAsRoot);
            }
            else
            {
                // Below root, so only include those allowed as sub-nodes for the parent
                var parentNode = ApplicationContext.Current.Services.ContentService.GetById(_content.ParentId);
                documentTypes = documentTypes.Where(x => parentNode.ContentType.AllowedContentTypes
                    .Select(y => y.Id.Value)
                    .Contains(x.Id));
            }

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
            PropertyMappingRepeater.DataSource = _content.ContentType.PropertyTypes;
            PropertyMappingRepeater.DataBind();
        }

        private void PopulatePropertyMappingWithDestinations()
        {
            // Get selected new document type
            var contentType = GetSelectedDocumentType();

            // Loop through list of source properties and populate destination options with all those of same property type
            foreach (RepeaterItem ri in PropertyMappingRepeater.Items)
            {
                if (ri.ItemType == ListItemType.Item || ri.ItemType == ListItemType.AlternatingItem)
                {
                    // Get data type from hidden field
                    var dataTypeId = Guid.Parse(((HiddenField)ri.FindControl("DataTypeId")).Value);

                    // Bind destination list with properties that match data type
                    var ddl = (DropDownList)ri.FindControl("DestinationProperty");
                    ddl.DataSource = contentType.PropertyTypes.Where(x => x.DataTypeId == dataTypeId);                    
                    ddl.DataValueField = "Alias";
                    ddl.DataTextField = "Name";
                    ddl.DataBind();
                    ddl.Items.Add(new ListItem("<" + global::umbraco.ui.Text("changeDocType", "none") + ">", string.Empty));

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

        private void DisplayNotAvailable()
        {
            NewTypePropertyPanel.Visible = false;
            NewTemplatePropertyPanel.Visible = false;
            SavePlaceholder.Visible = false;
            NotAvailablePlaceholder.Visible = true;
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
                var propertyValues = SaveMappedPropertyValues();

                // Get flag for if content already published
                var wasPublished = _content.Published;
                
                // Change the document type
                var newContentType = GetSelectedDocumentType();
                _content.ChangeContentType(newContentType);

                // Set the template if one has been selected
                if (NewTemplateList.SelectedItem != null)
                {
                    var templateId = int.Parse(NewTemplateList.SelectedItem.Value);
                    if (templateId > 0)
                    {
                        var template = ApplicationContext.Current.Services.FileService.GetTemplate(templateId);
                        _content.Template = template;
                    }
                }

                // Set the property values
                foreach (var propertyValue in propertyValues)
                {
                    _content.SetValue(propertyValue.Key, propertyValue.Value);
                }

                // Save
                var user = global::umbraco.BusinessLogic.User.GetCurrent();
                ApplicationContext.Current.Services.ContentService.Save(_content, user.Id);

                // Publish if the content was already published
                if (wasPublished)
                {
                    ApplicationContext.Current.Services.ContentService.Publish(_content, user.Id);
                }

                SuccessPlaceholder.Visible = true;
                SaveAndCancelPlaceholder.Visible = false;
                ValidationPlaceholder.Visible = false;
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

        private IList<KeyValuePair<string, object>> SaveMappedPropertyValues()
        {
            // Create list of mapped property values for assignment after the document type is changed
            var mappedPropertyValues = new List<KeyValuePair<string, object>>();
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
                        mappedPropertyValues.Add(new KeyValuePair<string,object>(mappedAlias, sourcePropertyValue));
                    }
                }
            }

            return mappedPropertyValues;
        }
    }
}