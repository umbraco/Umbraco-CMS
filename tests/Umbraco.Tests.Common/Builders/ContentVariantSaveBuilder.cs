// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders
{
    public class ContentVariantSaveBuilder<TParent> : ChildBuilderBase<TParent, ContentVariantSave>,
        IWithNameBuilder,
        IWithCultureInfoBuilder
    {
        private readonly List<ContentPropertyBasicBuilder<ContentVariantSaveBuilder<TParent>>> _propertyBuilders = new List<ContentPropertyBasicBuilder<ContentVariantSaveBuilder<TParent>>>();

        private string _name;
        private CultureInfo _cultureInfo;
        private bool? _save = null;
        private bool? _publish = null;

        public ContentVariantSaveBuilder(TParent parentBuilder)
            : base(parentBuilder)
        {
        }

        public ContentVariantSaveBuilder<TParent> WithSave(bool save)
        {
            _save = save;
            return this;
        }

        public ContentVariantSaveBuilder<TParent> WithPublish(bool publish)
        {
            _publish = publish;
            return this;
        }

        public ContentPropertyBasicBuilder<ContentVariantSaveBuilder<TParent>> AddProperty()
        {
            var builder = new ContentPropertyBasicBuilder<ContentVariantSaveBuilder<TParent>>(this);
            _propertyBuilders.Add(builder);
            return builder;
        }

        public override ContentVariantSave Build()
        {
            var name = _name ?? null;
            var culture = _cultureInfo?.Name ?? null;
            var save = _save ?? true;
            var publish = _publish ?? true;
            IEnumerable<ContentPropertyBasic> properties = _propertyBuilders.Select(x => x.Build());

            return new ContentVariantSave()
            {
                Name = name,
                Culture = culture,
                Save = save,
                Publish = publish,
                Properties = properties
            };
        }

        string IWithNameBuilder.Name
        {
            get => _name;
            set => _name = value;
        }

        CultureInfo IWithCultureInfoBuilder.CultureInfo
        {
            get => _cultureInfo;
            set => _cultureInfo = value;
        }
    }
}
