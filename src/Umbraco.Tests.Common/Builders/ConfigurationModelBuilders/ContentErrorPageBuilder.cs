using System;
using Umbraco.Core.Configuration.Models;

namespace Umbraco.Tests.Common.Builders
{
    public class ContentErrorPageBuilder : ContentErrorPageBuilder<object>
    {
        public ContentErrorPageBuilder() : base(null)
        {
        }
    }

    public class ContentErrorPageBuilder<TParent>
       : ChildBuilderBase<TParent, ContentErrorPage>
    {
        private int? _contentId;
        private Guid? _contentKey;
        private string _contentXPath;
        private string _culture;

        public ContentErrorPageBuilder(TParent parentBuilder) : base(parentBuilder)
        {
        }

        public ContentErrorPageBuilder<TParent> WithContentId(int contentId)
        {
            _contentId = contentId;
            return this;
        }

        public ContentErrorPageBuilder<TParent> WithContentKey(Guid contentKey)
        {
            _contentKey = contentKey;
            return this;
        }

        public ContentErrorPageBuilder<TParent> WithContentXPath(string contentXPath)
        {
            _contentXPath = contentXPath;
            return this;
        }

        public ContentErrorPageBuilder<TParent> WithCulture(string culture)
        {
            _culture = culture;
            return this;
        }

        public override ContentErrorPage Build()
        {
            var contentId = _contentId ?? 0;
            var contentKey = _contentKey ?? Guid.Empty;
            var contentXPath = _contentXPath ?? string.Empty;
            var culture = _culture ?? "en-US";

            return new ContentErrorPage
            {
                ContentId = contentId,
                ContentKey = contentKey,
                ContentXPath = contentXPath,
                Culture = culture,
            };
        }
    }
}
