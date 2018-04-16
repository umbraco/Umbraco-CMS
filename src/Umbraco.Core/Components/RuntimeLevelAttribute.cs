using System;

namespace Umbraco.Core.Components
{
    [AttributeUsage(AttributeTargets.Class /*, AllowMultiple = false, Inherited = true*/)]
    public class RuntimeLevelAttribute : Attribute
    {
        //public RuntimeLevelAttribute()
        //{ }

        public RuntimeLevel MinLevel { get; set; } = RuntimeLevel.Boot;
    }
}
