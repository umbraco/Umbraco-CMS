using System;

namespace Umbraco.Core.Persistence.DatabaseAnnotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LengthAttribute : Attribute
    {
        public LengthAttribute(int length)
        {
            Length = length;
        }

        public int Length { get; private set; }
    }
}