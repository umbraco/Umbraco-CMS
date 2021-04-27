// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine
{
    // examine's Lucene final composer composes after all user composers
    // and *also* after ICoreComposer (in case IUserComposer is disabled)
    [ComposeAfter(typeof(IUserComposer))]
    public class ExamineLuceneFinalComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder) => builder.AddExamineIndexConfiguration();
    }
}
