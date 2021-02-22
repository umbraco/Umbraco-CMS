// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for models builder settings.
    /// </summary>
    public class ModelsBuilderSettings
    {
        private bool _flagOutOfDateModels;

        private static string DefaultModelsDirectory => "~/umbraco/models";

        /// <summary>
        /// Gets or sets a value for the models mode.
        /// </summary>
        public ModelsMode ModelsMode { get; set; } = ModelsMode.PureLive;

        /// <summary>
        /// Gets or sets a value for models namespace.
        /// </summary>
        /// <remarks>That value could be overriden by other (attribute in user's code...). Return default if no value was supplied.</remarks>
        public string ModelsNamespace { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we should flag out-of-date models.
        /// </summary>
        /// <remarks>
        /// Models become out-of-date when data types or content types are updated. When this
        /// setting is activated the ~/App_Data/Models/ood.txt file is then created. When models are
        /// generated through the dashboard, the files is cleared. Default value is <c>false</c>.
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
        /// Gets or sets a value for the models directory.
        /// </summary>
        /// <remarks>Default is ~/App_Data/Models but that can be changed.</remarks>
        public string ModelsDirectory { get; set; } = DefaultModelsDirectory;

        /// <summary>
        /// Gets or sets a value indicating whether to accept an unsafe value for ModelsDirectory.
        /// </summary>
        /// <remarks>
        /// An unsafe value is an absolute path, or a relative path pointing outside
        /// of the website root.
        /// </remarks>
        public bool AcceptUnsafeModelsDirectory { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating the debug log level.
        /// </summary>
        /// <remarks>0 means minimal (safe on live site), anything else means more and more details (maybe not safe).</remarks>
        public int DebugLevel { get; set; } = 0;
    }
}
