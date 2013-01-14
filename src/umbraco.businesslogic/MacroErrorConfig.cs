using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using umbraco.IO;

namespace umbraco.businesslogic
{
    /// <summary>
    /// umbraco.BusinessLogic.MacroErrorConfig provides access to the macro error configuration in umbraco.
    /// This configuration defines how handle macro errors:
    /// - Show error within macro as text (default and current Umbraco 'normal' behavior)
    /// - Suppress error and hide macro
    /// - Throw an exception and invoke the global error handler (if one is defined, if not you'll get a YSOD)
    /// These options can be set on a per macro basis based on the macro name using regexes.
    /// </summary>
    public class MacroErrorConfig
    {
        private const string CacheKey = "MacroErrorCache";
        internal const string MacroErrorConfigFileName = "macroerrors.config";
        private static string _macroErrorConfig;
        private static readonly object Locker = new object();

        /// <summary>
        /// gets/sets the macroerrors.config file path
        /// </summary>
        /// <remarks>
        /// The setter is generally only going to be used in unit tests, otherwise it will attempt to resolve it using the IOHelper.MapPath
        /// </remarks>
        internal static string MacroErrorConfigFilePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_macroErrorConfig))
                {
                    _macroErrorConfig = IOHelper.MapPath(SystemDirectories.Config + "/" + MacroErrorConfigFileName);
                }
                return _macroErrorConfig;
            }
            set { _macroErrorConfig = value; }
        }

        /// <summary>
        /// The cache storage for macro error configuration.
        /// </summary>
        private static List<MacroErrorConfig> MacroErrorConfiguration
        {
            get
            {
                //ensure cache exists
                EnsureCache();
                return HttpRuntime.Cache[CacheKey] as List<MacroErrorConfig>;
            }
            set
            {
                HttpRuntime.Cache.Insert(CacheKey, value);
            }
        }

        /// <summary>
        /// Gets or sets the regex match for this macro error - we match on the macro name.
        /// </summary>
        /// <value>The regular expression used to match the macro name.</value>
        public string Regex { get; set; }

        /// <summary>
        /// Gets or sets the desired behaviour when a matching macro causes an error. See
        /// <see cref="MacroErrorBehaviour"/> for definitions.
        /// </summary>
        /// <value>Macro error behaviour enum.</value>
        public MacroErrorBehaviour Behaviour { get; set; }

        /// <summary>
        /// Gets the matching configuration by regular expression matching the macro name.
        /// </summary>
        /// <param name="macroName">The macro name that is faulting.</param>
        /// <returns>A MacroErrorConfig instance</returns>
        public static MacroErrorConfig getForMacroName(string macroName)
        {
            var matchingMacroErrorConfig = MacroErrorConfiguration.Find(mec => (new Regex(mec.Regex, RegexOptions.IgnoreCase).IsMatch(macroName)));
            if (matchingMacroErrorConfig == null)
            {
                // Return an instance that allows the default behaviour of Umbraco to continue.
                return new MacroErrorConfig { Regex = ".*", Behaviour = MacroErrorBehaviour.Inline };
            }
            return matchingMacroErrorConfig;
        }

        /// <summary>
        /// Read all MacroErrorConfig data and store it in cache.
        /// </summary>
        private static void EnsureCache()
        {
            // Don't read the configuration file if the cache is not null
            if (HttpRuntime.Cache[CacheKey] != null)
                return;

            lock (Locker)
            {
                if (HttpRuntime.Cache[CacheKey] == null)
                {
                    var list = new List<MacroErrorConfig>();

                    LoadXml(doc =>
                    {
                        foreach (var addElement in doc.Root.Elements("add"))
                        {
                            var regex = addElement.Attribute("regex");
                            var behaviour = addElement.Attribute("behaviour");
                            if (regex == null)
                            {
                                throw new ArgumentException("macroErrors.config has an <add> entry with no regex attribute defined. Please use regex=\".*\" if you wish to match all macros.");
                            }
                            if (behaviour == null)
                            {
                                throw new ArgumentException("macroErrors.config has an <add> entry with no behaviour attribute defined. Please define behaviour with one of 'inline', 'silent' or 'throw'.");
                            }
                            MacroErrorBehaviour behaviourEnum;
                            try
                            {
                                behaviourEnum = (MacroErrorBehaviour)Enum.Parse(typeof(MacroErrorBehaviour), behaviour.Value, true);
                            }
                            catch (Exception)
                            {
                                throw new ArgumentException("macroErrors.config has an <add> entry with a behaviour attribute value that is not supported. Please define behaviour with one of 'inline', 'silent' or 'throw'.");
                            }

                            list.Add(new MacroErrorConfig
                            {
                                Regex = regex.Value,
                                Behaviour = behaviourEnum
                            });
                        }
                    });

                    MacroErrorConfiguration = list;
                }
            }
        }

        internal static void LoadXml(Action<XDocument> callback)
        {
            lock (Locker)
            {
                var doc = File.Exists(MacroErrorConfigFilePath)
                    ? XDocument.Load(MacroErrorConfigFilePath)
                    : XDocument.Parse("<?xml version=\"1.0\"?><macroErrors />");
                if (doc.Root != null)
                {
                    callback.Invoke(doc);
                }
            }
        }
    }
}
