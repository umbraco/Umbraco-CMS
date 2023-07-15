using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Models;

public class CmsTagRelationship
{
    public int NodeId { get; set; }

    public int TagId { get; set; }

    public int PropertyTypeId { get; set; }

    public virtual UmbracoContent Node { get; set; } = null!;

    public virtual CmsPropertyType PropertyType { get; set; } = null!;

    public virtual CmsTag Tag { get; set; } = null!;
}
