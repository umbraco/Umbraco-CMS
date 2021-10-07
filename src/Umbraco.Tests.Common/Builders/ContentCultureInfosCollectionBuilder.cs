using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders
{
    public class ContentCultureInfosCollectionBuilder : ChildBuilderBase<ContentBuilder, ContentCultureInfosCollection>, IBuildContentCultureInfosCollection
    {
        private List<ContentCultureInfos> _cultureInfos;
        public ContentCultureInfosCollectionBuilder(ContentBuilder parentBuilder) : base(parentBuilder) => _cultureInfos = new List<ContentCultureInfos>();

        // TODO: Should this again wrap *another* child builder "ContentCultureInfosBuilder"?
        public ContentCultureInfosCollectionBuilder WithCultureInfo(string culture, string name, DateTime? date = null)
        {
            if (date is null)
            {
                date = DateTime.Now;
            }

            _cultureInfos.Add(new ContentCultureInfos(culture) { Name = name, Date = date.Value });
            return this;
        }

        public override ContentCultureInfosCollection Build()
        {
            var cultureInfos = new ContentCultureInfosCollection();

            foreach (ContentCultureInfos cultureInfo in _cultureInfos)
            {
                cultureInfos.AddOrUpdate(cultureInfo.Culture, cultureInfo.Name, cultureInfo.Date);
            }

            return cultureInfos;
        }
    }
}
