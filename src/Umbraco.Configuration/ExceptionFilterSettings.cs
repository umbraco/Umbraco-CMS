using System.Configuration;
using Umbraco.Abstractions;

namespace Umbraco.Configuration
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
