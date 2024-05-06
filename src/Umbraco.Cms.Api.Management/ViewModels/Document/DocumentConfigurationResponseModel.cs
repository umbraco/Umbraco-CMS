﻿namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentConfigurationResponseModel
{
    public required bool DisableDeleteWhenReferenced { get; set; }

    public required bool DisableUnpublishWhenReferenced { get; set; }

    public required bool AllowEditInvariantFromNonDefault { get; set; }

    public required bool AllowNonExistingSegmentsCreation { get; set; }

    public required ISet<string> ReservedFieldNames { get; set; }
}
