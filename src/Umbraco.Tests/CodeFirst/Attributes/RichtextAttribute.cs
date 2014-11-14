using System;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.CodeFirst.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class RichtextAttribute : PropertyTypeAttribute
    {
        public RichtextAttribute()
            : base(typeof(RichTextPropertyEditor))
        {
        }
    }
}