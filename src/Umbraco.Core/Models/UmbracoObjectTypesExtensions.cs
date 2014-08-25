using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Extension methods for the UmbracoObjectTypes enum
    /// </summary>
    public static class UmbracoObjectTypesExtensions
    {
        //MUST be concurrent to avoid thread collisions!
        private static readonly ConcurrentDictionary<UmbracoObjectTypes, Guid> UmbracoObjectTypeCache = new ConcurrentDictionary<UmbracoObjectTypes, Guid>();

        /// <summary>
        /// Get an UmbracoObjectTypes value from it's name
        /// </summary>
        /// <param name="name">Enum value name</param>
        /// <returns>an UmbracoObjectTypes Enum value</returns>
        public static UmbracoObjectTypes GetUmbracoObjectType(string name)
        {
            return (UmbracoObjectTypes)Enum.Parse(typeof(UmbracoObjectTypes), name, false);
        }

        /// <summary>
        /// Get an instance of an UmbracoObjectTypes enum value from it's GUID
        /// </summary>
        /// <param name="guid">Enum value GUID</param>
        /// <returns>an UmbracoObjectTypes Enum value</returns>
        public static UmbracoObjectTypes GetUmbracoObjectType(Guid guid)
        {
            var umbracoObjectType = UmbracoObjectTypes.Unknown;

            foreach (var name in Enum.GetNames(typeof(UmbracoObjectTypes)))
            {
                if (GetUmbracoObjectType(name).GetGuid() == guid)
                {
                    umbracoObjectType = GetUmbracoObjectType(name);
                }
            }

            return umbracoObjectType;
        }

        /// <summary>
        /// Extension method for the UmbracoObjectTypes enum to return the enum GUID
        /// </summary>
        /// <param name="umbracoObjectType">UmbracoObjectTypes Enum value</param>
        /// <returns>a GUID value of the UmbracoObjectTypes</returns>
        public static Guid GetGuid(this UmbracoObjectTypes umbracoObjectType)
        {
            return UmbracoObjectTypeCache.GetOrAdd(umbracoObjectType, types =>
                {
                    var type = typeof(UmbracoObjectTypes);
                    var memInfo = type.GetMember(umbracoObjectType.ToString());
                    var attributes = memInfo[0].GetCustomAttributes(typeof(UmbracoObjectTypeAttribute),
                        false);

                    if (attributes.Length == 0)
                        return Guid.Empty;

                    var attribute = ((UmbracoObjectTypeAttribute)attributes[0]);
                    if (attribute == null)
                        return Guid.Empty;

                    return attribute.ObjectId;
                });
        }

        /// <summary>
        /// Extension method for the UmbracoObjectTypes enum to return the enum name
        /// </summary>
        /// <param name="umbracoObjectType">UmbracoObjectTypes value</param>
        /// <returns>The enum name of the UmbracoObjectTypes value</returns>
        public static string GetName(this UmbracoObjectTypes umbracoObjectType)
        {
            return Enum.GetName(typeof(UmbracoObjectTypes), umbracoObjectType);
        }

        /// <summary>
        /// Extension method for the UmbracoObejctTypes enum to return the enum friendly name
        /// </summary>
        /// <param name="umbracoObjectType">UmbracoObjectTypes value</param>
        /// <returns>a string of the FriendlyName</returns>
        public static string GetFriendlyName(this UmbracoObjectTypes umbracoObjectType)
        {
            var type = typeof(UmbracoObjectTypes);
            var memInfo = type.GetMember(umbracoObjectType.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(FriendlyNameAttribute),
                false);

            if (attributes.Length == 0)
                return string.Empty;

            var attribute = ((FriendlyNameAttribute)attributes[0]);
            if (attribute == null)
                return string.Empty;

            return attribute.ToString();
        }
    }
}