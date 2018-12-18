using System;
using System.Collections.Generic;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Compares <see cref="UrlInfo"/>
    /// </summary>
    public class UrlInfoComparer : IEqualityComparer<UrlInfo>
    {
        private readonly bool _variesByCulture;

        public UrlInfoComparer(bool variesByCulture)
        {
            _variesByCulture = variesByCulture;
        }

        /// <summary>
        /// Determines equality between <see cref="UrlInfo"/>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        /// <remarks>
        /// If variesByCulture is true, then culture is compared, otherwise culture is not compared.
        /// Both culture and url are compared without case sensitivity.
        /// </remarks>
        public bool Equals(UrlInfo x, UrlInfo y)
        {
            if (ReferenceEquals(null, y)) return false;
            if (ReferenceEquals(null, x)) return false;
            if (ReferenceEquals(x, y)) return true;

            if (_variesByCulture)
            {
                return string.Equals(x.Culture, y.Culture, StringComparison.InvariantCultureIgnoreCase)
                       && x.IsUrl == y.IsUrl
                       && string.Equals(x.Text, y.Text, StringComparison.InvariantCultureIgnoreCase);
            }

            return x.IsUrl == y.IsUrl
                   && string.Equals(x.Text, y.Text, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Calculates a hash code
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <remarks>
        /// If variesByCulture is true then culture is used in the calculation, otherwise it's not
        /// </remarks>
        public int GetHashCode(UrlInfo obj)
        {
            unchecked
            {
                var hashCode = (obj.Text != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.Text) : 0);
                hashCode = (hashCode * 397) ^ obj.IsUrl.GetHashCode();
                if (_variesByCulture)
                    hashCode = (hashCode * 397) ^ (obj.Culture != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.Culture) : 0);
                return hashCode;
            }

        }
    }
}