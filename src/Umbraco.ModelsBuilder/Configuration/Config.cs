using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Hosting;
using Microsoft.CodeAnalysis.CSharp;
using Umbraco.Core;

namespace Umbraco.ModelsBuilder.Configuration
{
    /// <summary>
    /// Represents the models builder configuration.
    /// </summary>
    public class Config
    {
        private static Config _value;

        /// <summary>
        /// Gets the configuration - internal so that the UmbracoConfig extension
        /// can get the value to initialize its own value. Either a value has
        /// been provided via the Setup method, or a new instance is created, which
        /// will load settings from the config file.
        /// </summary>
        internal static Config Value => _value ?? new Config();

        /// <summary>
        /// Sets the configuration programmatically.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <remarks>
        /// <para>Once the configuration has been accessed via the UmbracoConfig extension,
        /// it cannot be changed anymore, and using this method will achieve nothing.</para>
        /// <para>For tests, see UmbracoConfigExtensions.ResetConfig().</para>
        /// </remarks>
        public static void Setup(Config config)
        {
            _value = config;
        }

        internal const string DefaultStaticMixinGetterPattern = "Get{0}";
        internal const LanguageVersion DefaultLanguageVersion = LanguageVersion.CSharp6;
        internal const string DefaultModelsNamespace = "Umbraco.Web.PublishedModels";
        internal const ClrNameSource DefaultClrNameSource = ClrNameSource.Alias; // for legacy reasons
        internal const string DefaultModelsDirectory = "~/App_Data/Models";

