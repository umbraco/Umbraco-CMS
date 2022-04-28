// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors
{
    internal class BlockListConfigurationEditor : ConfigurationEditor<BlockListConfiguration>
    {
        public BlockListConfigurationEditor(IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser) : base(ioHelper, editorConfigurationParser)
        {
        }

    }
}
