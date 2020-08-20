using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core
{
    //https://gist.github.com/DForshner/5688879
    public class PropertyComparer<T> : IEqualityComparer<T>
    {
        private Expression<Func<T, object>>[] properties;

        /// <summary>
        /// Creates an instance of PropertyComparer with a single property to compare.
        /// </summary>
        /// <param name="propertyName">Property to perform the comparison on.</param>
        public PropertyComparer(Expression<Func<T, object>> property)
        {
            ThrowExceptionIfPropertyIsNull(property);

            this.properties = new Expression<Func<T, object>>[] { property };
        }

        private static void ThrowExceptionIfPropertyIsNull(Expression<Func<T, object>> property)
        {
            if (property == null)
                throw new NullReferenceException("Property expression cannot be null.");
        }

        /// <summary>
        /// Creates an instance of PropertyComparer using an array of properties to compare.
        /// </summary>
        /// <param name="propertyName">Array of properties to perform comparisons on.</param>
        public PropertyComparer(Expression<Func<T, object>>[] properties)
        {
            if (properties.Length == 0)
                throw new ArgumentException("Array must contain at least on property to compare");

            foreach (var property in properties)
                ThrowExceptionIfPropertyIsNull(property);

            this.properties = properties;
        }

        public bool Equals(T x, T y)
        {
            // Check each property and return false if any fail to match.
            foreach (Expression<Func<T, object>> property in properties)
            {
                if (!PropertyEquals(x, y, property))
                    return false;
            }

            return true;
        }

        private bool PropertyEquals(T x, T y, Expression<Func<T, object>> property)
        {
            object xValue = property.Compile()(x);
            object yValue = property.Compile()(y);

            // If the xValue is null then it is equal only if yValue is also Null;
            if (xValue == null)
                return yValue == null;

            // Use the default type Equals comparer.
            return xValue.Equals(yValue);
        }

        /// <summary>
        /// Returns the hash code of the object using the configured properties.
        /// </summary>
        /// <param name="obj">Object to get hash code for</param>
        /// <returns></returns>
        public int GetHashCode(T obj)
        {
            if (properties.Length == 1)
                return GetHashCodeForSingleProperty(obj);

            return GetHashCodeForMultipleProperties(obj);
        }

        /// <summary>
        /// In case where there is only a single property just return the properties default GetHashCode method.
        /// </summary>
        private int GetHashCodeForSingleProperty(T obj)
        {
            object objValue = this.properties[0].Compile()(obj);

            if (objValue == null)
                return 0;
            else
                return objValue.GetHashCode();
        }

        /// <summary>
        /// Hashing strategy is based on http://stackoverflow.com/questions/1646807/quick-and-simple-hash-code-combinations
        /// </summary>
        private int GetHashCodeForMultipleProperties(T obj)
        {
            int hash = 17;

            // Check each property and return false if any fail to match.
            foreach (Expression<Func<T, object>> property in properties)
            {
                object objValue = property.Compile()(obj);

                if (objValue == null)
                    hash = hash * 31;
                else
                    hash = hash * 31 + objValue.GetHashCode();
            }

            return hash;
        }
    }

}
