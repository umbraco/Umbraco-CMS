using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Configuration.Models;

namespace Umbraco.Tests.Common.Builders
{
    public class ContentImagingSettingsBuilder : ContentImagingSettingsBuilder<object>
    {
        public ContentImagingSettingsBuilder() : base(null)
        {
        }
    }

    public class ContentImagingSettingsBuilder<TParent>
       : ChildBuilderBase<TParent, ContentImagingSettings>
    {

        private readonly List<ImagingAutoFillUploadFieldBuilder<ContentImagingSettingsBuilder<TParent>>> _imagingAutoFillUploadFieldBuilder = new List<ImagingAutoFillUploadFieldBuilder<ContentImagingSettingsBuilder<TParent>>>();

        public ContentImagingSettingsBuilder(TParent parentBuilder) : base(parentBuilder)
        {
        }

        public ImagingAutoFillUploadFieldBuilder<ContentImagingSettingsBuilder<TParent>> AddAutoFillImageProperty()
        {
            var builder = new ImagingAutoFillUploadFieldBuilder<ContentImagingSettingsBuilder<TParent>>(this);
            _imagingAutoFillUploadFieldBuilder.Add(builder);
            return builder;
        }

        public override ContentImagingSettings Build()
        {
            var imagingAutoFillUploadFields = _imagingAutoFillUploadFieldBuilder.Select(x => x.Build()).ToArray();

            return new ContentImagingSettings
            {
                AutoFillImageProperties = imagingAutoFillUploadFields,
            };
        }
    }
}
