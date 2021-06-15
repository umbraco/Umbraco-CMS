using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions
{
    public static class RuntimeStateExtensions
    {
        /// <summary>
        /// Returns true if Umbraco <see cref="IRuntimeState"/> is greater than <see cref="RuntimeLevel.BootFailed"/>
        /// </summary>
        public static bool UmbracoCanBoot(this IRuntimeState state) => state.Level > RuntimeLevel.BootFailed;
    }
}
