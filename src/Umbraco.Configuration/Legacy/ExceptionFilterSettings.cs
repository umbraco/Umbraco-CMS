using System.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Legacy
{
    public class ExceptionFilterSettings : IExceptionFilterSettings
    {
        public ExceptionFilterSettings()
        {
            if (bool.TryParse(ConfigurationManager.AppSettings["Umbraco.Web.DisableModelBindingExceptionFilter"],
                out var disabled))
            {
                Disabled = disabled;
            }
        }
        public bool Disabled { get; }
    }
}
