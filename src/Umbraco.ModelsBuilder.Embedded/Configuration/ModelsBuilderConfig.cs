using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Web.Configuration;
using Umbraco.Core;
using Umbraco.Core.IO;

namespace Umbraco.ModelsBuilder.Embedded.Configuration
{
    /// <summary>
    /// Represents the models builder configuration.
    /// </summary>
    public class ModelsBuilderConfig : IModelsBuilderConfig
    {
        private const string Prefix = "Umbraco.ModelsBuilder.";
        private object _modelsModelLock;
        private bool _modelsModelConfigured;
        private ModelsMode _modelsMode;
        private object _flagOutOfDateModelsLock;
        private bool _flagOutOfDateModelsConfigured;
        private bool _flagOutOfDateModels;
        public const string DefaultModelsNamespace = "Umbraco.Web.PublishedModels";
        public const string DefaultModelsDirectory = "~/App_Data/Models";

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelsBuilderConfig"/> class.
        /// </summary>
        public ModelsBuilderConfig()
        {
            // giant kill switch, default: false
            // must be explicitely set to true for anything else to happen
            Enable = ConfigurationManager.AppSettings[Prefix + "Enable"] == "true";

            // ensure defaults are initialized for tests
            ModelsNamespace = DefaultModelsNamespace;
            ModelsDirectory = IOHelper.MapPath(DefaultModelsDirectory);
            DebugLevel = 0;

            // stop here, everything is false
            if (!Enable) return;

            // default: false
            AcceptUnsafeModelsDirectory = ConfigurationManager.AppSettings[Prefix + "AcceptUnsafeModelsDirectory"].InvariantEquals("true");

            // default: true
            EnableFactory = !ConfigurationManager.AppSettings[Prefix + "EnableFactory"].InvariantEquals("false");

            // default: initialized above with DefaultModelsNamespace const
            var value = ConfigurationManager.AppSettings[Prefix + "ModelsNamespace"];
            if (!string.IsNullOrWhiteSpace(value))
                ModelsNamespace = value;

            // default: initialized above with DefaultModelsDirectory const
            value = ConfigurationManager.AppSettings[Prefix + "ModelsDirectory"];
            if (!string.IsNullOrWhiteSpace(value))
            {
                var root = IOHelper.MapPath("~/");
                if (root == null)
                    throw new ConfigurationErrorsException("Could not determine root directory.");

                // GetModelsDirectory will ensure that the path is safe
                ModelsDirectory = GetModelsDirectory(root, value, AcceptUnsafeModelsDirectory);
            }

            // default: 0
            value = ConfigurationManager.AppSettings[Prefix + "DebugLevel"];
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (!int.TryParse(value, out var debugLevel))
                    throw new ConfigurationErrorsException($"Invalid debug level \"{value}\".");
                DebugLevel = debugLevel;
            }

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelsBuilderConfig"/> class.
        /// </summary>
        public ModelsBuilderConfig(
            bool enable = false,
            ModelsMode modelsMode = ModelsMode.Nothing,
            string modelsNamespace = null,
            bool enableFactory = true,
            bool flagOutOfDateModels = true,
            string modelsDirectory = null,
            bool acceptUnsafeModelsDirectory = false,
            int debugLevel = 0)
        {
            Enable = enable;
            _modelsMode = modelsMode;

            ModelsNamespace = string.IsNullOrWhiteSpace(modelsNamespace) ? DefaultModelsNamespace : modelsNamespace;
            EnableFactory = enableFactory;
            _flagOutOfDateModels = flagOutOfDateModels;
            ModelsDirectory = string.IsNullOrWhiteSpace(modelsDirectory) ? DefaultModelsDirectory : modelsDirectory;
            AcceptUnsafeModelsDirectory = acceptUnsafeModelsDirectory;
            DebugLevel = debugLevel;
        }

        // internal for tests
        internal static string GetModelsDirectory(string root, string config, bool acceptUnsafe)
        {
            // making sure it is safe, ie under the website root,
            // unless AcceptUnsafeModelsDirectory and then everything is OK.

            if (!Path.IsPathRooted(root))
                throw new ConfigurationErrorsException($"Root is not rooted \"{root}\".");

            if (config.StartsWith("~/"))
            {
                var dir = Path.Combine(root, config.TrimStart("~/"));

                // sanitize - GetFullPath will take care of any relative
                // segments in path, eg '../../foo.tmp' - it may throw a SecurityException
                // if the combined path reaches illegal parts of the filesystem
                dir = Path.GetFullPath(dir);
                root = Path.GetFullPath(root);

                if (!dir.StartsWith(root) && !acceptUnsafe)
                    throw new ConfigurationErrorsException($"Invalid models directory \"{config}\".");

                return dir;
            }

            if (acceptUnsafe)
                return Path.GetFullPath(config);

            throw new ConfigurationErrorsException($"Invalid models directory \"{config}\".");
        }

