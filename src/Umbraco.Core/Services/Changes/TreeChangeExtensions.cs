// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Extensions;

public static class TreeChangeExtensions
{
    public static TreeChange<TItem>.EventArgs ToEventArgs<TItem>(this IEnumerable<TreeChange<TItem>> changes) =>
        new TreeChange<TItem>.EventArgs(changes);

    public static bool HasType(this TreeChangeTypes change, TreeChangeTypes type) =>
        (change & type) != TreeChangeTypes.None;

    public static bool HasTypesAll(this TreeChangeTypes change, TreeChangeTypes types) => (change & types) == types;

    public static bool HasTypesAny(this TreeChangeTypes change, TreeChangeTypes types) =>
        (change & types) != TreeChangeTypes.None;

    public static bool HasTypesNone(this TreeChangeTypes change, TreeChangeTypes types) =>
        (change & types) == TreeChangeTypes.None;
}
