using System.Text;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Building.Interfaces
{
    public interface ITextBuilderActions
    {
        /// <summary>
        ///     Writes a header to top of the generated code.
        ///     Before usings and namespace.
        /// </summary>
        /// <param name="sb"></param>
        void WriteHeader(StringBuilder sb);

        /// <summary>
        ///    Writes a marker for assembly attributes.
        ///    Used by in-memory models builder.
        /// </summary>
        /// <param name="sb"></param>
        void WriteAssemblyAttributesMarker(StringBuilder sb);

        /// <summary>
        ///    Writes the using directives to the generated code.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="typeUsing"></param>
        void WriteUsing(StringBuilder sb, IEnumerable<string> typeUsing);

        /// <summary>
        ///     Writes the namespace to the generated code.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="modelNamespace"></param>
        void WriteNamespace(StringBuilder sb, string modelNamespace);

        /// <summary>
        ///    Writes the content type to the generated code.
        ///    IE. the class declaration.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="type"></param>
        /// <param name="lineBreak"></param>
        void WriteContentType(StringBuilder sb, TypeModel type, bool lineBreak);

        /// <summary>
        ///     Writes the properties of the content type to the generated code.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="type"></param>
        void WriteContentTypeProperties(StringBuilder sb, TypeModel type);

        /// <summary>
        ///    Entry point for extensions that wants to add custom code to the generated class.
        ///    Does nothing by default.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="typeMode"></param>
        void WriteCustomCodeBeforeClassClose(StringBuilder sb, TypeModel typeMode);
    }
}
