using System;
using System.Collections.Generic;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{

    internal class ContentAppResolver : IValueResolver<IContent, ContentItemDisplay, IEnumerable<ContentApp>>
    {
        private readonly ContentApp _contentApp = new ContentApp
        {
            Alias = "content",
            Name = "Content",
            Icon = "icon-document",
            View = "views/content/apps/content/content.html"
        };

        private readonly ContentApp _infoApp = new ContentApp
        {
            Alias = "info",
            Name = "Info",
            Icon = "icon-info",
            View = "views/content/apps/info/info.html"
        };

        private readonly IDataTypeService _dataTypeService;
        private readonly PropertyEditorCollection _propertyEditorCollection;

        public ContentAppResolver(IDataTypeService dataTypeService, PropertyEditorCollection propertyEditorCollection)
        {
            _dataTypeService = dataTypeService ?? throw new ArgumentNullException(nameof(dataTypeService));
            _propertyEditorCollection = propertyEditorCollection ?? throw new ArgumentNullException(nameof(propertyEditorCollection));
        }

        public IEnumerable<ContentApp> Resolve(IContent source, ContentItemDisplay destination, IEnumerable<ContentApp> destMember, ResolutionContext context)
        {
            var apps = new List<ContentApp>();

            if (source.ContentType.IsContainer)
            {
                //If it's a container then add the list view app and view model
                apps.AppendListViewApp(_dataTypeService, _propertyEditorCollection, source.ContentType.Alias, "content");
            }

            apps.Add(_contentApp);
            apps.Add(_infoApp);

            return apps;
        }
}
    
}
