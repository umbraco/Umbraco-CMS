namespace Umbraco.Core
{
    public static partial class Constants
    {
        /// <summary>
        ///     Defines the constants used for Umbraco packages in the package.config xml
        /// </summary>
        public static class Packaging
        {
            public static readonly string UmbPackageNodeName = "umbPackage";
            public static readonly string DataTypesNodeName = "DataTypes";
            public static readonly string PackageXmlFileName = "package.xml";
            public static readonly string UmbracoPackageExtention = ".umb";
            public static readonly string DataTypeNodeName = "DataType";
            public static readonly string LanguagesNodeName = "Languages";
            public static readonly string FilesNodeName = "files";
            public static readonly string StylesheetsNodeName = "Stylesheets";
            public static readonly string TemplatesNodeName = "Templates";
            public static readonly string NameNodeName = "Name";
            public static readonly string TemplateNodeName = "Template";
            public static readonly string AliasNodeNameSmall = "alias";
            public static readonly string AliasNodeNameCapital = "Alias";
            public static readonly string DictionaryItemsNodeName = "DictionaryItems";
            public static readonly string DictionaryItemNodeName = "DictionaryItem";
            public static readonly string MacrosNodeName = "Macros";
            public static readonly string DocumentSetNodeName = "DocumentSet";
            public static readonly string DocumentTypesNodeName = "DocumentTypes";
            public static readonly string DocumentTypeNodeName = "DocumentType";
            public static readonly string FileNodeName = "file";
            public static readonly string OrgNameNodeName = "orgName";
            public static readonly string OrgPathNodeName = "orgPath";
            public static readonly string GuidNodeName = "guid";
            public static readonly string StylesheetNodeName = "styleSheet";
            public static readonly string MacroNodeName = "macro";
            public static readonly string InfoNodeName = "info";
            public static readonly string PackageRequirementsMajorXpath = "./package/requirements/major";
            public static readonly string PackageRequirementsMinorXpath = "./package/requirements/minor";
            public static readonly string PackageRequirementsPatchXpath = "./package/requirements/patch";
            public static readonly string PackageNameXpath = "./package/name";
            public static readonly string PackageVersionXpath = "./package/version";
            public static readonly string PackageUrlXpath = "./package/url";
            public static readonly string PackageLicenseXpath = "./package/license";
            public static readonly string AuthorNameXpath = "./author/name";
            public static readonly string AuthorWebsiteXpath = "./author/website";
            public static readonly string ReadmeXpath = "./readme";
            public static readonly string ControlNodeName = "control";
            public static readonly string ActionNodeName = "Action";
            public static readonly string ActionsNodeName = "Actions";
            public static readonly string UndoNodeAttribute = "undo";
            public static readonly string RunatNodeAttribute = "runat";
        }
    }
}