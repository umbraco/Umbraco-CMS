// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// Represents a block list property editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.BlockGrid,
        "Block Grid",
        "blockgrid",
        ValueType = ValueTypes.Json,
        Group = Constants.PropertyEditors.Groups.Lists,
        Icon = "icon-thumbnail-list")]
    public class BlockGridPropertyEditor : BlockGridBaseEditorPropertyEditor
    {
        private readonly IIOHelper _ioHelper;

        public BlockGridPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            PropertyEditorCollection propertyEditors,
            IIOHelper ioHelper)
            : base(dataValueEditorFactory, propertyEditors)
        {
            _ioHelper = ioHelper;
        }

        #region Pre Value Editor

        protected override IConfigurationEditor CreateConfigurationEditor() => new BlockGridConfigurationEditor(_ioHelper);

        #endregion
    }
}
