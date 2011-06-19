using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.MacroEngines
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class RazorDataTypeModel : Attribute
    {
        public readonly Guid DataTypeEditorId;

        public RazorDataTypeModel(string DataTypeEditorId)
        {
            this.DataTypeEditorId = new Guid(DataTypeEditorId);
        }


    }

}
