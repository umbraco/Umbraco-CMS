﻿using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Core.HealthCheck
{
    [DataContract(Name = "healthCheckGroup", Namespace = "")]
    public class HealthCheckGroup
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "checks")]
        public List<Core.HealthCheck.HealthCheck> Checks { get; set; }
    }
}
