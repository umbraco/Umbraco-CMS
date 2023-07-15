using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Models;

public class UmbracoLock
{
    public int Id { get; set; }

    public int Value { get; set; }

    public string Name { get; set; } = null!;
}
