using System.Text;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Infrastructure.ModelsBuilder.Building.Interfaces;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Building;

public class ModelsGenerator : IModelsGenerator
{
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ITextBuilder _textBuilder;
    private readonly IBuilderBase _builderBase;
    private readonly OutOfDateModelsStatus _outOfDateModels;
    private readonly UmbracoServices _umbracoService;
    private ModelsBuilderSettings _config;
    

    public ModelsGenerator(UmbracoServices umbracoService, IOptionsMonitor<ModelsBuilderSettings> config,
        OutOfDateModelsStatus outOfDateModels, IHostingEnvironment hostingEnvironment, ITextBuilder textBuilder, IBuilderBase builderBase)
    {
        _umbracoService = umbracoService;
        _config = config.CurrentValue;
        _outOfDateModels = outOfDateModels;
        _hostingEnvironment = hostingEnvironment;
        _textBuilder = textBuilder;
        _builderBase = builderBase;
        config.OnChange(x => _config = x);
    }

    public void GenerateModels()
    {
        var modelsDirectory = _config.ModelsDirectoryAbsolute(_hostingEnvironment);
        if (!Directory.Exists(modelsDirectory))
        {
            Directory.CreateDirectory(modelsDirectory);
        }

        foreach (var file in Directory.GetFiles(modelsDirectory, "*.generated.cs"))
        {
            File.Delete(file);
        }

        IList<TypeModel> typeModels = _umbracoService.GetAllTypes();
        _builderBase.Prepare(typeModels);
        foreach (TypeModel typeModel in _builderBase.GetModelsToGenerate())
        {
            var sb = new StringBuilder();
            _textBuilder.Generate(sb, typeModel);
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
