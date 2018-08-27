using System;
using System.Collections.Generic;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class MediaAppResolver : IValueResolver<IMedia, MediaItemDisplay, IEnumerable<ContentApp>>
    {
        private static readonly ContentApp _contentApp = new ContentApp
        {
            Alias = "content",
            Name = "Content",
            Icon = "icon-document",
            View = "views/media/apps/content/content.html"
        };

        private static readonly ContentApp _infoApp = new ContentApp
        {
            Alias = "info",
            Name = "Info",
            Icon = "icon-info",
            View = "views/media/apps/info/info.html"
        };

        private readonly IDataTypeService _dataTypeService;
        private readonly PropertyEditorCollection _propertyEditorCollection;

        public MediaAppResolver(IDataTypeService dataTypeService, PropertyEditorCollection propertyEditorCollection)
        {
            _dataTypeService = dataTypeService ?? throw new ArgumentNullException(nameof(dataTypeService));
            _propertyEditorCollection = propertyEditorCollection ?? throw new ArgumentNullException(nameof(propertyEditorCollection));
        }

        public IEnumerable<ContentApp> Resolve(IMedia source, MediaItemDisplay destination, IEnumerable<ContentApp> destMember, ResolutionContext context)
        {
            var apps = new List<ContentApp>();

            if (source.ContentType.IsContainer || source.ContentType.Alias == Core.Constants.Conventions.MediaTypes.Folder)
            {
                apps.AppendListViewApp(_dataTypeService, _propertyEditorCollection, source.ContentType.Alias, "media");
            }
            else
            {
                apps.Add(_contentApp);
            }

            apps.Add(_infoApp);

            return apps;
        }
    }

}