        /// <summary>
        /// Gets a value indicating whether the whole models experience is enabled.
        /// </summary>
        /// <remarks>
        ///     <para>If this is false then absolutely nothing happens.</para>
        ///     <para>Default value is <c>false</c> which means that unless we have this setting, nothing happens.</para>
        /// </remarks>
        public bool Enable { get; }

        /// <summary>
        /// Gets the models mode.
        /// </summary>
        public ModelsMode ModelsMode =>
            LazyInitializer.EnsureInitialized(ref _modelsMode, ref _modelsModelConfigured, ref _modelsModelLock, () =>
            {
                // mode
                var modelsMode = ConfigurationManager.AppSettings[Prefix + "ModelsMode"];
                if (string.IsNullOrWhiteSpace(modelsMode)) return ModelsMode.Nothing; //default
                switch (modelsMode)
                {
                    case nameof(ModelsMode.Nothing):
                        return ModelsMode.Nothing;
                    case nameof(ModelsMode.PureLive):
                        return ModelsMode.PureLive;
                    case nameof(ModelsMode.AppData):
                        return ModelsMode.AppData;
                    case nameof(ModelsMode.LiveAppData):
                        return ModelsMode.LiveAppData;
                    default:
                        throw new ConfigurationErrorsException($"ModelsMode \"{modelsMode}\" is not a valid mode." + " Note that modes are case-sensitive. Possible values are: " + string.Join(", ", Enum.GetNames(typeof(ModelsMode))));
                }
            });

        /// <summary>
        /// Gets a value indicating whether system.web/compilation/@debug is true.
        /// </summary>
        public bool IsDebug
        {
            get
            {
                var section = (CompilationSection)ConfigurationManager.GetSection("system.web/compilation");
                return section != null && section.Debug;
            }
        }

        /// <summary>
        /// Gets the models namespace.
        /// </summary>
        /// <remarks>That value could be overriden by other (attribute in user's code...). Return default if no value was supplied.</remarks>
        public string ModelsNamespace { get; }

        /// <summary>
        /// Gets a value indicating whether we should enable the models factory.
        /// </summary>
        /// <remarks>Default value is <c>true</c> because no factory is enabled by default in Umbraco.</remarks>
        public bool EnableFactory { get; }

        /// <summary>
        /// Gets a value indicating whether we should flag out-of-date models.
        /// </summary>
        /// <remarks>Models become out-of-date when data types or content types are updated. When this
        /// setting is activated the ~/App_Data/Models/ood.txt file is then created. When models are
        /// generated through the dashboard, the files is cleared. Default value is <c>false</c>.</remarks>
        public bool FlagOutOfDateModels
            => LazyInitializer.EnsureInitialized(ref _flagOutOfDateModels, ref _flagOutOfDateModelsConfigured, ref _flagOutOfDateModelsLock, () =>
            {
                var flagOutOfDateModels = !ConfigurationManager.AppSettings[Prefix + "FlagOutOfDateModels"].InvariantEquals("false");
                if (ModelsMode == ModelsMode.Nothing || ModelsMode.IsLive())
                {
                    flagOutOfDateModels = false;
                }

                return flagOutOfDateModels;
            });

        /// <summary>
        /// Gets the models directory.
        /// </summary>
        /// <remarks>Default is ~/App_Data/Models but that can be changed.</remarks>
        public string ModelsDirectory { get; }

        /// <summary>
        /// Gets a value indicating whether to accept an unsafe value for ModelsDirectory.
        /// </summary>
        /// <remarks>An unsafe value is an absolute path, or a relative path pointing outside
        /// of the website root.</remarks>
        public bool AcceptUnsafeModelsDirectory { get; }

        /// <summary>
        /// Gets a value indicating the debug log level.
        /// </summary>
        /// <remarks>0 means minimal (safe on live site), anything else means more and more details (maybe not safe).</remarks>
        public int DebugLevel { get; }
    }
}
