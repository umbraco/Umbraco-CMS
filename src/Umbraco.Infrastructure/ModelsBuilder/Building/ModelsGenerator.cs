using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Building;

public class ModelsGenerator : IModelsGenerator
{
    private readonly OutOfDateModelsStatus _outOfDateModels;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly UmbracoServices _umbracoService;
    private ModelsBuilderSettings _config;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModelsGenerator" /> class.
    /// </summary>
    /// <param name="umbracoService">The Umbraco services.</param>
    /// <param name="config">The models builder configuration.</param>
    /// <param name="outOfDateModels">The out of date models status.</param>
    /// <param name="hostEnvironment">The host environment.</param>
    public ModelsGenerator(
        UmbracoServices umbracoService,
        IOptionsMonitor<ModelsBuilderSettings> config,
        OutOfDateModelsStatus outOfDateModels,
        IHostEnvironment hostEnvironment)
    {
        _umbracoService = umbracoService;
        _config = config.CurrentValue;
        _outOfDateModels = outOfDateModels;
        _hostEnvironment = hostEnvironment;
        config.OnChange(x => _config = x);
    }

    public void GenerateModels()
    {
        var modelsDirectory = _config.ModelsDirectoryAbsolute(_hostEnvironment);
        if (!Directory.Exists(modelsDirectory))
        {
            Directory.CreateDirectory(modelsDirectory);
        }

        IList<TypeModel> typeModels = _umbracoService.GetAllTypes();

        var builder = new TextBuilder(_config, typeModels);

        var generatedFiles = new List<string>();
        foreach (TypeModel typeModel in builder.GetModelsToGenerate())
        {
            var sb = new StringBuilder();
            builder.Generate(sb, typeModel);
            var filename = Path.Combine(modelsDirectory, typeModel.ClrName + ".generated.cs");
            generatedFiles.Add(filename);

            var code = sb.ToString();

            // leave the file alone if its contents is identical to the generated model
            if (File.Exists(filename) && File.ReadAllText(filename).Equals(code))
            {
                continue;
            }

            // overwrite the file
            File.WriteAllText(filename, code);
        }

        // clean up old/leftover generated files
        foreach (var file in Directory.GetFiles(modelsDirectory, "*.generated.cs"))
        {
            if (generatedFiles.InvariantContains(file) is false)
            {
                File.Delete(file);
            }
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
