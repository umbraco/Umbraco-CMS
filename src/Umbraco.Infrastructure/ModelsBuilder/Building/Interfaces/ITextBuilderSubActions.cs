using System.Text;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Building.Interfaces
{
    public interface ITextBuilderSubActions
    {
        /// <summary>
        ///     Transforms input into valid XML comment string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        string XmlCommentString(string input);

        /// <summary>
        ///    Writes a property inherited by interface to the generated code.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="property"></param>
        void WriteInterfaceProperty(StringBuilder sb, PropertyModel property);


        /// <summary>
        ///     Writes attributes to the generated code.
        ///     IE. [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "14.xxxxx")].
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="tabs"></param>        
        void WriteGeneratedCodeAttribute(StringBuilder sb, string tabs);

        /// <summary>
        ///     Writes MaybeNull attributes to the generated code.
        ///     IE. [return: global::System.Diagnostics.CodeAnalysis.MaybeNull].
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="tabs"></param>
        /// <param name="isReturn"></param>
        void WriteMaybeNullAttribute(StringBuilder sb, string tabs, bool isReturn = false);

        /// <summary>
        ///    Writes Mixin property to the generated code.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="property"></param>
        /// <param name="mixinClrName"></param>
        void WriteMixinProperty(StringBuilder sb, PropertyModel property, string mixinClrName);

        /// <summary>
        ///   Writes a property to the generated code.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="type"></param>
        /// <param name="property"></param>
        /// <param name="mixinClrName"></param>
        void WriteProperty(StringBuilder sb, TypeModel type, PropertyModel property, string? mixinClrName = null);


        void WriteClrType(StringBuilder sb, Type type);

        void WriteNonGenericClrType(StringBuilder sb, string s);

        void WriteClrType(StringBuilder sb, string type);
    }
}
