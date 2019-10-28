using System;
using System.IO;
using System.Text;
using System.Web;
using Umbraco.ModelsBuilder.Building;
using Umbraco.ModelsBuilder.Configuration;
using Umbraco.ModelsBuilder.Umbraco;

namespace Umbraco.ModelsBuilder.Umbraco
{
    public class ModelsGenerator
    {
        private readonly UmbracoServices _umbracoService;
        private readonly IModelsBuilderConfig _config;
        private readonly ModelsGenerationError _errors;

        public ModelsGenerator(UmbracoServices umbracoService, IModelsBuilderConfig config)
        {
            _umbracoService = umbracoService;
            _config = config;
            _errors = new ModelsGenerationError(config);
        }

        internal void GenerateModels()
        {
            if (!Directory.Exists(_config.ModelsDirectory))
                Directory.CreateDirectory(_config.ModelsDirectory);

            foreach (var file in Directory.GetFiles(_config.ModelsDirectory, "*.generated.cs"))
                File.Delete(file);

            var typeModels = _umbracoService.GetAllTypes();

            var builder = new TextBuilder(_config, typeModels);

            foreach (var typeModel in builder.GetModelsToGenerate())
            {
                var sb = new StringBuilder();
                builder.Generate(sb, typeModel);
                var filename = Path.Combine(_config.ModelsDirectory, typeModel.ClrName + ".generated.cs");
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

            OutOfDateModelsStatus.Clear();
        }

        internal void ClearErrors() => _errors.Clear();
        internal void ReportError(string message, Exception e) => _errors.Report(message, e);
        internal string GetLastError() => _errors.GetLastError();

    }
}
