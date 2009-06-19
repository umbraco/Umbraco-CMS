using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.Linq.DTMetal.CodeBuilder.DataType
{
    [global::System.AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class DataTypeAttribute : Attribute
    {
        public DataTypeAttribute(string controlId)
        {
            this.ControlId = new Guid(controlId);
        }

        public Guid ControlId { get; private set; }
    }
}
