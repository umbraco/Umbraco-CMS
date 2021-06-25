// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.PublishedCache.Internal;

namespace Umbraco.Cms.Tests.Common.TestHelpers.PublishedContent
{
    public class InternalPublishedPropertyWithLanguageVariants : InternalPublishedProperty
    {
        private readonly IDictionary<string, object> _solidSourceValues = new Dictionary<string, object>();
        private readonly IDictionary<string, object> _solidValues = new Dictionary<string, object>();
        private readonly IDictionary<string, object> _solidXPathValues = new Dictionary<string, object>();

        public override object GetSourceValue(string culture = null, string segment = null)
        {
            if (string.IsNullOrEmpty(culture))
            {
                return base.GetSourceValue(culture, segment);
            }

            return _solidSourceValues.ContainsKey(culture) ? _solidSourceValues[culture] : null;
        }

        public override object GetValue(string culture = null, string segment = null)
        {
            if (string.IsNullOrEmpty(culture))
            {
                return base.GetValue(culture, segment);
            }

            return _solidValues.ContainsKey(culture) ? _solidValues[culture] : null;
        }

        public override object GetXPathValue(string culture = null, string segment = null)
        {
            if (string.IsNullOrEmpty(culture))
            {
                return base.GetXPathValue(culture, segment);
            }

            return _solidXPathValues.ContainsKey(culture) ? _solidXPathValues[culture] : null;
        }

        public override bool HasValue(string culture = null, string segment = null)
        {
            if (string.IsNullOrEmpty(culture))
            {
                return base.HasValue(culture, segment);
            }

            return _solidSourceValues.ContainsKey(culture);
        }

        public void SetSourceValue(string culture, object value, bool defaultValue = false)
        {
            _solidSourceValues.Add(culture, value);
            if (defaultValue)
            {
                SolidSourceValue = value;
                SolidHasValue = true;
            }
        }

        public void SetValue(string culture, object value, bool defaultValue = false)
        {
            _solidValues.Add(culture, value);
            if (defaultValue)
            {
                SolidValue = value;
                SolidHasValue = true;
            }
        }

        public void SetXPathValue(string culture, object value, bool defaultValue = false)
        {
            _solidXPathValues.Add(culture, value);
            if (defaultValue)
            {
                SolidXPathValue = value;
            }
        }
    }
}
