using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using umbraco.cms.helpers;

namespace umbraco.Linq.Core
{
    /// <summary>
    /// Class containing helpers when doing reflection against nodes
    /// </summary>
    public sealed class ReflectionAssistance
    {
        private ReflectionAssistance() { }

        /// <summary>
        /// Prebuilt function for getting the custom properites of the class
        /// </summary>
        /// <remarks>This is a Lambda function which will return all the properties of the current class which are custom DocType properties</remarks>
        internal static Func<PropertyInfo, bool> CustomDocTypeProperties = p => p.GetCustomAttributes(typeof(UmbracoInfoAttribute), false).Count() != 0 && p.GetCustomAttributes(typeof(PropertyAttribute), false).Count() != 0;

        /// <summary>
        /// Prebuild function for getting the mandatory properites of the class
        /// </summary>
        /// <remarks>This is a Lambda function which will return all the properties of the current class which are custom DocType properties and mandatory</remarks>
        internal static Func<PropertyInfo, bool> MandatoryDocTypeProperties = p => p.GetCustomAttributes(typeof(UmbracoInfoAttribute), false).Count() != 0 && ((UmbracoInfoAttribute)p.GetCustomAttributes(typeof(UmbracoInfoAttribute), false)[0]).Mandatory;

        /// <summary>
        /// Compares a .NET type to an Xml representation
        /// </summary>
        internal static Func<Type, XElement, bool> CompareByAlias = (t, x) => x.Name.LocalName == Casing.SafeAlias(ReflectionAssistance.GetUmbracoInfoAttribute(t).Alias);

        /// <summary>
        /// Get the <see cref="umbraco.Linq.Core.umbracoInfoAttribute"/> for a <see cref="System.Reflection.MethodInfo"/> object
        /// </summary>
        /// <param name="member">The methodInfo to get the <see cref="umbraco.Linq.Core.umbracoInfoAttribute"/> for.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "umbraco")]
        public static UmbracoInfoAttribute GetUmbracoInfoAttribute(MemberInfo member)
        {
            return member.GetCustomAttributes(typeof(UmbracoInfoAttribute), true).Cast<UmbracoInfoAttribute>().SingleOrDefault();
        }

        /// <summary>
        /// Get the <see cref="umbraco.Linq.Core.umbracoInfoAttribute"/> for a type
        /// </summary>
        /// <param name="type">The type to get the <see cref="umbraco.Linq.Core.umbracoInfoAttribute"/> for.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static UmbracoInfoAttribute GetUmbracoInfoAttribute(Type type)
        {
            return type.GetCustomAttributes(typeof(UmbracoInfoAttribute), true).Cast<UmbracoInfoAttribute>().SingleOrDefault();
        }
    }
}
