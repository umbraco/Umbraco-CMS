using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;

namespace Umbraco.ModelsBuilder.Embedded
{
    internal class NonRecursivePhysicalFileProvider : PhysicalFileProvider, IFileProvider
    {
        private static readonly char[] s_pathSeparators = new char[2]
        {
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar
        };

        public NonRecursivePhysicalFileProvider(string root)
            : base(root)
        {
        }

        IDirectoryContents IFileProvider.GetDirectoryContents(string subpath) => IsRoot(subpath) ? GetDirectoryContents(subpath) : null;

        IFileInfo IFileProvider.GetFileInfo(string subpath) => IsRoot(subpath) ? GetFileInfo(subpath) : null;

        IChangeToken IFileProvider.Watch(string filter) => IsRoot(filter) ? Watch(filter) : NullChangeToken.Singleton;

        private bool IsRoot(string path) => !s_pathSeparators.Any(x => path.Contains(x));
    }

    public class ModelsBuilderRazorRuntimeCompilationOptions : IConfigureOptions<MvcRazorRuntimeCompilationOptions>
    {
        private readonly ModelsBuilderSettings _config;
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelsBuilderRazorRuntimeCompilationOptions"/> class.
        /// </summary>
        public ModelsBuilderRazorRuntimeCompilationOptions(
            IOptions<ModelsBuilderSettings> config,
            IHostingEnvironment hostingEnvironment)
        {
            _config = config.Value;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <inheritdoc/>
        public void Configure(MvcRazorRuntimeCompilationOptions options)
        {
            //RazorProjectEngine.Create()

            // TODO: Not sure this is going to be possible :/
            // See https://stackoverflow.com/questions/58685966/adding-assemblies-types-to-be-made-available-to-razor-page-at-runtime
            // See https://github.com/dotnet/aspnetcore/blob/master/src/Mvc/Mvc.Razor.RuntimeCompilation/src/MvcRazorRuntimeCompilationOptions.cs
            // See https://github.com/dotnet/aspnetcore/blob/b795ac3546eb3e2f47a01a64feb3020794ca33bb/src/Mvc/Mvc.Razor.RuntimeCompilation/src/RazorReferenceManager.cs
            // See https://github.com/dotnet/aspnetcore/blob/114f0f6d1ef1d777fb93d90c87ac506027c55ea0/src/Mvc/Mvc.Razor.RuntimeCompilation/src/RuntimeViewCompiler.cs#L26

            // This is where the RazorProjectEngine gets created
            // https://github.com/dotnet/aspnetcore/blob/336e05577cd8bec2000ffcada926189199e4cef0/src/Mvc/Mvc.Razor.RuntimeCompilation/src/DependencyInjection/RazorRuntimeCompilationMvcCoreBuilderExtensions.cs#L86
            // In theory, it seems like 

            //MetadataReference ref;
            //var modelsDirectory = _config.ModelsDirectoryAbsolute(_hostingEnvironment);

            // From what I can tell, we can specify a file provider here for all razor files
            // that need to be watched which will recompiled when they are changed.

            // TODO: Should be constants and or shared with our RazorViewEngineOptions classes
            options.FileProviders.Add(new NonRecursivePhysicalFileProvider(_hostingEnvironment.MapPathContentRoot("~/Views")));
            options.FileProviders.Add(new PhysicalFileProvider(_hostingEnvironment.MapPathContentRoot("~/Views/Partials")));
            options.FileProviders.Add(new PhysicalFileProvider(_hostingEnvironment.MapPathContentRoot("~/Views/MacroPartials")));
            options.FileProviders.Add(new PhysicalFileProvider(_hostingEnvironment.MapPathContentRoot("~/App_Plugins")));
        }
    }
}
