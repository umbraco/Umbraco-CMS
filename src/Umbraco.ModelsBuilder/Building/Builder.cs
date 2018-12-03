using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Umbraco.Core.Configuration;
using Umbraco.ModelsBuilder.Configuration;

namespace Umbraco.ModelsBuilder.Building
{
    // NOTE
    // The idea was to have different types of builder, because I wanted to experiment with
    // building code with CodeDom. Turns out more complicated than I thought and maybe not
    // worth it at the moment, to we're using TextBuilder and its Generate method is specific.
    //
    // Keeping the code as-is for the time being...

    /// <summary>
    /// Provides a base class for all builders.
    /// </summary>
    internal abstract class Builder
    {
        private readonly IList<TypeModel> _typeModels;

        protected Dictionary<string, string> ModelsMap { get; } = new Dictionary<string, string>();
        protected ParseResult ParseResult { get; }

        // the list of assemblies that will be 'using' by default
        protected readonly IList<string> TypesUsing = new List<string>
        {
            "System",
            "System.Collections.Generic",
            "System.Linq.Expressions",
            "System.Web",
            "Umbraco.Core.Models",
            "Umbraco.Core.Models.PublishedContent",
            "Umbraco.Web",
            "Umbraco.ModelsBuilder",
            "Umbraco.ModelsBuilder.Umbraco",
        };

        /// <summary>
        /// Gets or sets a value indicating the namespace to use for the models.
        /// </summary>
        /// <remarks>May be overriden by code attributes.</remarks>
        public string ModelsNamespace { get; set; }

        /// <summary>
        /// Gets the list of assemblies to add to the set of 'using' assemblies in each model file.
        /// </summary>
        public IList<string> Using => TypesUsing;

        /// <summary>
        /// Gets the list of models to generate.
        /// </summary>
        /// <returns>The models to generate, ie those that are not ignored.</returns>
        public IEnumerable<TypeModel> GetModelsToGenerate()
        {
            return _typeModels.Where(x => !x.IsContentIgnored);
        }

        /// <summary>
        /// Gets the list of all models.
        /// </summary>
        /// <remarks>Includes those that are ignored.</remarks>
        internal IList<TypeModel> TypeModels => _typeModels;

        /// <summary>
        /// Initializes a new instance of the <see cref="Builder"/> class with a list of models to generate
        /// and the result of code parsing.
        /// </summary>
        /// <param name="typeModels">The list of models to generate.</param>
        /// <param name="parseResult">The result of code parsing.</param>
        protected Builder(IList<TypeModel> typeModels, ParseResult parseResult)
        {
            _typeModels = typeModels ?? throw new ArgumentNullException(nameof(typeModels));
            ParseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));

            Prepare();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Builder"/> class with a list of models to generate,
        /// the result of code parsing, and a models namespace.
        /// </summary>
        /// <param name="typeModels">The list of models to generate.</param>
        /// <param name="parseResult">The result of code parsing.</param>
        /// <param name="modelsNamespace">The models namespace.</param>
        protected Builder(IList<TypeModel> typeModels, ParseResult parseResult, string modelsNamespace)
            : this(typeModels, parseResult)
        {
            // can be null or empty, we'll manage
            ModelsNamespace = modelsNamespace;
        }

        // for unit tests only
        protected Builder()
        { }

