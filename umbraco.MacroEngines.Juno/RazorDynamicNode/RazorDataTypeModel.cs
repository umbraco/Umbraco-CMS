using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.MacroEngines
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class RazorDataTypeModel : Attribute
    {
        public readonly Guid DataTypeEditorId;
        public readonly int Priority;

        public RazorDataTypeModel(string DataTypeEditorId)
        {
            this.DataTypeEditorId = new Guid(DataTypeEditorId);
            Priority = 100;
        }

        public RazorDataTypeModel(string DataTypeEditorId, int Priority)
        {
            this.DataTypeEditorId = new Guid(DataTypeEditorId);
            this.Priority = Priority;
        }
    }

}
