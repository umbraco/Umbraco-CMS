using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// A resolver to resolve all registered validators
    /// </summary>
    internal class ValidatorsResolver : LazyManyObjectsResolverBase<ValidatorsResolver, ManifestValueValidator>
    {
        public ValidatorsResolver(IServiceProvider serviceProvider, ILogger logger, IEnumerable<Lazy<Type>> lazyTypeList)
            : base(serviceProvider, logger, lazyTypeList, ObjectLifetimeScope.Application)
        {
        }

        /// <summary>
        /// Returns the validators
        /// </summary>
        public IEnumerable<ManifestValueValidator> Validators
        {
            get { return Values; }
        }

        /// <summary>
        /// Gets a validator by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ManifestValueValidator GetValidator(string name)
        {
            return Values.FirstOrDefault(x => x.TypeName.InvariantEquals(name));
        } 
    }
}