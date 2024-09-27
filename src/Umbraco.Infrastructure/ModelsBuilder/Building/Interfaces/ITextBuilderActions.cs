using System.Text;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Building.Interfaces
{
    public interface ITextBuilderActions
    {
        /// <summary>
        ///     Writes a header to top of the generated code.
        ///     Before usings and namespace.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        void WriteHeader(StringBuilder sb);

        /// <summary>
        ///    Writes a marker for assembly attributes.
        ///    Used by in-memory models builder.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        void WriteAssemblyAttributesMarker(StringBuilder sb);

        /// <summary>
        ///    Writes the using directives to the generated code.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="typeUsing">List of using statements</param>
        void WriteUsing(StringBuilder sb, IEnumerable<string> typeUsing);

        /// <summary>
        ///     Writes the namespace to the generated code.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="modelNamespace">Namespace of the model</param>
        void WriteNamespace(StringBuilder sb, string modelNamespace);

        /// <summary>
        ///    Writes the content type to the generated code.
        ///    IE. the class declaration.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="type">The type of the model</param>
        /// <param name="lineBreak">whether to add linebreak</param>
        void WriteContentType(StringBuilder sb, TypeModel type, bool lineBreak);

        /// <summary>
        ///     Writes the properties of the content type to the generated code.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="type">The type of the model</param>
        void WriteContentTypeProperties(StringBuilder sb, TypeModel type);

        /// <summary>
        ///    Entry point for extensions that wants to add custom code to the generated class.
        ///    Does nothing by default.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="typeMode">The type of the model</param>
        void WriteCustomCodeBeforeClassClose(StringBuilder sb, TypeModel typeMode);
    }
}
