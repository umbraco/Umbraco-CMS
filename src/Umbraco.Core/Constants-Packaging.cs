namespace Umbraco.Core
{
    public static partial class Constants
    {
        /// <summary>
        ///     Defines the constants used for Umbraco packages in the package.config xml
        /// </summary>
        public static class Packaging
        {
            public const string UmbPackageNodeName = "umbPackage";
            public const string DataTypesNodeName = "DataTypes";
            public const string PackageXmlFileName = "package.xml";
            public const string UmbracoPackageExtention = ".umb";
            public const string DataTypeNodeName = "DataType";
            public const string LanguagesNodeName = "Languages";
            public const string FilesNodeName = "files";
            public const string StylesheetsNodeName = "Stylesheets";
            public const string TemplatesNodeName = "Templates";
            public const string NameNodeName = "Name";
            public const string TemplateNodeName = "Template";
            public const string AliasNodeNameSmall = "alias";
            public const string AliasNodeNameCapital = "Alias";
            public const string DictionaryItemsNodeName = "DictionaryItems";
            public const string DictionaryItemNodeName = "DictionaryItem";
            public const string MacrosNodeName = "Macros";
            public const string DocumentsNodeName = "Documents";
            public const string DocumentSetNodeName = "DocumentSet";
            public const string DocumentTypesNodeName = "DocumentTypes";
            public const string DocumentTypeNodeName = "DocumentType";
            public const string FileNodeName = "file";
            public const string OrgNameNodeName = "orgName";
            public const string OrgPathNodeName = "orgPath";
            public const string GuidNodeName = "guid";
            public const string StylesheetNodeName = "styleSheet";
            public const string MacroNodeName = "macro";
            public const string InfoNodeName = "info";
            public const string PackageRequirementsMajorXpath = "./package/requirements/major";
            public const string PackageRequirementsMinorXpath = "./package/requirements/minor";
            public const string PackageRequirementsPatchXpath = "./package/requirements/patch";
            public const string PackageNameXpath = "./package/name";
            public const string PackageVersionXpath = "./package/version";
            public const string PackageUrlXpath = "./package/url";
            public const string PackageLicenseXpath = "./package/license";
            public const string PackageLicenseXpathUrlAttribute = "url";
            public const string AuthorNameXpath = "./author/name";
            public const string AuthorWebsiteXpath = "./author/website";
            public const string ReadmeXpath = "./readme";
            public const string ControlNodeName = "control";
            public const string ActionNodeName = "Action";
            public const string ActionsNodeName = "Actions";
            public const string UndoNodeAttribute = "undo";
            public const string RunatNodeAttribute = "runat";
        }
    }
}