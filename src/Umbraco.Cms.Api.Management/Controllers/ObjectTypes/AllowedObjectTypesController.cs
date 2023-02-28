﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.RelationType;

namespace Umbraco.Cms.Api.Management.Controllers.ObjectTypes;

public class AllowedObjectTypesController : ObjectTypesControllerBase
{
    private readonly IObjectTypeViewModelFactory _objectTypeViewModelFactory;

    public AllowedObjectTypesController(IObjectTypeViewModelFactory objectTypeViewModelFactory) => _objectTypeViewModelFactory = objectTypeViewModelFactory;

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<ObjectTypeResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Allowed(int skip = 0, int take = 100)
    {
        ObjectTypeResponseModel[] objectTypes = _objectTypeViewModelFactory.Create().ToArray();

        return await Task.FromResult(Ok(new PagedViewModel<ObjectTypeResponseModel>
        {
            Total = objectTypes.Length,
            Items = objectTypes.Skip(skip).Take(take),
        }));
    }
}
