using System.Configuration;
using System.Text;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentSecurityPolicySrcElement : UmbracoConfigurationElement, IContentSecurityPolicySrc
    {
        [ConfigurationProperty("unsafeInline", DefaultValue = false)]
        internal bool UnsafeInline => (bool)base["unsafeInline"];

        [ConfigurationProperty("unsafeEval", DefaultValue = false)]
        internal bool UnsafeEval => (bool)base["unsafeEval"];

        [ConfigurationProperty("none", DefaultValue = false)]
        internal bool None => (bool)base["none"];

        [ConfigurationProperty("self", DefaultValue = false)]
        internal bool Self => (bool)base["self"];

        [ConfigurationProperty("hosts", DefaultValue = null)]
        internal string Hosts => (string)base["hosts"];

        bool IContentSecurityPolicySrc.UnsafeInline => UnsafeInline;

        bool IContentSecurityPolicySrc.UnsafeEval => UnsafeEval;

        bool IContentSecurityPolicySrc.None => None;

        bool IContentSecurityPolicySrc.Self => Self;

        string IContentSecurityPolicySrc.Hosts => Hosts;
        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Self)
            {
                sb.Append("'self' ");
            }
            if (None)
            {
                sb.Append("'none' ");
            }
            if (UnsafeEval)
            {
                sb.Append("'unsafe-eval' ");
            }
            if (UnsafeInline)
            {
                sb.Append("'unsafe-inline' ");
            }
            if (!string.IsNullOrEmpty(Hosts))
            {
                sb.Append("hosts ");
            }
            return sb.ToString();
        }
    }
}
