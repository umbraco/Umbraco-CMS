using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Umbraco.ModelsBuilder.Embedded
{
    public class RoslynCompiler
    {
        private OutputKind _outputKind;
        private CSharpParseOptions _parseOptions;

        public RoslynCompiler()
        {
            _outputKind = OutputKind.DynamicallyLinkedLibrary;
            _parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);  // What languageversion should we default to?
        }

        public Assembly GetCompiledAssembly(string pathToSourceFile, IEnumerable<MetadataReference> refs)
        {
            // TODO: Get proper temp file location/filename
            var outputPath = $"generated.cs.{Guid.NewGuid()}.dll";
            var sourceCode = File.ReadAllText(pathToSourceFile);

            CompileToFile(outputPath, sourceCode, "ModelsGenerated", refs);
            return Assembly.LoadFile(outputPath);

        } 

        private void CompileToFile(string outputFile, string sourceCode, string assemblyName, IEnumerable<MetadataReference> references)
        {
            var sourceText = SourceText.From(sourceCode);

            var syntaxTree = SyntaxFactory.ParseSyntaxTree(sourceText, _parseOptions);

            var compilation = CSharpCompilation.Create(assemblyName,
                new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(_outputKind,
                optimizationLevel: OptimizationLevel.Release,
                // Not entirely certain that assemblyIdentityComparer is nececary? 
                assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));

            compilation.Emit(outputFile);
        }
    }
}
