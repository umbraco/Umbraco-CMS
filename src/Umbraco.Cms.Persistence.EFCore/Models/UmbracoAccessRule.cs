using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Models;

public class UmbracoAccessRule
{
    public Guid Id { get; set; }

    public Guid AccessId { get; set; }

    public string RuleValue { get; set; } = null!;

    public string RuleType { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public DateTime UpdateDate { get; set; }

    public virtual UmbracoAccess Access { get; set; } = null!;
}
