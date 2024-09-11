using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.ModelsBuilder.Building.Interfaces;
using NPoco.Expressions;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Building
{
    public class TextBuilderSubActions : ITextBuilderSubActions
    {
        private readonly IBuilderBase _builderBase;
        private Dictionary<string, string> ModelsMap { get; } = new();
        public TextBuilderSubActions(IBuilderBase builderBase)
        {
            _builderBase = builderBase;
        }
        public void WriteGeneratedCodeAttribute(StringBuilder sb, string tabs) =>
            sb.AppendFormat(
        "{0}[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"Umbraco.ModelsBuilder.Embedded\", \"{1}\")]\n",
        tabs, ApiVersion.Current.Version);
        public void WriteInterfaceProperty(StringBuilder sb, PropertyModel property)
        {
            if (property.Errors != null)
            {
                sb.Append("\t\t/*\n");
                sb.Append("\t\t * THIS PROPERTY CANNOT BE IMPLEMENTED, BECAUSE:\n");
                sb.Append("\t\t *\n");
                var first = true;
                foreach (var error in property.Errors)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append("\t\t *\n");
                    }

                    foreach (var s in SplitError(error))
                    {
                        sb.Append("\t\t * ");
                        sb.Append(s);
                        sb.Append("\n");
                    }
                }

                sb.Append("\t\t *\n");
                sb.Append("\n");
            }

            if (!string.IsNullOrWhiteSpace(property.Name))
            {
                sb.AppendFormat("\t\t/// <summary>{0}</summary>\n", XmlCommentString(property.Name));
            }

            WriteGeneratedCodeAttribute(sb, "\t\t");
            if (!property.ModelClrType.IsValueType)
            {
                WriteMaybeNullAttribute(sb, "\t\t");
            }

            sb.Append("\t\t");
            WriteClrType(sb, property.ClrTypeName);
            sb.AppendFormat(
                " {0} {{ get; }}\n",
                property.ClrName);

            if (property.Errors != null)
            {
                sb.Append("\n");
                sb.Append("\t\t *\n");
                sb.Append("\t\t */\n");
            }
        }

        public void WriteMaybeNullAttribute(StringBuilder sb, string tabs, bool isReturn = false) =>
            sb.AppendFormat("{0}[{1}global::System.Diagnostics.CodeAnalysis.MaybeNull]\n", tabs,
            isReturn ? "return: " : string.Empty);
        public void WriteMixinProperty(StringBuilder sb, PropertyModel property, string mixinClrName)
        {
            sb.Append("\n");

            // Adds xml summary to each property containing
            // property name and property description
            if (!string.IsNullOrWhiteSpace(property.Name) || !string.IsNullOrWhiteSpace(property.Description))
            {
                sb.Append("\t\t///<summary>\n");

                if (!string.IsNullOrWhiteSpace(property.Description))
                {
                    sb.AppendFormat("\t\t/// {0}: {1}\n", XmlCommentString(property.Name),
                        XmlCommentString(property.Description));
                }
                else
                {
                    sb.AppendFormat("\t\t/// {0}\n", XmlCommentString(property.Name));
                }

                sb.Append("\t\t///</summary>\n");
            }

            WriteGeneratedCodeAttribute(sb, "\t\t");

            if (!property.ModelClrType.IsValueType)
            {
                WriteMaybeNullAttribute(sb, "\t\t");
            }

            sb.AppendFormat("\t\t[ImplementPropertyType(\"{0}\")]\n", property.Alias);

            sb.Append("\t\tpublic virtual ");
            WriteClrType(sb, property.ClrTypeName);

            sb.AppendFormat(
                " {0} => ",
                property.ClrName);
            WriteNonGenericClrType(sb, _builderBase.GetModelsNamespace() + "." + mixinClrName);
            sb.AppendFormat(
                ".{0}(this, _publishedValueFallback);\n",
                MixinStaticGetterName(property.ClrName));
        }
        public void WriteProperty(StringBuilder sb, TypeModel type, PropertyModel property, string? mixinClrName = null)
        {
            var mixinStatic = mixinClrName != null;

            sb.Append("\n");

            if (property.Errors != null)
            {
                sb.Append("\t\t/*\n");
                sb.Append("\t\t * THIS PROPERTY CANNOT BE IMPLEMENTED, BECAUSE:\n");
                sb.Append("\t\t *\n");
                var first = true;
                foreach (var error in property.Errors)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append("\t\t *\n");
                    }

                    foreach (var s in SplitError(error))
                    {
                        sb.Append("\t\t * ");
                        sb.Append(s);
                        sb.Append("\n");
                    }
                }

                sb.Append("\t\t *\n");
                sb.Append("\n");
            }

            // Adds xml summary to each property containing
            // property name and property description
            if (!string.IsNullOrWhiteSpace(property.Name) || !string.IsNullOrWhiteSpace(property.Description))
            {
                sb.Append("\t\t///<summary>\n");

                if (!string.IsNullOrWhiteSpace(property.Description))
                {
                    sb.AppendFormat("\t\t/// {0}: {1}\n", XmlCommentString(property.Name),
                        XmlCommentString(property.Description));
                }
                else
                {
                    sb.AppendFormat("\t\t/// {0}\n", XmlCommentString(property.Name));
                }

                sb.Append("\t\t///</summary>\n");
            }

            WriteGeneratedCodeAttribute(sb, "\t\t");
            if (!property.ModelClrType.IsValueType)
            {
                WriteMaybeNullAttribute(sb, "\t\t");
            }

            sb.AppendFormat("\t\t[ImplementPropertyType(\"{0}\")]\n", property.Alias);

            if (mixinStatic)
            {
                sb.Append("\t\tpublic virtual ");
                WriteClrType(sb, property.ClrTypeName);
                sb.AppendFormat(
                    " {0} => {1}(this, _publishedValueFallback);\n",
                    property.ClrName, MixinStaticGetterName(property.ClrName));
            }
            else
            {
                sb.Append("\t\tpublic virtual ");
                WriteClrType(sb, property.ClrTypeName);
                sb.AppendFormat(
                    " {0} => this.Value",
                    property.ClrName);
                if (property.ModelClrType != typeof(object))
                {
                    sb.Append("<");
                    WriteClrType(sb, property.ClrTypeName);
                    sb.Append(">");
                }

                sb.AppendFormat(
                    "(_publishedValueFallback, \"{0}\");\n",
                    property.Alias);
            }

            if (property.Errors != null)
            {
                sb.Append("\n");
                sb.Append("\t\t *\n");
                sb.Append("\t\t */\n");
            }

            if (!mixinStatic)
            {
                return;
            }

            var mixinStaticGetterName = MixinStaticGetterName(property.ClrName);

            // if (type.StaticMixinMethods.Contains(mixinStaticGetterName)) return;
            sb.Append("\n");

            if (!string.IsNullOrWhiteSpace(property.Name))
            {
                sb.AppendFormat("\t\t/// <summary>Static getter for {0}</summary>\n", XmlCommentString(property.Name));
            }

            WriteGeneratedCodeAttribute(sb, "\t\t");
            if (!property.ModelClrType.IsValueType)
            {
                WriteMaybeNullAttribute(sb, "\t\t", true);
            }

            sb.Append("\t\tpublic static ");
            WriteClrType(sb, property.ClrTypeName);
            sb.AppendFormat(
                " {0}(I{1} that, IPublishedValueFallback publishedValueFallback) => that.Value",
                mixinStaticGetterName, mixinClrName);
            if (property.ModelClrType != typeof(object))
            {
                sb.Append("<");
                WriteClrType(sb, property.ClrTypeName);
                sb.Append(">");
            }

            sb.AppendFormat(
                "(publishedValueFallback, \"{0}\");\n",
                property.Alias);
        }
        public string XmlCommentString(string input) => input.Replace('<', '{').Replace('>', '}').Replace('\r', ' ').Replace('\n', ' ');

        private static IEnumerable<string> SplitError(string error)
        {
            var p = 0;
            while (p < error.Length)
            {
                var n = p + 50;
                while (n < error.Length && error[n] != ' ')
                {
                    n++;
                }

                if (n >= error.Length)
                {
                    break;
                }

                yield return error.Substring(p, n - p);
                p = n + 1;
            }

            if (p < error.Length)
            {
                yield return error[p..];
            }
        }
        private static string MixinStaticGetterName(string clrName) => string.Format("Get{0}", clrName);
        public void WriteClrType(StringBuilder sb, Type type)
        {
            var s = type.ToString();

            if (type.IsGenericType)
            {
                var p = s.IndexOf('`');
                WriteNonGenericClrType(sb, s.Substring(0, p));
                sb.Append("<");
                Type[] args = type.GetGenericArguments();
                for (var i = 0; i < args.Length; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }

                    WriteClrType(sb, args[i]);
                }

                sb.Append(">");
            }
            else
            {
                WriteNonGenericClrType(sb, s);
            }
        }

        public void WriteNonGenericClrType(StringBuilder sb, string s)
        {
            // map model types
            s = Regex.Replace(s, @"\{(.*)\}\[\*\]", m => ModelsMap[m.Groups[1].Value + "[]"]);

            // takes care eg of "System.Int32" vs. "int"
            if (TextBuilderConstants.TypesMap.TryGetValue(s, out var typeName))
            {
                sb.Append(typeName);
                return;
            }

            // if full type name matches a using clause, strip
            // so if we want Umbraco.Core.Models.IPublishedContent
            // and using Umbraco.Core.Models, then we just need IPublishedContent
            typeName = s;
            string? typeUsing = null;
            var p = typeName.LastIndexOf('.');
            if (p > 0)
            {
                var x = typeName.Substring(0, p);
                if (_builderBase.Using.Contains(x))
                {
                    typeName = typeName.Substring(p + 1);
                    typeUsing = x;
                }
                else if (x == _builderBase.GetModelsNamespace()) // that one is used by default
                {
                    typeName = typeName.Substring(p + 1);
                    typeUsing = _builderBase.GetModelsNamespace();
                }
            }

            // nested types *after* using
            typeName = typeName.Replace("+", ".");

            // symbol to test is the first part of the name
            // so if type name is Foo.Bar.Nil we want to ensure that Foo is not ambiguous
            p = typeName.IndexOf('.');
            var symbol = p > 0 ? typeName.Substring(0, p) : typeName;

            // what we should find - WITHOUT any generic <T> thing - just the type
            // no 'using' = the exact symbol
            // a 'using' = using.symbol
            var match = typeUsing == null ? symbol : typeUsing + "." + symbol;

            // if not ambiguous, be happy
            if (!_builderBase.IsAmbiguousSymbol(symbol, match))
            {
                sb.Append(typeName);
                return;
            }

            // symbol is ambiguous
            // if no 'using', must prepend global::
            if (typeUsing == null)
            {
                sb.Append("global::");
                sb.Append(s.Replace("+", "."));
                return;
            }

            // could fullname be non-ambiguous?
            // note: all-or-nothing, not trying to segment the using clause
            typeName = s.Replace("+", ".");
            p = typeName.IndexOf('.');
            symbol = typeName.Substring(0, p);
            match = symbol;

            // still ambiguous, must prepend global::
            if (_builderBase.IsAmbiguousSymbol(symbol, match))
            {
                sb.Append("global::");
            }

            sb.Append(typeName);
        }

        public void WriteClrType(StringBuilder sb, string type)
        {
            var p = type.IndexOf('<');
            if (type.Contains('<'))
            {
                WriteNonGenericClrType(sb, type[..p]);
                sb.Append("<");
                var args = type[(p + 1)..].TrimEnd(Constants.CharArrays.GreaterThan)
                    .Split(Constants.CharArrays.Comma); // TODO: will NOT work with nested generic types
                for (var i = 0; i < args.Length; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }

                    WriteClrType(sb, args[i]);
                }

                sb.Append(">");
            }
            else
            {
                WriteNonGenericClrType(sb, type);
            }
        }
    }
}
