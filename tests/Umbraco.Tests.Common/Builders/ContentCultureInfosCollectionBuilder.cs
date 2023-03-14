using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentCultureInfosCollectionBuilder : ChildBuilderBase<ContentBuilder, ContentCultureInfosCollection>,
    IBuildContentCultureInfosCollection
{
    private readonly List<ContentCultureInfosBuilder> _cultureInfosBuilders;

    public ContentCultureInfosCollectionBuilder(ContentBuilder parentBuilder) : base(parentBuilder) =>
        _cultureInfosBuilders = new List<ContentCultureInfosBuilder>();

    public ContentCultureInfosBuilder AddCultureInfos()
    {
        var builder = new ContentCultureInfosBuilder(this);
        _cultureInfosBuilders.Add(builder);
        return builder;
    }

    public override ContentCultureInfosCollection Build()
    {
        if (_cultureInfosBuilders.Count < 1)
        {
            throw new InvalidOperationException("You must add at least one culture infos to the collection builder");
        }

        var cultureInfosCollection = new ContentCultureInfosCollection();

        foreach (var cultureInfosBuilder in _cultureInfosBuilders)
        {
            cultureInfosCollection.Add(cultureInfosBuilder.Build());
        }

        return cultureInfosCollection;
    }
}
