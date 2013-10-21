using System;
using umbraco.cms.businesslogic.property;

namespace umbraco.editorControls.userControlGrapper
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public interface IUsercontrolDataEditor
    {
        object value { get; set;}
    }

    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public interface IUsercontrolPropertyData
    {
        Property PropertyObject { set; }
    }
}
