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
    private readonly OutOfDateModelsStatus _outOfDateModels;
    private readonly UmbracoServices _umbracoService;
    private ModelsBuilderSettings _config;


    public ModelsGenerator(
        UmbracoServices umbracoService,
        IOptionsMonitor<ModelsBuilderSettings> config,
        OutOfDateModelsStatus outOfDateModels,
        IHostingEnvironment hostingEnvironment,
        ITextBuilder textBuilder)
    {
        _umbracoService = umbracoService;
        _config = config.CurrentValue;
        _outOfDateModels = outOfDateModels;
        _hostingEnvironment = hostingEnvironment;
        _textBuilder = textBuilder;
        config.OnChange(x => _config = x);
    }

    public void GenerateModels(string outputFileExtension)
    {
        var modelsDirectory = _config.ModelsDirectoryAbsolute(_hostingEnvironment);
        if (!Directory.Exists(modelsDirectory))
        {
            Directory.CreateDirectory(modelsDirectory);
        }

        foreach (var file in Directory.GetFiles(modelsDirectory, "*" + outputFileExtension))
        {
            File.Delete(file);
        }

        IList<TypeModel> typeModels = _umbracoService.GetAllTypes();

        foreach (TypeModel typeModel in typeModels)
        {
            var sb = new StringBuilder();
            _textBuilder.Generate(sb, typeModel, typeModels);
            var filename = Path.Combine(modelsDirectory, typeModel.ClrName + outputFileExtension);
            File.WriteAllText(filename, sb.ToString());
        }

        _outOfDateModels.Clear();
    }
}
