using System;

namespace Umbraco.Core.HealthCheck.Checks
{
    public class DisabledHealthCheck
    {
        public Guid Id { get; set; }

        public DateTime DisabledOn { get; set; }

        public int DisabledBy { get; set; }
    }
}
