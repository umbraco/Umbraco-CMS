using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.MacroEngines
{
    public interface IRazorDataTypeModel
    {
        bool Init(int CurrentNodeId, string PropertyData, out object instance);
    }
}
