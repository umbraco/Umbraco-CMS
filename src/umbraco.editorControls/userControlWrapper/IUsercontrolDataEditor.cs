using umbraco.cms.businesslogic.property;

namespace umbraco.editorControls.userControlGrapper
{
    public interface IUsercontrolDataEditor
    {
        object value { get; set;}
    }

    public interface IUsercontrolPropertyData
    {
        Property PropertyObject { set; }
    }
}
