using System.Collections.Generic;

namespace Umbraco.Web.HealthCheck
{
    public interface IHealthCheckResolver
    {
        IEnumerable<HealthCheck> GetHealthChecks(HealthCheckContext context);
    }
}