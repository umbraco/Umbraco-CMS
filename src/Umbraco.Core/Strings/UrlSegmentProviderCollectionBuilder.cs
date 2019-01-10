﻿using Umbraco.Core.Composing;

namespace Umbraco.Core.Strings
{
    public class UrlSegmentProviderCollectionBuilder : OrderedCollectionBuilderBase<UrlSegmentProviderCollectionBuilder, UrlSegmentProviderCollection, IUrlSegmentProvider>
    {
        protected override UrlSegmentProviderCollectionBuilder This => this;
    }
}