        /// <summary>
        /// Prepares generation by processing the result of code parsing.
        /// </summary>
        /// <remarks>
        ///     Preparation includes figuring out from the existing code which models or properties should
        ///     be ignored or renamed, etc. -- anything that comes from the attributes in the existing code.
        /// </remarks>
        private void Prepare()
        {
            var pureLive = UmbracoConfig.For.ModelsBuilder().ModelsMode == ModelsMode.PureLive;

            // mark IsContentIgnored models that we discovered should be ignored
            // then propagate / ignore children of ignored contents
            // ignore content = don't generate a class for it, don't generate children
            foreach (var typeModel in _typeModels.Where(x => ParseResult.IsIgnored(x.Alias)))
                typeModel.IsContentIgnored = true;
            foreach (var typeModel in _typeModels.Where(x => !x.IsContentIgnored && x.EnumerateBaseTypes().Any(xx => xx.IsContentIgnored)))
                typeModel.IsContentIgnored = true;

            // handle model renames
            foreach (var typeModel in _typeModels.Where(x => ParseResult.IsContentRenamed(x.Alias)))
            {
                typeModel.ClrName = ParseResult.ContentClrName(typeModel.Alias);
                typeModel.IsRenamed = true;
                ModelsMap[typeModel.Alias] = typeModel.ClrName;
            }

            // handle implement
            foreach (var typeModel in _typeModels.Where(x => ParseResult.HasContentImplement(x.Alias)))
            {
                typeModel.HasImplement = true;
            }

            // mark OmitBase models that we discovered already have a base class
            foreach (var typeModel in _typeModels.Where(x => ParseResult.HasContentBase(ParseResult.ContentClrName(x.Alias) ?? x.ClrName)))
                typeModel.HasBase = true;

            foreach (var typeModel in _typeModels)
            {
                // mark IsRemoved properties that we discovered should be ignored
                // ie is marked as ignored on type, or on any parent type
                var tm = typeModel;
                foreach (var property in typeModel.Properties
                    .Where(property => tm.EnumerateBaseTypes(true).Any(x => ParseResult.IsPropertyIgnored(ParseResult.ContentClrName(x.Alias) ?? x.ClrName, property.Alias))))
                {
                    property.IsIgnored = true;
                }

                // handle property renames
                foreach (var property in typeModel.Properties)
                    property.ClrName = ParseResult.PropertyClrName(ParseResult.ContentClrName(typeModel.Alias) ?? typeModel.ClrName, property.Alias) ?? property.ClrName;
            }

            // for the first two of these two tests,
            //  always throw, even in purelive: cannot happen unless ppl start fidling with attributes to rename
            //  things, and then they should pay attention to the generation error log - there's no magic here
            // for the last one, don't throw in purelive, see comment

            // ensure we have no duplicates type names
            foreach (var xx in _typeModels.Where(x => !x.IsContentIgnored).GroupBy(x => x.ClrName).Where(x => x.Count() > 1))
                throw new InvalidOperationException($"Type name \"{xx.Key}\" is used"
                    + $" for types with alias {string.Join(", ", xx.Select(x => x.ItemType + ":\"" + x.Alias + "\""))}. Names have to be unique."
                    + " Consider using an attribute to assign different names to conflicting types.");

            // ensure we have no duplicates property names
            foreach (var typeModel in _typeModels.Where(x => !x.IsContentIgnored))
                foreach (var xx in typeModel.Properties.Where(x => !x.IsIgnored).GroupBy(x => x.ClrName).Where(x => x.Count() > 1))
                    throw new InvalidOperationException($"Property name \"{xx.Key}\" in type {typeModel.ItemType}:\"{typeModel.Alias}\""
                        + $" is used for properties with alias {string.Join(", ", xx.Select(x => "\"" + x.Alias + "\""))}. Names have to be unique."
                        + " Consider using an attribute to assign different names to conflicting properties.");

            // ensure content & property type don't have identical name (csharp hates it)
            foreach (var typeModel in _typeModels.Where(x => !x.IsContentIgnored))
            {
                foreach (var xx in typeModel.Properties.Where(x => !x.IsIgnored && x.ClrName == typeModel.ClrName))
                {
                    if (!pureLive)
                        throw new InvalidOperationException($"The model class for content type with alias \"{typeModel.Alias}\" is named \"{xx.ClrName}\"."
                            + $" CSharp does not support using the same name for the property with alias \"{xx.Alias}\"."
                            + " Consider using an attribute to assign a different name to the property.");

                    // for purelive, will we generate a commented out properties with an error message,
                    // instead of throwing, because then it kills the sites and ppl don't understand why
                    xx.AddError($"The class {typeModel.ClrName} cannot implement this property, because"
                        + $" CSharp does not support naming the property with alias \"{xx.Alias}\" with the same name as content type with alias \"{typeModel.Alias}\"."
                        + " Consider using an attribute to assign a different name to the property.");

                    // will not be implemented on interface nor class
                    // note: we will still create the static getter, and implement the property on other classes...
                }
            }

            // ensure we have no collision between base types
            // NO: we may want to define a base class in a partial, on a model that has a parent
            // we are NOT checking that the defined base type does maintain the inheritance chain
            //foreach (var xx in _typeModels.Where(x => !x.IsContentIgnored).Where(x => x.BaseType != null && x.HasBase))
            //    throw new InvalidOperationException(string.Format("Type alias \"{0}\" has more than one parent class.",
            //        xx.Alias));

            // discover interfaces that need to be declared / implemented
            foreach (var typeModel in _typeModels)
            {
                // collect all the (non-removed) types implemented at parent level
                // ie the parent content types and the mixins content types, recursively
                var parentImplems = new List<TypeModel>();
                if (typeModel.BaseType != null && !typeModel.BaseType.IsContentIgnored)
                    TypeModel.CollectImplems(parentImplems, typeModel.BaseType);

                // interfaces we must declare we implement (initially empty)
                // ie this type's mixins, except those that have been removed,
                // and except those that are already declared at the parent level
                // in other words, DeclaringInterfaces is "local mixins"
                var declaring = typeModel.MixinTypes
                    .Where(x => !x.IsContentIgnored)
                    .Except(parentImplems);
                typeModel.DeclaringInterfaces.AddRange(declaring);

                // interfaces we must actually implement (initially empty)
                // if we declare we implement a mixin interface, we must actually implement
                // its properties, all recursively (ie if the mixin interface implements...)
                // so, starting with local mixins, we collect all the (non-removed) types above them
                var mixinImplems = new List<TypeModel>();
                foreach (var i in typeModel.DeclaringInterfaces)
                    TypeModel.CollectImplems(mixinImplems, i);
                // and then we remove from that list anything that is already declared at the parent level
                typeModel.ImplementingInterfaces.AddRange(mixinImplems.Except(parentImplems));
            }

            // register using types
            foreach (var usingNamespace in ParseResult.UsingNamespaces)
            {
                if (!TypesUsing.Contains(usingNamespace))
                    TypesUsing.Add(usingNamespace);
            }

            // discover static mixin methods
            foreach (var typeModel in _typeModels)
                typeModel.StaticMixinMethods.AddRange(ParseResult.StaticMixinMethods(typeModel.ClrName));

            // handle ctor
            foreach (var typeModel in _typeModels.Where(x => ParseResult.HasCtor(x.ClrName)))
                typeModel.HasCtor = true;
        }

        private SemanticModel _ambiguousSymbolsModel;
        private int _ambiguousSymbolsPos;

        // internal for tests
        internal void PrepareAmbiguousSymbols()
        {
            var codeBuilder = new StringBuilder();
            foreach (var t in TypesUsing)
                codeBuilder.AppendFormat("using {0};\n", t);

            codeBuilder.AppendFormat("namespace {0}\n{{ }}\n", GetModelsNamespace());

            var compiler = new Compiler();
            SyntaxTree[] trees;
            var compilation = compiler.GetCompilation("MyCompilation", new Dictionary<string, string> { { "code", codeBuilder.ToString() } }, out trees);
            var tree = trees[0];
            _ambiguousSymbolsModel = compilation.GetSemanticModel(tree);

            var namespaceSyntax = tree.GetRoot().DescendantNodes().OfType<NamespaceDeclarationSyntax>().First();
            //var namespaceSymbol = model.GetDeclaredSymbol(namespaceSyntax);
            _ambiguousSymbolsPos = namespaceSyntax.OpenBraceToken.SpanStart;
        }

        // looking for a simple symbol eg 'Umbraco' or 'String'
        // expecting to match eg 'Umbraco' or 'System.String'
        // returns true if either
        // - more than 1 symbol is found (explicitely ambiguous)
        // - 1 symbol is found BUT not matching (implicitely ambiguous)
        protected bool IsAmbiguousSymbol(string symbol, string match)
        {
            if (_ambiguousSymbolsModel == null)
                PrepareAmbiguousSymbols();
            if (_ambiguousSymbolsModel == null)
                throw new Exception("Could not prepare ambiguous symbols.");
            var symbols = _ambiguousSymbolsModel.LookupNamespacesAndTypes(_ambiguousSymbolsPos, null, symbol);

            if (symbols.Length > 1) return true;
            if (symbols.Length == 0) return false; // what else?

            // only 1 - ensure it matches
            var found = symbols[0].ToDisplayString();
            var pos = found.IndexOf('<'); // generic?
            if (pos > 0) found = found.Substring(0, pos); // strip
            return found != match; // and compare
        }

        internal string ModelsNamespaceForTests;

        public string GetModelsNamespace()
        {
            if (ModelsNamespaceForTests != null)
                return ModelsNamespaceForTests;

            // code attribute overrides everything
            if (ParseResult.HasModelsNamespace)
                return ParseResult.ModelsNamespace;

            // if builder was initialized with a namespace, use that one
            if (!string.IsNullOrWhiteSpace(ModelsNamespace))
                return ModelsNamespace;

            // default
            // fixme - should NOT reference config here, should make ModelsNamespace mandatory
            return UmbracoConfig.For.ModelsBuilder().ModelsNamespace;
        }

        protected string GetModelsBaseClassName(TypeModel type)
        {
            // code attribute overrides everything
            if (ParseResult.HasModelsBaseClassName)
                return ParseResult.ModelsBaseClassName;

            // default
            return type.IsElement ? "PublishedElementModel" : "PublishedContentModel";
        }
    }
}