        /// <summary>
        /// Initializes a new instance of the <see cref="Config"/> class.
        /// </summary>
        private Config()
        {
            const string prefix = "Umbraco.ModelsBuilder.";

            // giant kill switch, default: false
            // must be explicitely set to true for anything else to happen
            Enable = ConfigurationManager.AppSettings[prefix + "Enable"] == "true";

            // ensure defaults are initialized for tests
            StaticMixinGetterPattern = DefaultStaticMixinGetterPattern;
            LanguageVersion = DefaultLanguageVersion;
            ModelsNamespace = DefaultModelsNamespace;
            ClrNameSource = DefaultClrNameSource;
            ModelsDirectory = HostingEnvironment.IsHosted
                ? HostingEnvironment.MapPath(DefaultModelsDirectory)
                : DefaultModelsDirectory.TrimStart("~/");
            DebugLevel = 0;

            // stop here, everything is false
            if (!Enable) return;

            // mode
            var modelsMode = ConfigurationManager.AppSettings[prefix + "ModelsMode"];
            if (!string.IsNullOrWhiteSpace(modelsMode))
            {
                switch (modelsMode)
                {
                    case nameof(ModelsMode.Nothing):
                        ModelsMode = ModelsMode.Nothing;
                        break;
                    case nameof(ModelsMode.PureLive):
                        ModelsMode = ModelsMode.PureLive;
                        break;
                    case nameof(ModelsMode.Dll):
                        ModelsMode = ModelsMode.Dll;
                        break;
                    case nameof(ModelsMode.LiveDll):
                        ModelsMode = ModelsMode.LiveDll;
                        break;
                    case nameof(ModelsMode.AppData):
                        ModelsMode = ModelsMode.AppData;
                        break;
                    case nameof(ModelsMode.LiveAppData):
                        ModelsMode = ModelsMode.LiveAppData;
                        break;
                    default:
                        throw new ConfigurationErrorsException($"ModelsMode \"{modelsMode}\" is not a valid mode."
                            + " Note that modes are case-sensitive.");
                }
            }

            // default: false
            EnableApi = ConfigurationManager.AppSettings[prefix + "EnableApi"].InvariantEquals("true");
            AcceptUnsafeModelsDirectory = ConfigurationManager.AppSettings[prefix + "AcceptUnsafeModelsDirectory"].InvariantEquals("true");

            // default: true
            EnableFactory = !ConfigurationManager.AppSettings[prefix + "EnableFactory"].InvariantEquals("false");
            StaticMixinGetters = !ConfigurationManager.AppSettings[prefix + "StaticMixinGetters"].InvariantEquals("false");
            FlagOutOfDateModels = !ConfigurationManager.AppSettings[prefix + "FlagOutOfDateModels"].InvariantEquals("false");

            // default: initialized above with DefaultModelsNamespace const
            var value = ConfigurationManager.AppSettings[prefix + "ModelsNamespace"];
            if (!string.IsNullOrWhiteSpace(value))
                ModelsNamespace = value;

            // default: initialized above with DefaultStaticMixinGetterPattern const
            value = ConfigurationManager.AppSettings[prefix + "StaticMixinGetterPattern"];
            if (!string.IsNullOrWhiteSpace(value))
                StaticMixinGetterPattern = value;

            // default: initialized above with DefaultLanguageVersion const
            value = ConfigurationManager.AppSettings[prefix + "LanguageVersion"];
            if (!string.IsNullOrWhiteSpace(value))
            {
                LanguageVersion lv;
                if (!Enum.TryParse(value, true, out lv))
                    throw new ConfigurationErrorsException($"Invalid language version \"{value}\".");
                LanguageVersion = lv;
            }

            // default: initialized above with DefaultClrNameSource const
            value = ConfigurationManager.AppSettings[prefix + "ClrNameSource"];
            if (!string.IsNullOrWhiteSpace(value))
            {
                switch (value)
                {
                    case nameof(ClrNameSource.Nothing):
                        ClrNameSource = ClrNameSource.Nothing;
                        break;
                    case nameof(ClrNameSource.Alias):
                        ClrNameSource = ClrNameSource.Alias;
                        break;
                    case nameof(ClrNameSource.RawAlias):
                        ClrNameSource = ClrNameSource.RawAlias;
                        break;
                    case nameof(ClrNameSource.Name):
                        ClrNameSource = ClrNameSource.Name;
                        break;
                    default:
                        throw new ConfigurationErrorsException($"ClrNameSource \"{value}\" is not a valid source."
                            + " Note that sources are case-sensitive.");
                }
            }

            // default: initialized above with DefaultModelsDirectory const
            value = ConfigurationManager.AppSettings[prefix + "ModelsDirectory"];
            if (!string.IsNullOrWhiteSpace(value))
            {
                var root = HostingEnvironment.IsHosted
                    ? HostingEnvironment.MapPath("~/")
                    : Directory.GetCurrentDirectory();
                if (root == null)
                    throw new ConfigurationErrorsException("Could not determine root directory.");

                // GetModelsDirectory will ensure that the path is safe
                ModelsDirectory = GetModelsDirectory(root, value, AcceptUnsafeModelsDirectory);
            }

            // default: 0
            value = ConfigurationManager.AppSettings[prefix + "DebugLevel"];
            if (!string.IsNullOrWhiteSpace(value))
            {
                int debugLevel;
                if (!int.TryParse(value, out debugLevel))
                    throw new ConfigurationErrorsException($"Invalid debug level \"{value}\".");
                DebugLevel = debugLevel;
            }

            // not flagging if not generating, or live (incl. pure)
            if (ModelsMode == ModelsMode.Nothing || ModelsMode.IsLive())
                FlagOutOfDateModels = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Config"/> class.
        /// </summary>
        public Config(
            bool enable = false,
            ModelsMode modelsMode = ModelsMode.Nothing,
            bool enableApi = true,
            string modelsNamespace = null,
            bool enableFactory = true,
            LanguageVersion languageVersion = DefaultLanguageVersion,
            bool staticMixinGetters = true,
            string staticMixinGetterPattern = null,
            bool flagOutOfDateModels = true,
            ClrNameSource clrNameSource = DefaultClrNameSource,
            string modelsDirectory = null,
            bool acceptUnsafeModelsDirectory = false,
            int debugLevel = 0)
        {
            Enable = enable;
            ModelsMode = modelsMode;

            EnableApi = enableApi;
            ModelsNamespace = string.IsNullOrWhiteSpace(modelsNamespace) ? DefaultModelsNamespace : modelsNamespace;
            EnableFactory = enableFactory;
            LanguageVersion = languageVersion;
            StaticMixinGetters = staticMixinGetters;
            StaticMixinGetterPattern = string.IsNullOrWhiteSpace(staticMixinGetterPattern) ? DefaultStaticMixinGetterPattern : staticMixinGetterPattern;
            FlagOutOfDateModels = flagOutOfDateModels;
            ClrNameSource = clrNameSource;
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
        public ModelsMode ModelsMode { get; }

        /// <summary>
        /// Gets a value indicating whether to serve the API.
        /// </summary>
        public bool ApiServer => EnableApi && ApiInstalled && IsDebug;

        /// <summary>
        /// Gets a value indicating whether to enable the API.
        /// </summary>
        /// <remarks>
        ///     <para>Default value is <c>true</c>.</para>
        ///     <para>The API is used by the Visual Studio extension and the console tool to talk to Umbraco
        ///     and retrieve the content types. It needs to be enabled so the extension & tool can work.</para>
        /// </remarks>
        public bool EnableApi { get; }

        /// <summary>
        /// Gets a value indicating whether the API is installed.
        /// </summary>
        // fixme - this is now always true as the API is part of Core
        public bool ApiInstalled => true;

        /// <summary>
        /// Gets a value indicating whether system.web/compilation/@debug is true.
        /// </summary>
        public bool IsDebug
        {
            get
            {
                var section = (CompilationSection) ConfigurationManager.GetSection("system.web/compilation");
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
        /// Gets the Roslyn parser language version.
        /// </summary>
        /// <remarks>Default value is <c>CSharp6</c>.</remarks>
        public LanguageVersion LanguageVersion { get; }

        /// <summary>
        /// Gets a value indicating whether to generate static mixin getters.
        /// </summary>
        /// <remarks>Default value is <c>false</c> for backward compat reaons.</remarks>
        public bool StaticMixinGetters { get; }

        /// <summary>
        /// Gets the string pattern for mixin properties static getter name.
        /// </summary>
        /// <remarks>Default value is "GetXxx". Standard string format.</remarks>
        public string StaticMixinGetterPattern { get; }

        /// <summary>
        /// Gets a value indicating whether we should flag out-of-date models.
        /// </summary>
        /// <remarks>Models become out-of-date when data types or content types are updated. When this
        /// setting is activated the ~/App_Data/Models/ood.txt file is then created. When models are
        /// generated through the dashboard, the files is cleared. Default value is <c>false</c>.</remarks>
        public bool FlagOutOfDateModels { get; }

        /// <summary>
        /// Gets the CLR name source.
        /// </summary>
        public ClrNameSource ClrNameSource { get; }

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
