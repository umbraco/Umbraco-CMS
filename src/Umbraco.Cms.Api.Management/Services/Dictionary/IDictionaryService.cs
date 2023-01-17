using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Services.Dictionary;

public interface IDictionaryService
{
    ProblemDetails? DetectNamingCollision(string name, IDictionaryItem? current = null);
}
