using System.Text;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Building.Interfaces
{
    public interface ITextBuilderSubActions
    {
        /// <summary>
        ///     Transforms input into valid XML comment string.
        /// </summary>
        /// <param name="input">Text to add as comment</param>
        /// <returns></returns>
        string XmlCommentString(string input);

        /// <summary>
        ///    Writes a property inherited by interface to the generated code.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="property">The property to be written</param>
        void WriteInterfaceProperty(StringBuilder sb, PropertyModel property);


        /// <summary>
        ///     Writes attributes to the generated code.
        ///     IE. [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "14.xxxxx")].
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="tabs">Indent with this string</param>
        void WriteGeneratedCodeAttribute(StringBuilder sb, string tabs);

        /// <summary>
        ///     Writes MaybeNull attributes to the generated code.
        ///     IE. [return: global::System.Diagnostics.CodeAnalysis.MaybeNull].
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="tabs">Indent with this string</param>
        /// <param name="isReturn">Adds return: to the attribute</param>
        void WriteMaybeNullAttribute(StringBuilder sb, string tabs, bool isReturn = false);

        /// <summary>
        ///    Writes Mixin property to the generated code.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="property">The property to be written</param>
        /// <param name="mixinClrName">Type of mixin</param>
        void WriteMixinProperty(StringBuilder sb, PropertyModel property, string mixinClrName);

        /// <summary>
        ///   Writes a property to the generated code.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="type">The type of the model</param>
        /// <param name="property">The property to be written</param>
        /// <param name="mixinClrName">Type of mixin</param>
        void WriteProperty(StringBuilder sb, TypeModel type, PropertyModel property, string? mixinClrName = null);


        void WriteClrType(StringBuilder sb, Type type);

        void WriteNonGenericClrType(StringBuilder sb, string s);

        void WriteClrType(StringBuilder sb, string type);
    }
}
