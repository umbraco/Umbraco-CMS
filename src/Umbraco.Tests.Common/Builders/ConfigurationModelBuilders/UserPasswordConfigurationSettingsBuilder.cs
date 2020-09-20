using Umbraco.Core.Configuration.Models;

namespace Umbraco.Tests.Common.Builders
{
    public class UserPasswordConfigurationSettingsBuilder : BuilderBase<UserPasswordConfigurationSettings>
    {
        public override UserPasswordConfigurationSettings Build()
        {
            return new UserPasswordConfigurationSettings();
        }
    }
}
