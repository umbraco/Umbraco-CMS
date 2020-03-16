using System;
using System.Configuration;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Umbraco.Core.Configuration;
using Umbraco.Core;
using Umbraco.Core.IO;
using ConfigurationSection = System.Configuration.ConfigurationSection;

namespace Umbraco.Configuration.Models
{
    /// <summary>
    /// Represents the models builder configuration.
    /// </summary>
    internal class ModelsBuilderConfig : IModelsBuilderConfig
    {
        private readonly IConfiguration _configuration;
        public string DefaultModelsDirectory => "~/App_Data/Models";

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelsBuilderConfig"/> class.
        /// </summary>
        public ModelsBuilderConfig(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private static string GetModelsDirectory(string root, string config, bool acceptUnsafe)
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
        public bool Enable => _configuration.GetValue("Umbraco:CMS:ModelsBuilder:Enable", false);

        /// <summary>
        /// Gets the models mode.
        /// </summary>
        public ModelsMode ModelsMode => _configuration.GetValue("Umbraco:CMS:ModelsBuilder:ModelsMode", ModelsMode.Nothing);

        /// <summary>
        /// Gets the models namespace.
        /// </summary>
        /// <remarks>That value could be overriden by other (attribute in user's code...). Return default if no value was supplied.</remarks>
        public string ModelsNamespace => _configuration.GetValue<string>("Umbraco:CMS:ModelsBuilder:ModelsNamespace");

        /// <summary>
        /// Gets a value indicating whether we should enable the models factory.
        /// </summary>
        /// <remarks>Default value is <c>true</c> because no factory is enabled by default in Umbraco.</remarks>
        public bool EnableFactory => _configuration.GetValue("Umbraco:CMS:ModelsBuilder:EnableFactory", true);

        /// <summary>
        /// Gets a value indicating whether we should flag out-of-date models.
        /// </summary>
        /// <remarks>Models become out-of-date when data types or content types are updated. When this
        /// setting is activated the ~/App_Data/Models/ood.txt file is then created. When models are
        /// generated through the dashboard, the files is cleared. Default value is <c>false</c>.</remarks>
        public bool FlagOutOfDateModels => _configuration.GetValue("Umbraco:CMS:ModelsBuilder:FlagOutOfDateModels", false) && !ModelsMode.IsLive();

        /// <summary>
        /// Gets the models directory.
        /// </summary>
        /// <remarks>Default is ~/App_Data/Models but that can be changed.</remarks>
        public string ModelsDirectory => GetModelsDirectory("~/",_configuration.GetValue("Umbraco:CMS:ModelsBuilder:ModelsDirectory", "~/App_Data/Models"), AcceptUnsafeModelsDirectory);

        /// <summary>
        /// Gets a value indicating whether to accept an unsafe value for ModelsDirectory.
        /// </summary>
        /// <remarks>An unsafe value is an absolute path, or a relative path pointing outside
        /// of the website root.</remarks>
        public bool AcceptUnsafeModelsDirectory => _configuration.GetValue("Umbraco:CMS:ModelsBuilder:AcceptUnsafeModelsDirectory", false);

        /// <summary>
        /// Gets a value indicating the debug log level.
        /// </summary>
        /// <remarks>0 means minimal (safe on live site), anything else means more and more details (maybe not safe).</remarks>
        public int DebugLevel => _configuration.GetValue("Umbraco:CMS:ModelsBuilder:DebugLevel", 0);
    }
}
