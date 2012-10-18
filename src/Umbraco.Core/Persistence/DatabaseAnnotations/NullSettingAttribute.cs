using System;

namespace Umbraco.Core.Persistence.DatabaseAnnotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NullSettingAttribute : Attribute
    {
        public NullSettings NullSetting { get; set; }
    }
}