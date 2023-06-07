// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Web.BackOffice.Authorization;

public class MediaPermissionsResource
{
    public MediaPermissionsResource(IMedia? media) => Media = media;

    public MediaPermissionsResource(int nodeId) => NodeId = nodeId;

    public int? NodeId { get; }
    public IMedia? Media { get; }
}
