using Microsoft.Extensions.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    /// <summary>
    ///     Represents the models builder configuration.
    /// </summary>
    internal class ModelsBuilderConfig : IModelsBuilderConfig
    {
        private const string Prefix = Constants.Configuration.ConfigModelsBuilderPrefix;
        private readonly IConfiguration _configuration;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelsBuilderConfig" /> class.
        /// </summary>
        public ModelsBuilderConfig(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string DefaultModelsDirectory => "~/Umbraco/Models";

        /// <summary>
        ///     Gets a value indicating whether the whole models experience is enabled.
        /// </summary>
        /// <remarks>
        ///     <para>If this is false then absolutely nothing happens.</para>
        ///     <para>Default value is <c>false</c> which means that unless we have this setting, nothing happens.</para>
        /// </remarks>
        public bool Enable => _configuration.GetValue(Prefix+"Enable", false);

        /// <summary>
        ///     Gets the models mode.
        /// </summary>
        public ModelsMode ModelsMode =>
            _configuration.GetValue(Prefix+"ModelsMode", ModelsMode.Nothing);

        /// <summary>
        ///     Gets the models namespace.
        /// </summary>
        /// <remarks>That value could be overriden by other (attribute in user's code...). Return default if no value was supplied.</remarks>
        public string ModelsNamespace => _configuration.GetValue<string>(Prefix+"ModelsNamespace");

        /// <summary>
        ///     Gets a value indicating whether we should enable the models factory.
        /// </summary>
        /// <remarks>Default value is <c>true</c> because no factory is enabled by default in Umbraco.</remarks>
        public bool EnableFactory => _configuration.GetValue(Prefix+"EnableFactory", true);

        /// <summary>
        ///     Gets a value indicating whether we should flag out-of-date models.
        /// </summary>
        /// <remarks>
        ///     Models become out-of-date when data types or content types are updated. When this
        ///     setting is activated the ~/App_Data/Models/ood.txt file is then created. When models are
        ///     generated through the dashboard, the files is cleared. Default value is <c>false</c>.
        /// </remarks>
        public bool FlagOutOfDateModels =>
            _configuration.GetValue(Prefix+"FlagOutOfDateModels", false) && !ModelsMode.IsLive();

        /// <summary>
        ///     Gets the models directory.
        /// </summary>
        /// <remarks>Default is ~/App_Data/Models but that can be changed.</remarks>
        public string ModelsDirectory =>
            _configuration.GetValue(Prefix+"ModelsDirectory", "~/Umbraco/Models");

        /// <summary>
        ///     Gets a value indicating whether to accept an unsafe value for ModelsDirectory.
        /// </summary>
        /// <remarks>
        ///     An unsafe value is an absolute path, or a relative path pointing outside
        ///     of the website root.
        /// </remarks>
        public bool AcceptUnsafeModelsDirectory =>
            _configuration.GetValue(Prefix+"AcceptUnsafeModelsDirectory", false);

        /// <summary>
        ///     Gets a value indicating the debug log level.
        /// </summary>
        /// <remarks>0 means minimal (safe on live site), anything else means more and more details (maybe not safe).</remarks>
        public int DebugLevel => _configuration.GetValue(Prefix+"DebugLevel", 0);
    }
}
