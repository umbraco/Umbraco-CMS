// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Extensions
{
    public static class TypeLoaderExtensions
    {
        /// <summary>
        /// Gets all classes implementing <see cref="IDataEditor"/>.
        /// </summary>
        public static IEnumerable<Type> GetDataEditors(this TypeLoader mgr) => mgr.GetTypes<IDataEditor>();

        /// <summary>
        /// Gets all classes implementing ICacheRefresher.
        /// </summary>
        public static IEnumerable<Type> GetCacheRefreshers(this TypeLoader mgr) => mgr.GetTypes<ICacheRefresher>();

        /// <summary>
        /// Gest all classes implementing <see cref="PackageMigrationPlan"/>
        /// </summary>
        /// <param name="mgr"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetPackageMigrationPlans(this TypeLoader mgr) => mgr.GetTypes<PackageMigrationPlan>();
    }
}
