using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public class DisabledHealthCheckElement : ConfigurationElement, IDisabledHealthCheckElement
    {
        private const string ID_KEY = "id";
        private const string DISABLED_ON_KEY = "disabledOn";
        private const string DISABLED_BY_KEY = "disabledBy";

        [ConfigurationProperty(ID_KEY, IsKey = true, IsRequired = true)]
        public Guid Id
        {
            get
            {
                return ((Guid)(base[ID_KEY]));
            }
        }

        [ConfigurationProperty(DISABLED_ON_KEY, IsKey = false, IsRequired = false)]
        public DateTime DisabledOn
        {
            get
            {
                return ((DateTime)(base[DISABLED_ON_KEY]));
            }
        }

        [ConfigurationProperty(DISABLED_BY_KEY, IsKey = false, IsRequired = false)]
        public int DisabledBy
        {
            get
            {
                return ((int)(base[DISABLED_BY_KEY]));
            }
        }
    }
}
