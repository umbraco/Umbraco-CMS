using Umbraco.Configuration;

namespace Umbraco.Core.Configuration.Models
{
    /// <summary>
    ///     Represents the models builder configuration.
    /// </summary>
    public class ModelsBuilderSettings 
    {
        public static string DefaultModelsDirectory => "~/App_Data/Models";

        /// <summary>
        ///     Gets a value indicating whether the whole models experience is enabled.
        /// </summary>
        /// <remarks>
        ///     <para>If this is false then absolutely nothing happens.</para>
        ///     <para>Default value is <c>false</c> which means that unless we have this setting, nothing happens.</para>
        /// </remarks>
        public bool Enable { get; set; } = false;

        /// <summary>
        ///     Gets the models mode.
        /// </summary>
        public ModelsMode ModelsMode { get; set; } = ModelsMode.Nothing;

        /// <summary>
        ///     Gets the models namespace.
        /// </summary>
        /// <remarks>That value could be overriden by other (attribute in user's code...). Return default if no value was supplied.</remarks>
        public string ModelsNamespace { get; set; }

        /// <summary>
        ///     Gets a value indicating whether we should enable the models factory.
        /// </summary>
        /// <remarks>Default value is <c>true</c> because no factory is enabled by default in Umbraco.</remarks>
        public bool EnableFactory { get; set; } = true;

        private bool _flagOutOfDateModels;

        /// <summary>
        ///     Gets a value indicating whether we should flag out-of-date models.
        /// </summary>
        /// <remarks>
        ///     Models become out-of-date when data types or content types are updated. When this
        ///     setting is activated the ~/App_Data/Models/ood.txt file is then created. When models are
        ///     generated through the dashboard, the files is cleared. Default value is <c>false</c>.
        /// </remarks>
        public bool FlagOutOfDateModels
        {
            get => _flagOutOfDateModels;

            set
            {
                if (!ModelsMode.IsLive())
                {
                    _flagOutOfDateModels = false;
                }

                _flagOutOfDateModels = value;
            }
        }

        /// <summary>
        ///     Gets the models directory.
        /// </summary>
        /// <remarks>Default is ~/App_Data/Models but that can be changed.</remarks>
        public string ModelsDirectory { get; set; } = DefaultModelsDirectory;

        /// <summary>
        ///     Gets a value indicating whether to accept an unsafe value for ModelsDirectory.
        /// </summary>
        /// <remarks>
        ///     An unsafe value is an absolute path, or a relative path pointing outside
        ///     of the website root.
        /// </remarks>
        public bool AcceptUnsafeModelsDirectory { get; set; } = false;

        /// <summary>
        ///     Gets a value indicating the debug log level.
        /// </summary>
        /// <remarks>0 means minimal (safe on live site), anything else means more and more details (maybe not safe).</remarks>
        public int DebugLevel { get; set; } = 0;
    }
}
