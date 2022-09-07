﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Factories;
using Umbraco.Cms.ManagementApi.ViewModels.Relation;

namespace Umbraco.Cms.ManagementApi.Controllers.Relation;

[ApiVersion("1.0")]
public class GetRelationController : RelationControllerBase
{
    private readonly IRelationService _relationService;
    private readonly IRelationViewModelFactory _relationViewModelFactory;

    public GetRelationController(IRelationService relationService, IRelationViewModelFactory relationViewModelFactory)
    {
        _relationService = relationService;
        _relationViewModelFactory = relationViewModelFactory;
    }

    [HttpGet("{id:int}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(RelationViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    public IActionResult Get(int id)
    {
        IRelation? relation = _relationService.GetById(id);
        if (relation is null)
        {
            return NotFound();
        }

        return Ok(_relationViewModelFactory.Create(relation));
    }
}
