using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Services.Dictionary;

public class DictionaryService : IDictionaryService
{
    private readonly ILocalizationService _localizationService;

    public DictionaryService(ILocalizationService localizationService) => _localizationService = localizationService;

    public ProblemDetails? DetectNamingCollision(string name, IDictionaryItem? current = null)
    {
        if (current?.ItemKey == name)
        {
            return null;
        }

        IDictionaryItem? collision = _localizationService.GetDictionaryItemByKey(name);
        return collision == null || collision.Key == current?.Key
            ? null
            : new ProblemDetailsBuilder()
                .WithStatus(StatusCodes.Status409Conflict)
                .WithTitle("Duplicate name detected")
                .WithDetail($"Another dictionary item already exists with the name: {name}")
                .Build();
    }
}
