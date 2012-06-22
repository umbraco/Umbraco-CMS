using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace umbraco.cms.businesslogic
{
    [Obsolete("umbraco.cms.businesslogic.Tuple<T,T2> is Obsolete, use System.Tuple instead")]
    public class Tuple<T, T2> : IEquatable<Tuple<T, T2>>
    {
        public T first { get; set; }
        public T second { get; set; }

        public bool Equals(Tuple<T, T2> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.first, this.first) && Equals(other.second, this.second);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(Tuple<T, T2>)) return false;
            return Equals((Tuple<T, T2>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.first != null ? this.first.GetHashCode() : 0) * 397) ^ (this.second != null ? this.second.GetHashCode() : 0);
            }
        }

    }
}
