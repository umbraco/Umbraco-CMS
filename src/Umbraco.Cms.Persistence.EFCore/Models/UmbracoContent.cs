using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Models;

public class UmbracoContent
{
    public int NodeId { get; set; }

    public int ContentTypeId { get; set; }

    public virtual ICollection<CmsContentNu> CmsContentNus { get; set; } = new List<CmsContentNu>();

    public virtual CmsMember? CmsMember { get; set; }

    public virtual ICollection<CmsTagRelationship> CmsTagRelationships { get; set; } = new List<CmsTagRelationship>();

    public virtual CmsContentType ContentType { get; set; } = null!;

    public virtual UmbracoNode Node { get; set; } = null!;

    public virtual ICollection<UmbracoContentSchedule> UmbracoContentSchedules { get; set; } = new List<UmbracoContentSchedule>();

    public virtual ICollection<UmbracoContentVersion> UmbracoContentVersions { get; set; } = new List<UmbracoContentVersion>();

    public virtual UmbracoDocument? UmbracoDocument { get; set; }
}
