<<<<<<<< HEAD:src/Umbraco.Search/Services/IIndexingRebuilderService.cs
﻿namespace Umbraco.Search.Services;
========
﻿using Examine;

namespace Umbraco.Cms.Infrastructure.Services;
>>>>>>>> origin/v13/dev:src/Umbraco.Infrastructure/Services/IIndexingRebuilderService.cs

public interface IIndexingRebuilderService
{
    bool CanRebuild(string indexName);
    bool TryRebuild(string index);

    bool IsRebuilding(string indexName);
}
