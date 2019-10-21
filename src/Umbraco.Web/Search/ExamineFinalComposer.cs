﻿using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Search
{
    // examine's final composer composes after all user composers
    // and *also* after ICoreComposer (in case IUserComposer is disabled)
    [ComposeAfter(typeof(IUserComposer))]
    [ComposeAfter(typeof(ICoreComposer))]
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class ExamineFinalComposer : ComponentComposer<ExamineFinalComponent>
    { }
}
