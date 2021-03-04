﻿using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Models.Trees
{
    /// <summary>
    /// Represents the export member menu item
    /// </summary>
    public sealed class ExportMember : ActionMenuItem
    {
        public override string AngularServiceName => "umbracoMenuActions";

        public ExportMember(ILocalizedTextService textService) : base("export", textService)
        {
            Icon = "download-alt";
        }
    }
}
