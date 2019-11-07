using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public class DisabledHealthCheckElement : ConfigurationElement, IDisabledHealthCheck
    {
        private const string IdKey = "id";
        private const string DisabledOnKey = "disabledOn";
        private const string DisabledByKey = "disabledBy";

        [ConfigurationProperty(IdKey, IsKey = true, IsRequired = true)]
        public Guid Id
        {
            get
            {
                return ((Guid)(base[IdKey]));
            }
        }

        [ConfigurationProperty(DisabledOnKey, IsKey = false, IsRequired = false)]
        public DateTime DisabledOn
        {
            get
            {
                return ((DateTime)(base[DisabledOnKey]));
            }
        }

        [ConfigurationProperty(DisabledByKey, IsKey = false, IsRequired = false)]
        public int DisabledBy
        {
            get
            {
                return ((int)(base[DisabledByKey]));
            }
        }
    }
}
