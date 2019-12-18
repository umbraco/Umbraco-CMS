using System.CodeDom;
using System.Collections.Generic;

namespace Umbraco.ModelsBuilder.Building
{
    // NOTE
    // See nodes in Builder.cs class - that one does not work, is not complete,
    // and was just some sort of experiment...

    /// <summary>
    /// Implements a builder that works by using CodeDom
    /// </summary>
    internal class CodeDomBuilder : Builder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeDomBuilder"/> class with a list of models to generate.
        /// </summary>
        /// <param name="typeModels">The list of models to generate.</param>
        public CodeDomBuilder(IList<TypeModel> typeModels)
            : base(typeModels, null)
        { }

        /// <summary>
        /// Outputs a generated model to a code namespace.
        /// </summary>
        /// <param name="ns">The code namespace.</param>
        /// <param name="typeModel">The model to generate.</param>
        public void Generate(CodeNamespace ns, TypeModel typeModel)
        {
            // what about USING?
            // what about references?

            if (typeModel.IsMixin)
            {
                var i = new CodeTypeDeclaration("I" + typeModel.ClrName)
                {
                    IsInterface = true,
                    IsPartial = true,
                    Attributes = MemberAttributes.Public
                };
                i.BaseTypes.Add(typeModel.BaseType == null ? "IPublishedContent" : "I" + typeModel.BaseType.ClrName);

                foreach (var mixinType in typeModel.DeclaringInterfaces)
                    i.BaseTypes.Add(mixinType.ClrName);

                i.Comments.Add(new CodeCommentStatement($"Mixin content Type {typeModel.Id} with alias \"{typeModel.Alias}\""));

                foreach (var propertyModel in typeModel.Properties)
                {
                    var p = new CodeMemberProperty
                    {
                        Name = propertyModel.ClrName,
                        Type = new CodeTypeReference(propertyModel.ModelClrType),
                        Attributes = MemberAttributes.Public,
                        HasGet = true,
                        HasSet = false
                    };
                    i.Members.Add(p);
                }
            }

            var c = new CodeTypeDeclaration(typeModel.ClrName)
            {
                IsClass = true,
                IsPartial = true,
                Attributes = MemberAttributes.Public
            };

            c.BaseTypes.Add(typeModel.BaseType == null ? "PublishedContentModel" : typeModel.BaseType.ClrName);

            // if it's a missing it implements its own interface
            if (typeModel.IsMixin)
                c.BaseTypes.Add("I" + typeModel.ClrName);

            // write the mixins, if any, as interfaces
            // only if not a mixin because otherwise the interface already has them
            if (typeModel.IsMixin == false)
                foreach (var mixinType in typeModel.DeclaringInterfaces)
                    c.BaseTypes.Add("I" + mixinType.ClrName);

            foreach (var mixin in typeModel.MixinTypes)
                c.BaseTypes.Add("I" + mixin.ClrName);

            c.Comments.Add(new CodeCommentStatement($"Content Type {typeModel.Id} with alias \"{typeModel.Alias}\""));

            foreach (var propertyModel in typeModel.Properties)
            {
                var p = new CodeMemberProperty
                {
                    Name = propertyModel.ClrName,
                    Type = new CodeTypeReference(propertyModel.ModelClrType),
                    Attributes = MemberAttributes.Public,
                    HasGet = true,
                    HasSet = false
                };
                p.GetStatements.Add(new CodeMethodReturnStatement( // return
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(
                            new CodeThisReferenceExpression(), // this
                            "Value", // .Value
                            new[] // <T>
                            {
                                new CodeTypeReference(propertyModel.ModelClrType)
                            }),
                            new CodeExpression[] // ("alias")
                            {
                                new CodePrimitiveExpression(propertyModel.Alias)
                            })));
                c.Members.Add(p);
            }
        }
    }
}
