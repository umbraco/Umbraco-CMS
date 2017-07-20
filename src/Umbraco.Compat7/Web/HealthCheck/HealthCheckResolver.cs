using System.Collections.Generic;
using Umbraco.Core.ObjectResolution;
using CoreCurrent = Umbraco.Core.Composing.Current;
using LightInject;

// ReSharper disable once CheckNamespace
namespace Umbraco.Web.HealthCheck
{
    public class HealthCheckResolver : LazyManyObjectsResolverBase<HealthCheckCollectionBuilder, HealthCheckCollection, HealthCheck>
    {
        private HealthCheckResolver(HealthCheckCollectionBuilder builder)
            : base(builder)
        { }

        public static HealthCheckResolver Current { get; }
            = new HealthCheckResolver(CoreCurrent.Container.GetInstance<HealthCheckCollectionBuilder>());

        public IEnumerable<HealthCheck> HealthChecks => Builder.CreateCollection(); // transient
    }
}
