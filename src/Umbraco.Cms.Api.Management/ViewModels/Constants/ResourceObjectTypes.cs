namespace Umbraco.Cms.Api.Management.ViewModels.Constants;

public static class ResourceObjectTypes
{
    public static string Template => Core.Constants.UdiEntityType.Template;

    public static string Script => Core.Constants.UdiEntityType.Script;

    public static string Stylesheet => Core.Constants.UdiEntityType.Stylesheet;

    public static string RelationType => Core.Constants.UdiEntityType.RelationType;

    public static string PartialView => Core.Constants.UdiEntityType.PartialView;

    public static string MemberType => Core.Constants.UdiEntityType.MemberType;

    public static string MemberGroup => Core.Constants.UdiEntityType.MemberGroup;

    public static string DocumentType => Core.Constants.UdiEntityType.DocumentType;

    public static string Document => Core.Constants.UdiEntityType.Document;

    public static string DocumentBlueprint => Core.Constants.UdiEntityType.DocumentBlueprint;

    public static string DictionaryItem => Core.Constants.UdiEntityType.DictionaryItem;

    public static string DataType => Core.Constants.UdiEntityType.DataType;

    public static string Language => Core.Constants.UdiEntityType.Language;

    public static string MediaType => Core.Constants.UdiEntityType.MediaType;

    public static string Media => Core.Constants.UdiEntityType.Media;

    public static string Member => Core.Constants.UdiEntityType.Member;

    public const string User = "user";
    public const string UserGroup = "user-group";
    public const string UserData = "user-data";
    public const string UserPermissions = "user-permissions";
    public const string Relation = "relation";
    public const string TemplateScaffold = "template-scaffold";
    public const string Tag = "tag";
    public const string StaticFile = "static-file";
    public const string DocumentInRecycleBin = "document-recyclebin";
    public const string MediaInRecycleBin = "media-recyclebin";
}
