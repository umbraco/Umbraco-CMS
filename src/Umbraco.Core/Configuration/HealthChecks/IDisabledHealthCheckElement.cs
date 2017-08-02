using System;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public interface IDisabledHealthCheckElement
    {
        Guid Id { get; }
        DateTime DisabledOn { get; }
        int DisabledBy { get; }
    }
}
