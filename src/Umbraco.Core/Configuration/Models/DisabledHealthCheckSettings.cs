using System;

namespace Umbraco.Core.Configuration.Models
{
    public class DisabledHealthCheckSettings
    {
        public Guid Id { get; set; }

        public DateTime DisabledOn { get; set; }

        public int DisabledBy { get; set; }
    }
}
