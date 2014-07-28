using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Http;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Am abstract API controller providing functionality used for dealing with content and media types
    /// </summary>
    [PluginController("UmbracoApi")]    
    public abstract class ContentAndMediaTypeBaseController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ContentAndMediaTypeBaseController()
            : this(UmbracoContext.Current)
        {            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        public ContentAndMediaTypeBaseController(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
        }

        /// <summary>
        /// Returns the container configuration JSON structure for the content item id passed in
        /// </summary>
        /// <param name="contentId"></param>
        public ContentTypeContainerConfiguration GetContainerConfig(int contentId)
        {
            var contentItem = Services.ContentService.GetById(contentId);            
            if (contentItem == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            if (!string.IsNullOrEmpty(contentItem.ContentType.ContainerConfig))
            {
                var containerConfig = JsonConvert.DeserializeObject<ContentTypeContainerConfiguration>(contentItem.ContentType.ContainerConfig);

                // Populate the column headings and localization keys
                if (!string.IsNullOrEmpty(containerConfig.AdditionalColumnAliases))
                {
                    var aliases = containerConfig.AdditionalColumnAliases.Split(',');
                    var headings = new string[aliases.Length];
                    var localizationKeys = new string[aliases.Length];

                    // Find all the properties for doc types that might be in the list
                    var allowedContentTypeIds = contentItem.ContentType.AllowedContentTypes
                        .Select(x => x.Id.Value)
                        .ToArray();
                    var allPropertiesOfAllowedContentTypes = Services.ContentTypeService
                        .GetAllContentTypes(allowedContentTypeIds)
                        .SelectMany(x => x.PropertyTypes)
                        .ToList();

                    for (int i = 0; i < aliases.Length; i++)
                    {
                        // Try to find heading from custom property (getting the name from the alias)
                        // - need to look in children of the current content's content type
                        var property = allPropertiesOfAllowedContentTypes
                            .FirstOrDefault(x => x.Alias == aliases[i].ToFirstLower());
                        if (property != null)
                        {
                            headings[i] = property.Name;
                        }
                        else if (aliases[i] == "UpdateDate")
                        {
                            // Special case to restore hard-coded column titles
                            headings[i] = "Last edited";
                            localizationKeys[i] = "defaultdialogs_lastEdited";
                        }
                        else if (aliases[i] == "Owner")
                        {
                            // Special case to restore hard-coded column titles (2)
                            headings[i] = "Updated by";
                            localizationKeys[i] = "content_updatedBy";
                        }
                        else
                        {
                            // Otherwise just sentence case the alias
                            headings[i] = aliases[i].ToFirstUpper().SplitPascalCasing();
                            localizationKeys[i] = "content_" + aliases[i].ToFirstLower();
                        }
                    }

                    containerConfig.AdditionalColumnHeaders = string.Join(",", headings);
                    containerConfig.AdditionalColumnLocalizationKeys = string.Join(",", localizationKeys);
                }

                return containerConfig;
            }

            return null;
        }
    }
}