﻿namespace Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;

public class RedirectUrlResponseModel
{
    public Guid Id { get; set; }

    public required string OriginalUrl { get; set; }

    public required string DestinationUrl { get; set; }

    public DateTime Created { get; set; }

    public Guid ContentId { get; set; }

    public string? Culture { get; set; }
}
