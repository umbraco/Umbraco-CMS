using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Umbraco.ModelsBuilder.Embedded
{
    public class RoslynCompiler
    {
        private OutputKind _outputKind;
        private CSharpParseOptions _parseOptions;
        private List<MetadataReference> _refs;

        public RoslynCompiler(IEnumerable<Assembly> referenceAssemblies)
        {
            _outputKind = OutputKind.DynamicallyLinkedLibrary;
            _parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);  // What languageversion should we default to?

            // The references should be the same every time GetCompiledAssembly is called
            // Making it kind of a waste to convert the Assembly types into MetadataReference
            // every time GetCompiledAssembly is called, so that's why I do it in the ctor
            _refs = new List<MetadataReference>();
            foreach(var assembly in referenceAssemblies.Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location)).Distinct())
            {
                _refs.Add(MetadataReference.CreateFromFile(assembly.Location));
            };

            // Might have to do this another way, see
            // see https://github.com/aspnet/RoslynCodeDomProvider/blob/master/src/Microsoft.CodeDom.Providers.DotNetCompilerPlatform/CSharpCompiler.cs:
            // mentions "Bug 913691: Explicitly add System.Runtime as a reference."
            // and explicitly adds System.Runtime to references
            _refs.Add(MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location));
            _refs.Add(MetadataReference.CreateFromFile(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute).Assembly.Location));
        }

        public void CompileToFile(string pathToSourceFile, string saveLocation)
        {
            var sourceCode = File.ReadAllText(pathToSourceFile);

            var sourceText = SourceText.From(sourceCode);

            var syntaxTree = SyntaxFactory.ParseSyntaxTree(sourceText, _parseOptions);

            var compilation = CSharpCompilation.Create("ModelsGeneratedAssembly",
                new[] { syntaxTree },
                references: _refs,
                options: new CSharpCompilationOptions(_outputKind,
                optimizationLevel: OptimizationLevel.Release,
                // Not entirely certain that assemblyIdentityComparer is nececary? 
                assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));

            compilation.Emit(saveLocation);

        }
    }
}
