using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models.Blocks;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a block list property editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.BlockList,
        "Block List",
        "blocklist",
        ValueType = ValueTypes.Json,
        Group = Constants.PropertyEditors.Groups.Lists,
        Icon = "icon-thumbnail-list")]
    public class BlockListPropertyEditor : BlockEditorPropertyEditor
    {
        private readonly IIOHelper _ioHelper;

        public BlockListPropertyEditor(
            ILoggerFactory loggerFactory,
            Lazy<PropertyEditorCollection> propertyEditors,
            IDataTypeService dataTypeService,
            IContentTypeService contentTypeService,
            ILocalizedTextService localizedTextService,
            IIOHelper ioHelper,
            ILocalizationService localizationService,
            IShortStringHelper shortStringHelper)
            : base(loggerFactory, propertyEditors, dataTypeService, contentTypeService, localizedTextService, localizationService, shortStringHelper)
        {
            _ioHelper = ioHelper;
        }

        #region Pre Value Editor

        protected override IConfigurationEditor CreateConfigurationEditor() => new BlockListConfigurationEditor(_ioHelper);

        #endregion
    }
}
