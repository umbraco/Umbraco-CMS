using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Umbraco.Core.Configuration;
using Umbraco.ModelsBuilder.Configuration;

namespace Umbraco.ModelsBuilder.Building
{
    // main Roslyn compiler
    internal class Compiler
    {
        private readonly LanguageVersion _languageVersion;

        public Compiler()
            : this(UmbracoConfig.For.ModelsBuilder().LanguageVersion)
        { }

        public Compiler(LanguageVersion languageVersion)
        {
            _languageVersion = languageVersion;
            References = ReferencedAssemblies.References;
            Debug = HttpContext.Current != null && HttpContext.Current.IsDebuggingEnabled;
        }

        // gets or sets the references
        public IEnumerable<PortableExecutableReference> References { get; set; }

        public bool Debug { get; set; }

        // gets a compilation
        public CSharpCompilation GetCompilation(string assemblyName, IDictionary<string, string> files)
        {
            SyntaxTree[] trees;
            return GetCompilation(assemblyName, files, out trees);
        }

        // gets a compilation
        // used by CodeParser to get a "compilation" of the existing files
        public CSharpCompilation GetCompilation(string assemblyName, IDictionary<string, string> files, out SyntaxTree[] trees)
        {
            var options = new CSharpParseOptions(_languageVersion);
            trees = files.Select(x =>
            {
                var text = x.Value;
                var tree = CSharpSyntaxTree.ParseText(text, /*options:*/ options);
                var diagnostic = tree.GetDiagnostics().FirstOrDefault(y => y.Severity == DiagnosticSeverity.Error);
                if (diagnostic != null)
                    ThrowExceptionFromDiagnostic(x.Key, x.Value, diagnostic);
                return tree;
            }).ToArray();

            var refs = References;

            var compilationOptions = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default,
                optimizationLevel: Debug ? OptimizationLevel.Debug : OptimizationLevel.Release
            );
            var compilation = CSharpCompilation.Create(
                assemblyName,
                /*syntaxTrees:*/ trees,
                /*references:*/ refs,
                compilationOptions);

            return compilation;
        }

        // compile files into a Dll
        // used by ModelsBuilderBackOfficeController in [Live]Dll mode, to compile the models to disk
        public void Compile(string assemblyName, IDictionary<string, string> files, string binPath)
        {
            var assemblyPath = Path.Combine(binPath, assemblyName + ".dll");
            using (var stream = new FileStream(assemblyPath, FileMode.Create))
            {
                Compile(assemblyName, files, stream);
            }

            // this is how we'd create the pdb:
            /*
            var pdbPath = Path.Combine(binPath, assemblyName + ".pdb");

            // create the compilation
            var compilation = GetCompilation(assemblyName, files);

            // check diagnostics for errors (not warnings)
            foreach (var diag in compilation.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error))
                ThrowExceptionFromDiagnostic(files, diag);

            // emit
            var result = compilation.Emit(assemblyPath, pdbPath);
            if (result.Success) return;

            // deal with errors
            var diagnostic = result.Diagnostics.First(x => x.Severity == DiagnosticSeverity.Error);
            ThrowExceptionFromDiagnostic(files, diagnostic);
            */
        }

        // compile files into an assembly
        public Assembly Compile(string assemblyName, IDictionary<string, string> files)
        {
            using (var stream = new MemoryStream())
            {
                Compile(assemblyName, files, stream);
                return Assembly.Load(stream.GetBuffer());
            }
        }

        // compile one file into an assembly
        public Assembly Compile(string assemblyName, string path, string code)
        {
            using (var stream = new MemoryStream())
            {
                Compile(assemblyName, new Dictionary<string, string> { { path, code } }, stream);
               return Assembly.Load(stream.GetBuffer());
            }
        }

        // compiles files into a stream
        public void Compile(string assemblyName, IDictionary<string, string> files, Stream stream)
        {
            // create the compilation
            var compilation = GetCompilation(assemblyName, files);

            // check diagnostics for errors (not warnings)
            foreach (var diag in compilation.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error))
                ThrowExceptionFromDiagnostic(files, diag);

            // emit
            var result = compilation.Emit(stream);
            if (result.Success) return;

            // deal with errors
            var diagnostic = result.Diagnostics.First(x => x.Severity == DiagnosticSeverity.Error);
            ThrowExceptionFromDiagnostic(files, diagnostic);
        }

        // compiles one file into a stream
        public void Compile(string assemblyName, string path, string code, Stream stream)
        {
            Compile(assemblyName, new Dictionary<string, string> { { path, code } }, stream);
        }

        private static void ThrowExceptionFromDiagnostic(IDictionary<string, string> files, Diagnostic diagnostic)
        {
            var message = diagnostic.GetMessage();
            if (diagnostic.Location == Location.None)
                throw new CompilerException(message);

            var position = diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1;
            var path = diagnostic.Location.SourceTree.FilePath;
            var code = files.ContainsKey(path) ? files[path] : string.Empty;
            throw new CompilerException(message, path, code, position);
        }

        private static void ThrowExceptionFromDiagnostic(string path, string code, Diagnostic diagnostic)
        {
            var message = diagnostic.GetMessage();
            if (diagnostic.Location == Location.None)
                throw new CompilerException(message);

            var position = diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1;
            throw new CompilerException(message, path, code, position);
        }
    }
}
