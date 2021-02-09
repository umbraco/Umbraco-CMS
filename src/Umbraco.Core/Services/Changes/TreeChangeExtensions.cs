// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Extensions
{
    public static class TreeChangeExtensions
    {
        public static TreeChange<TItem>.EventArgs ToEventArgs<TItem>(this IEnumerable<TreeChange<TItem>> changes)
        {
            return new TreeChange<TItem>.EventArgs(changes);
        }

        public static bool HasType(this TreeChangeTypes change, TreeChangeTypes type)
        {
            return (change & type) != TreeChangeTypes.None;
        }

        public static bool HasTypesAll(this TreeChangeTypes change, TreeChangeTypes types)
        {
            return (change & types) == types;
        }

        public static bool HasTypesAny(this TreeChangeTypes change, TreeChangeTypes types)
        {
            return (change & types) != TreeChangeTypes.None;
        }

        public static bool HasTypesNone(this TreeChangeTypes change, TreeChangeTypes types)
        {
            return (change & types) == TreeChangeTypes.None;
        }
    }
}
