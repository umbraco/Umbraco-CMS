using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Umbraco.Core.Composing;
using Umbraco.Core.Strings;

namespace Umbraco.Core
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Tries to return a value based on a property name for an object but ignores case sensitivity
        /// </summary>
        /// <param name="type"></param>
        /// <param name="shortStringHelper"></param>
        /// <param name="target"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        /// <remarks>
        /// Currently this will only work for ProperCase and camelCase properties, see the TODO below to enable complete case insensitivity
        /// </remarks>
        internal static Attempt<object> GetMemberIgnoreCase(this Type type, IShortStringHelper shortStringHelper, object target, string memberName)
        {
            Func<string, Attempt<object>> getMember =
                memberAlias =>
                {
                    try
                    {
                        return Attempt<object>.Succeed(
                                                   type.InvokeMember(memberAlias,
                                                                                  System.Reflection.BindingFlags.GetProperty |
                                                                                  System.Reflection.BindingFlags.Instance |
                                                                                  System.Reflection.BindingFlags.Public,
                                                                                  null,
                                                                                  target,
                                                                                  null));
                    }
                    catch (MissingMethodException ex)
                    {
                        return Attempt<object>.Fail(ex);
                    }
                };

            //try with the current casing
            var attempt = getMember(memberName);
            if (attempt.Success == false)
            {
                //if we cannot get with the current alias, try changing it's case
                attempt = memberName[0].IsUpperCase()
                    ? getMember(memberName.ToCleanString(shortStringHelper, CleanStringType.Ascii | CleanStringType.ConvertCase | CleanStringType.CamelCase))
                    : getMember(memberName.ToCleanString(shortStringHelper, CleanStringType.Ascii | CleanStringType.ConvertCase | CleanStringType.PascalCase));

                // TODO: If this still fails then we should get a list of properties from the object and then compare - doing the above without listing
                // all properties will surely be faster than using reflection to get ALL properties first and then query against them.
            }

            return attempt;
        }

    }
}
