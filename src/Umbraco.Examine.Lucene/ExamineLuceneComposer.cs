// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine
{
    // We want to run after core composers since we are replacing some items
    public sealed class ExamineLuceneComposer :IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.AddExamineLucene();
        }
    }
}
