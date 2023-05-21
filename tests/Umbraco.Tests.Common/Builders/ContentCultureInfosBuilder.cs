using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentCultureInfosBuilder : ChildBuilderBase<ContentCultureInfosCollectionBuilder, ContentCultureInfos>,
    IWithNameBuilder,
    IWithDateBuilder
{
    private string _cultureIso;

    public ContentCultureInfosBuilder(ContentCultureInfosCollectionBuilder parentBuilder) : base(parentBuilder)
    {
    }

    public DateTime? Date { get; set; }

    public string Name { get; set; }

    public ContentCultureInfosBuilder WithCultureIso(string cultureIso)
    {
        _cultureIso = cultureIso;
        return this;
    }

    public override ContentCultureInfos Build()
    {
        var name = Name ?? Guid.NewGuid().ToString();
        var cultureIso = _cultureIso ?? "en-us";
        var date = Date ?? DateTime.Now;

        return new ContentCultureInfos(cultureIso) { Name = name, Date = date };
    }
}
