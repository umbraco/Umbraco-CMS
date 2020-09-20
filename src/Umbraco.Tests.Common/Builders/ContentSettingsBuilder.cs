using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Macros;

namespace Umbraco.Tests.Common.Builders
{
    public class ContentSettingsBuilder : ContentSettingsBuilder<object>
    {
        public ContentSettingsBuilder() : base(null)
        {
        }
    }

    public class ContentSettingsBuilder<TParent> : ChildBuilderBase<TParent, ContentSettings>
    {
        private string _macroErrors;
        private readonly ContentImagingSettingsBuilder<ContentSettingsBuilder<TParent>> _contentImagingSettingsBuilder;
        private readonly List<ContentErrorPageBuilder<ContentSettingsBuilder<TParent>>> _contentErrorPageBuilders = new List<ContentErrorPageBuilder<ContentSettingsBuilder<TParent>>>();

        public ContentSettingsBuilder(TParent parentBuilder) : base(parentBuilder)
        {
            _contentImagingSettingsBuilder = new ContentImagingSettingsBuilder<ContentSettingsBuilder<TParent>>(this);
            _contentErrorPageBuilders = new List<ContentErrorPageBuilder<ContentSettingsBuilder<TParent>>>();
        }

        public ContentImagingSettingsBuilder<ContentSettingsBuilder<TParent>> AddImaging() => _contentImagingSettingsBuilder;

        public ContentErrorPageBuilder<ContentSettingsBuilder<TParent>> AddErrorPage()
        {
            var builder = new ContentErrorPageBuilder<ContentSettingsBuilder<TParent>>(this);
            _contentErrorPageBuilders.Add(builder);
            return builder;
        }

        public ContentSettingsBuilder<TParent> WithMacroErrors(string macroErrors)
        {
            _macroErrors = macroErrors;
            return this;
        }

        public override ContentSettings Build()
        {
            var macroErrors = _macroErrors ?? MacroErrorBehaviour.Inline.ToString();
            var contentImagingSettings = _contentImagingSettingsBuilder.Build();
            var contentErrorPages = _contentErrorPageBuilders.Select(x => x.Build()).ToArray();

            return new ContentSettings
            {
                MacroErrors = _macroErrors,
                Imaging = contentImagingSettings,
                Error404Collection = contentErrorPages,
            };
        }
    }
}
