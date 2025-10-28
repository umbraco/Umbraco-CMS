namespace Umbraco.Cms.Core;

/// <summary>
///     Defines constants.
/// </summary>
public static partial class Constants
{
    /// <summary>
    ///     Defines constants for ModelsBuilder.
    /// </summary>
    public static class ModelsBuilder
    {
        public const string DefaultModelsNamespace = "Umbraco.Cms.Web.Common.PublishedModels";

        public static class ModelsModes
        {
            public const string SourceCodeAuto = "SourceCodeAuto";

            public const string SourceCodeManual = "SourceCodeManual";

            public const string Nothing = "Nothing";
        }
    }
}
