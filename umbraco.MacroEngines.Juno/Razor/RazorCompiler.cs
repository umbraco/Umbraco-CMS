using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace umbraco.MacroEngines.Razor
{
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
    internal class RazorCompiler
    {
        #region Fields
        private readonly IRazorProvider provider;
        private Type templateBaseType;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="RazorCompiler"/>.
        /// </summary>
        /// <param name="provider">The provider used to compile templates.</param>
        public RazorCompiler(IRazorProvider provider)
        {
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
        private CompilerResults Compile(string className, string template, Type modelType = null)
        {
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

            templateBaseType = baseType;
            generator.GeneratedClass.BaseTypes.Add(baseType);

            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(template))))
            {
                parser.Parse(reader, generator);
            }

            var statement = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Clear");
            generator.GeneratedExecuteMethod.Statements.Insert(0, new CodeExpressionStatement(statement));

            var builder = new StringBuilder();
            using (var writer = new StringWriter(builder))
            {
                codeDom.GenerateCodeFromCompileUnit(generator.GeneratedCode, writer, new CodeGeneratorOptions());
            }

            var parameters = new CompilerParameters();
            AddReferences(parameters);

            parameters.GenerateInMemory = true;
            parameters.IncludeDebugInformation = false;
            parameters.GenerateExecutable = false;
            parameters.CompilerOptions = "/target:library /optimize";

            var result = codeDom.CompileAssemblyFromSource(parameters, new[] { builder.ToString() });
            return result;
        }

        /// <summary>
        /// Creates a <see cref="ITemplate" /> from the specified template string.
        /// </summary>
        /// <param name="template">The template to compile.</param>
        /// <param name="modelType">[Optional] The model type.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        public ITemplate CreateTemplate(string template, Type modelType = null)
        {
            string className = Regex.Replace(Guid.NewGuid().ToString("N"), @"[^A-Za-z]*", "");

            var result = Compile(className, template, modelType);

            if (result.Errors != null && result.Errors.Count > 0)
                throw new TemplateException(result.Errors);

            ITemplate instance = (ITemplate)result.CompiledAssembly.CreateInstance("Razor.Dynamic." + className);

            return instance;
        }
        #endregion

        /// <summary>
        /// Adds any required references to the compiler parameters.
        /// </summary>
        /// <param name="parameters">The compiler parameters.</param>
        private void AddReferences(CompilerParameters parameters)
        {
            var list = new List<string>();
            IEnumerable<string> coreRefs = GetCoreReferences();
            foreach (string location in coreRefs)
            {
                list.Add(location.ToLowerInvariant());
            }

            IEnumerable<string> baseRefs = GetBaseTypeReferencedAssemblies();
            foreach (string location in baseRefs)
            {
                list.Add(location.ToLowerInvariant());
            }

            foreach (string location in list)
                System.Diagnostics.Debug.Print(location);
            IEnumerable<string> distinctList = list.Distinct(new AssemblyVersionComparer());
            parameters.ReferencedAssemblies.AddRange(distinctList.ToArray());
        }

        /// <summary>
        /// Gets the locations of assemblies referenced by a custom base template type.
        /// </summary>
        /// <returns>An enumerable of reference assembly locations.</returns>
        private IEnumerable<string> GetBaseTypeReferencedAssemblies()
        {
            if (templateBaseType == null)
                return new string[0];

            return templateBaseType.Assembly
                .GetReferencedAssemblies()
                .Select(n => Assembly.ReflectionOnlyLoad(n.FullName).Location);
        }


        /// <summary>
        /// Gets the locations of all core referenced assemblies.
        /// </summary>
        /// <returns>An enumerable of reference assembly locations.</returns>
        private static IEnumerable<string> GetCoreReferences()
        {
            var refs = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).Select(a => a.Location);

            return refs.Concat(typeof(RazorCompiler)
                 .Assembly
                 .GetReferencedAssemblies().Select(n => Assembly.ReflectionOnlyLoad(n.FullName).Location));
        }

    }

    public class AssemblyVersionComparer : IEqualityComparer<string>
    {
        bool IEqualityComparer<string>.Equals(string x, string y)
        {
            x = findAssemblyName(x);
            y = findAssemblyName(y);
            return (x.Contains(y) || y.Contains(x));
        } 
        
        int IEqualityComparer<string>.GetHashCode(string obj)
        {
            // 1) find the assembly name without version number and path (xxx.yyy.dll)
            obj = findAssemblyName(obj);
            // 2) send det som hashcode
            if (Object.ReferenceEquals(obj, null))             
                return 0; 
            return obj.GetHashCode();
        }

        private string findAssemblyName(string fullAssemblyPath)
        {

            Regex r = new Regex(@"\\([^\\]*.dll)"); 
            Match m = r.Match(fullAssemblyPath); 
            if (m.Groups.Count > 0)
            {
                fullAssemblyPath = m.Groups[0].Value;
            }
            return fullAssemblyPath;
        }
    }

}