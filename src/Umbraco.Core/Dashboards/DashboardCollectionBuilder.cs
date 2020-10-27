using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;
using Umbraco.Core.Manifest;

namespace Umbraco.Web.Dashboards
{
    public class DashboardCollectionBuilder : WeightedCollectionBuilderBase<DashboardCollectionBuilder, DashboardCollection, IDashboard>
    {
        private Dictionary<Type, int> _customWeights = new Dictionary<Type, int>();

        protected override DashboardCollectionBuilder This => this;

        /// <summary>
        /// Changes the default weight of a dashboard
        /// </summary>
        /// <typeparam name="T">The type of dashboard</typeparam>
        /// <param name="weight">The new dashboard weight</param>
        /// <returns></returns>
        public DashboardCollectionBuilder SetWeight<T>(int weight) where T : IDashboard
        {
            _customWeights[typeof(T)] = weight;
            return this;
        }

        protected override IEnumerable<IDashboard> CreateItems(IFactory factory)
        {
            // get the manifest parser just-in-time - injecting it in the ctor would mean that
            // simply getting the builder in order to configure the collection, would require
            // its dependencies too, and that can create cycles or other oddities
            var manifestParser = factory.GetRequiredService<IManifestParser>();

            var dashboardSections = Merge(base.CreateItems(factory), manifestParser.Manifest.Dashboards);

            return dashboardSections;
        }

        private IEnumerable<IDashboard> Merge(IEnumerable<IDashboard> dashboardsFromCode, IReadOnlyList<ManifestDashboard> dashboardFromManifest)
        {
            return dashboardsFromCode.Concat(dashboardFromManifest)
                .Where(x => !string.IsNullOrEmpty(x.Alias))
                .OrderBy(GetWeight);
        }

        private int GetWeight(IDashboard dashboard)
        {
            var typeOfDashboard = dashboard.GetType();
            if(_customWeights.ContainsKey(typeOfDashboard))
            {
                return _customWeights[typeOfDashboard];
            }

            switch (dashboard)
            {
                case ManifestDashboard manifestDashboardDefinition:
                    return manifestDashboardDefinition.Weight;

                default:
                    var weightAttribute = dashboard.GetType().GetCustomAttribute<WeightAttribute>(false);
                    return weightAttribute?.Weight ?? DefaultWeight;
            }
        }
    }
}
