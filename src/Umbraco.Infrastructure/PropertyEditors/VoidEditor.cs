﻿using Microsoft.Extensions.Logging;
using Umbraco.Core.Composing;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents a void editor.
    /// </summary>
    /// <remarks>Can be used in some places where an editor is needed but no actual
    /// editor is available. Not to be used otherwise. Not discovered, and therefore
    /// not part of the editors collection.</remarks>
    [HideFromTypeFinder]
    public class VoidEditor : DataEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VoidEditor"/> class.
        /// </summary>
        /// <param name="aliasSuffix">An optional alias suffix.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        /// <remarks>The default alias of the editor is "Umbraco.Void". When a suffix is provided,
        /// it is appended to the alias. Eg if the suffix is "Foo" the alias is "Umbraco.Void.Foo".</remarks>
        public VoidEditor(string aliasSuffix, ILoggerFactory loggerFactory, IDataTypeService dataTypeService, ILocalizationService localizationService, ILocalizedTextService localizedTextService, IShortStringHelper shortStringHelper)
            : base(loggerFactory, dataTypeService, localizationService, localizedTextService, shortStringHelper)
        {
            Alias = "Umbraco.Void";
            if (string.IsNullOrWhiteSpace(aliasSuffix)) return;
            Alias += "." + aliasSuffix;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoidEditor"/> class.
        /// </summary>
        /// <param name="loggerFactory">A logger factory.</param>
        /// <remarks>The alias of the editor is "Umbraco.Void".</remarks>
        public VoidEditor(ILoggerFactory loggerFactory, IDataTypeService dataTypeService, ILocalizationService localizationService,  ILocalizedTextService localizedTextService, IShortStringHelper shortStringHelper)
            : this(null, loggerFactory, dataTypeService, localizationService, localizedTextService, shortStringHelper)
        { }
    }
}
