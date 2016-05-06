namespace Umbraco.Core.PropertyEditors.ValueCorrectors
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
   
    using Umbraco.Core.ObjectResolution;

    /// <summary>
    /// A resolver for <see cref="ILegacyDbPropertyValueCorrection"/>.
    /// </summary>
    internal class LegacyDbPropertyValueCorrectionResolver : ResolverBase<LegacyDbPropertyValueCorrectionResolver>
    {
        /// <summary>
        /// The cache of resolved correction instances.
        /// </summary>
        private readonly ConcurrentDictionary<string, DbPropertyValueCorrectionBase> _correctionCache = new ConcurrentDictionary<string, DbPropertyValueCorrectionBase>();

        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyDbPropertyValueCorrectionResolver"/> class.
        /// </summary>
        /// <param name="values">
        /// The values.
        /// </param>
        internal LegacyDbPropertyValueCorrectionResolver(IEnumerable<Type> values)
        {
            this.BuildCache(values);
        }


        /// <summary>
        /// Applies the resolved correction if there are any.
        /// </summary>
        /// <param name="propertyEditorAlias">
        /// The property editor alias.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The corrected value <see cref="object"/>.
        /// </returns>
        public object CorrectedValue(string propertyEditorAlias, object value)
        {
            var correction = this._correctionCache.FirstOrDefault(x => x.Key == propertyEditorAlias).Value;
            return correction == null ? value : correction.ApplyCorrection(value);
        }

        /// <summary>
        /// Builds the type cache.
        /// </summary>
        /// <param name="values">
        /// The values.
        /// </param>
        private void BuildCache(IEnumerable<Type> values)
        {
            var attempts = values.Select(type => ActivatorHelper.CreateInstance<DbPropertyValueCorrectionBase>(type, new object[] { })).Where(x => x.Success);
            foreach (var attempt in attempts)
            {
                this.AddOrUpdateCache(attempt.Result);
            }
        }

        /// <summary>
        /// Adds or updates a <see cref="DbPropertyValueCorrectionBase"/> instance to the concurrent cache.
        /// </summary>
        /// <param name="correction">
        /// The <see cref="DbPropertyValueCorrectionBase"/>.
        /// </param>
        private void AddOrUpdateCache(DbPropertyValueCorrectionBase correction)
        {
            var att = correction.GetType().GetCustomAttribute<DbPropertyValueCorrectionAttribute>(false);
            if (att != null)
            {
                this._correctionCache.AddOrUpdate(att.PropertyEditorAlias, correction, (x, y) => correction);
            }
        }
    }
}