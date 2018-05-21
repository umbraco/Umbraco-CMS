using System;
using System.Globalization;

namespace Umbraco.ModelsBuilder.Umbraco
{
    // because, of course, it's internal in Umbraco
    // see also System.Web.Util.HashCodeCombiner
    class HashCombiner
    {
        private long _combinedHash = 5381L;

        public void Add(int i)
        {
            _combinedHash = ((_combinedHash << 5) + _combinedHash) ^ i;
        }

        public void Add(object o)
        {
            Add(o.GetHashCode());
        }

        public void Add(DateTime d)
        {
            Add(d.GetHashCode());
        }

        public void Add(string s)
        {
            if (s == null) return;
            Add((StringComparer.InvariantCulture).GetHashCode(s));
        }

        public string GetCombinedHashCode()
        {
            return _combinedHash.ToString("x", CultureInfo.InvariantCulture);
        }
    }
}
