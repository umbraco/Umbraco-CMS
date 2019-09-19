using System;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public interface IDisabledHealthCheck
    {
        Guid Id { get; }
        DateTime DisabledOn { get; }
        int DisabledBy { get; }
    }
}
