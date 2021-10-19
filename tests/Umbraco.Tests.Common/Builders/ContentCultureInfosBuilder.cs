using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders
{
    public class ContentCultureInfosBuilder : ChildBuilderBase<ContentCultureInfosCollectionBuilder, ContentCultureInfos>,
        IWithNameBuilder,
        IWithDateBuilder
    {
        private string _name;
        private string _cultureIso;
        private DateTime? _date;
        public ContentCultureInfosBuilder(ContentCultureInfosCollectionBuilder parentBuilder) : base(parentBuilder)
        {
        }

        public ContentCultureInfosBuilder WithCultureIso(string cultureIso)
        {
            _cultureIso = cultureIso;
            return this;
        }

        public override ContentCultureInfos Build()
        {
            var name = _name ?? Guid.NewGuid().ToString();
            var cultureIso = _cultureIso ?? "en-us";
            DateTime date = _date ?? DateTime.Now;

            return new ContentCultureInfos(cultureIso) { Name = name, Date = date };
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public DateTime? Date
        {
            get => _date;
            set => _date = value;
        }
    }
}
