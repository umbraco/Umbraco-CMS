using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Compilation;
using Umbraco.Core.Configuration;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Composing
{

    /// <summary>
    /// An implementation of TypeFinder that uses the BuildManager to resolve references for aspnet framework hosted websites
    /// </summary>
    /// <remarks>
    /// This finder will also try to resolve dynamic assemblies created from App_Code
    /// </remarks>
    internal class BuildManagerTypeFinder : TypeFinder, ITypeFinder
    {

        public BuildManagerTypeFinder(
            ILogger logger,
            IAssemblyProvider assemblyProvider,
            ITypeFinderConfig typeFinderConfig = null) : base(logger, assemblyProvider, typeFinderConfig)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Explicitly implement and return result from BuildManager
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Type ITypeFinder.GetTypeByName (string name) => BuildManager.GetType(name, false);


        /// <summary>
        /// TypeFinder config via appSettings
        /// </summary>
        internal class TypeFinderConfig : ITypeFinderConfig
        {
            private readonly ITypeFinderSettings _settings;
            private IEnumerable<string> _assembliesAcceptingLoadExceptions;

            public TypeFinderConfig(ITypeFinderSettings settings)
            {
                _settings = settings;
            }

            public IEnumerable<string> AssembliesAcceptingLoadExceptions
            {
                get
                {
                    if (_assembliesAcceptingLoadExceptions != null)
                        return _assembliesAcceptingLoadExceptions;

                    var s = _settings.AssembliesAcceptingLoadExceptions;
                    return _assembliesAcceptingLoadExceptions = string.IsNullOrWhiteSpace(s)
                        ? Array.Empty<string>()
                        : s.Split(',').Select(x => x.Trim()).ToArray();
                }
            }
        }
    }
}
