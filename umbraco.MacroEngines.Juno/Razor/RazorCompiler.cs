using System.CodeDom;

namespace umbraco.MacroEngines.Razor {
    using System;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Razor;
    using System.Web.Razor.Parser;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Compiles razor templates.
    /// </summary>
    internal class RazorCompiler {
        #region Fields
        private readonly IRazorProvider provider;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="RazorCompiler"/>.
        /// </summary>
        /// <param name="provider">The provider used to compile templates.</param>
        public RazorCompiler(IRazorProvider provider) {
            if (provider == null)
                throw new ArgumentNullException("provider");

            this.provider = provider;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Compiles the template.
        /// </summary>
        /// <param name="className">The class name of the dynamic type.</param>
        /// <param name="template">The template to compile.</param>
        /// <param name="modelType">[Optional] The mode type.</param>
        private CompilerResults Compile(string className, string template, Type modelType = null) {
            var languageService = provider.CreateLanguageService();
            var codeDom = provider.CreateCodeDomProvider();
            var host = new RazorEngineHost(languageService);

            var generator = languageService.CreateCodeGenerator(className, "Razor.Dynamic", null, host);
            var parser = new RazorParser(languageService.CreateCodeParser(), new HtmlMarkupParser());

            // Umbraco hack for use with DynamicNode
            bool anonymousType = modelType.FullName == "umbraco.MacroEngines.DynamicNode" || (modelType.IsClass && modelType.IsSealed && modelType.BaseType == typeof(object) && modelType.Name.StartsWith("<>") && modelType.GetCustomAttributes(typeof(CompilerGeneratedAttribute), true) != null);

            //There's no simple way of determining if an object is an anonymous type - this seems like a problem
            Type baseType = (modelType == null)
                ? typeof(TemplateBase)
                : (anonymousType
                    ? typeof(TemplateBaseDynamic)
                    : typeof(TemplateBase<>).MakeGenericType(modelType));

            generator.GeneratedClass.BaseTypes.Add(baseType);

            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(template)))) {
                parser.Parse(reader, generator);
            }

            var statement = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Clear");
            generator.GeneratedExecuteMethod.Statements.Insert(0, new CodeExpressionStatement(statement));

            var builder = new StringBuilder();
            using (var writer = new StringWriter(builder)) {
                codeDom.GenerateCodeFromCompileUnit(generator.GeneratedCode, writer, new CodeGeneratorOptions());
            }

            var @params = new CompilerParameters();
//            @params.ReferencedAssemblies.Add("System");
//            @params.ReferencedAssemblies.Add("System.Web");
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.IsDynamic)
                    @params.ReferencedAssemblies.Add(assembly.Location);
            }

            @params.GenerateInMemory = true;
            @params.IncludeDebugInformation = false;
            @params.GenerateExecutable = false;
            @params.CompilerOptions = "/target:library /optimize";

            var result = codeDom.CompileAssemblyFromSource(@params, new[] { builder.ToString() });
            return result;
        }

        /// <summary>
        /// Creates a <see cref="ITemplate" /> from the specified template string.
        /// </summary>
        /// <param name="template">The template to compile.</param>
        /// <param name="modelType">[Optional] The model type.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        public ITemplate CreateTemplate(string template, Type modelType = null) {
            string className = Regex.Replace(Guid.NewGuid().ToString("N"), @"[^A-Za-z]*", "");

            var result = Compile(className, template, modelType);

            if (result.Errors != null && result.Errors.Count > 0)
                throw new TemplateException(result.Errors);

            ITemplate instance = (ITemplate)result.CompiledAssembly.CreateInstance("Razor.Dynamic." + className);

            return instance;
        }
        #endregion
    }
}