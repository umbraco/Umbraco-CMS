﻿using System.IO;
using System.Text;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;

namespace Umbraco.ModelsBuilder.Embedded.Building
{
    public class ModelsGenerator
    {
        private readonly UmbracoServices _umbracoService;
        private readonly IModelsBuilderConfig _config;
        private readonly OutOfDateModelsStatus _outOfDateModels;
        private readonly IHostingEnvironment _hostingEnvironment;

        public ModelsGenerator(UmbracoServices umbracoService, IModelsBuilderConfig config, OutOfDateModelsStatus outOfDateModels, IHostingEnvironment hostingEnvironment)
        {
            _umbracoService = umbracoService;
            _config = config;
            _outOfDateModels = outOfDateModels;
            _hostingEnvironment = hostingEnvironment;
        }

        internal void GenerateModels()
        {
            var modelsDirectory = _config.ModelsDirectoryAbsolute(_hostingEnvironment);
            if (!Directory.Exists(modelsDirectory))
                Directory.CreateDirectory(modelsDirectory);

            foreach (var file in Directory.GetFiles(modelsDirectory, "*.generated.cs"))
                File.Delete(file);

            var typeModels = _umbracoService.GetAllTypes();

            var builder = new TextBuilder(_config, typeModels);

            foreach (var typeModel in builder.GetModelsToGenerate())
            {
                var sb = new StringBuilder();
                builder.Generate(sb, typeModel);
                var filename = Path.Combine(modelsDirectory, typeModel.ClrName + ".generated.cs");
                File.WriteAllText(filename, sb.ToString());
            }

            // the idea was to calculate the current hash and to add it as an extra file to the compilation,
            // in order to be able to detect whether a DLL is consistent with an environment - however the
            // environment *might not* contain the local partial files, and thus it could be impossible to
            // calculate the hash. So... maybe that's not a good idea after all?
            /*
            var currentHash = HashHelper.Hash(ourFiles, typeModels);
            ourFiles["models.hash.cs"] = $@"using Umbraco.ModelsBuilder;
[assembly:ModelsBuilderAssembly(SourceHash = ""{currentHash}"")]
";
            */

            _outOfDateModels.Clear();
        }
    }
}
