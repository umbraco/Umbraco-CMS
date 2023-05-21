// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Extensions;

public static class TypeLoaderExtensions
{
    /// <summary>
    ///     Gets all types implementing <see cref="IDataEditor" />.
    /// </summary>
    public static IEnumerable<Type> GetDataEditors(this TypeLoader mgr) => mgr.GetTypes<IDataEditor>();

    /// <summary>
    ///     Gets all types implementing ICacheRefresher.
    /// </summary>
    public static IEnumerable<Type> GetCacheRefreshers(this TypeLoader mgr) => mgr.GetTypes<ICacheRefresher>();

    /// <summary>
    ///     Gets all types implementing <see cref="IAction" />
    /// </summary>
    /// <param name="mgr"></param>
    /// <returns></returns>
    public static IEnumerable<Type> GetActions(this TypeLoader mgr) => mgr.GetTypes<IAction>();
}
