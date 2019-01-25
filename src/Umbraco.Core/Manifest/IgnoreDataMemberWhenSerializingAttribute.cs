using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Manifest
{
    /// <summary>
    /// When applied to the member of a type, specifies that the member is not part of a data contract when
    /// serialized but still when deserialized as opposite to <see cref="IgnoreDataMemberAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class IgnoreDataMemberWhenSerializingAttribute : Attribute
    {

    }
}
