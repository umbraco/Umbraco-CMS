using System;
using System.Collections.Generic;
using System.Reflection;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// The name of a content variant for a given culture
    /// </summary>
    public class CultureName : BeingDirtyBase, IDeepCloneable, IEquatable<CultureName>
    {
        private DateTime _date;
        private string _name;
        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        public CultureName(string culture, string name, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(culture)) throw new ArgumentException("message", nameof(culture));
            Culture = culture;
            _name = name;
            _date = date;
        }

        public string Culture { get; private set; }

        public string Name
        {
            get => _name;
            set => SetPropertyValueAndDetectChanges(value, ref _name, Ps.Value.NameSelector);
        }

        public DateTime Date
        {
            get => _date;
            set => SetPropertyValueAndDetectChanges(value, ref _date, Ps.Value.DateSelector);
        }

        public object DeepClone()
        {
            return new CultureName(Culture, Name, Date);
        }

        public override bool Equals(object obj)
        {
            return obj is CultureName && Equals((CultureName)obj);
        }

        public bool Equals(CultureName other)
        {
            return Culture == other.Culture &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            var hashCode = 479558943;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Culture);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }

        /// <summary>
        /// Allows deconstructing into culture and name
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="name"></param>
        public void Deconstruct(out string culture, out string name)
        {
            culture = Culture;
            name = Name;
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class PropertySelectors
        {
            public readonly PropertyInfo CultureSelector = ExpressionHelper.GetPropertyInfo<CultureName, string>(x => x.Culture);
            public readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<CultureName, string>(x => x.Name);
            public readonly PropertyInfo DateSelector = ExpressionHelper.GetPropertyInfo<CultureName, DateTime>(x => x.Date);
        }
    }
}
