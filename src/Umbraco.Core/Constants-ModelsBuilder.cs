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
        /// <summary>
        ///     The default namespace used for generated models.
        /// </summary>
        public const string DefaultModelsNamespace = "Umbraco.Cms.Web.Common.PublishedModels";

        /// <summary>
        ///     Defines the available ModelsBuilder modes.
        /// </summary>
        public static class ModelsModes
        {
            /// <summary>
            ///     Mode where source code is automatically generated and compiled at runtime.
            /// </summary>
            public const string SourceCodeAuto = "SourceCodeAuto";

            /// <summary>
            ///     Mode where source code is generated but requires manual compilation.
            /// </summary>
            public const string SourceCodeManual = "SourceCodeManual";

            /// <summary>
            ///     Mode where no models are generated.
            /// </summary>
            public const string Nothing = "Nothing";
        }
    }
}
