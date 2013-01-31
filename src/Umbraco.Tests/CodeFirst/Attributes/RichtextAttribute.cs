using System;
using umbraco.editorControls.tinyMCE3;

namespace Umbraco.Tests.CodeFirst.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class RichtextAttribute : PropertyTypeAttribute
    {
        public RichtextAttribute()
            : base(typeof(tinyMCE3dataType))
        {
        }
    }
}