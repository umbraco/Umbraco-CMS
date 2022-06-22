// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders
{
    public class ContentItemSaveBuilder : BuilderBase<ContentItemSave>,
        IWithIdBuilder,
        IWithParentIdBuilder
    {
        private List<ContentVariantSaveBuilder<ContentItemSaveBuilder>> _variantBuilders = new List<ContentVariantSaveBuilder<ContentItemSaveBuilder>>();

        private int? _id;
        private int? _parentId;
        private string _contentTypeAlias;
        private ContentSaveAction? _action;

        public ContentVariantSaveBuilder<ContentItemSaveBuilder> AddVariant()
        {
            var builder = new ContentVariantSaveBuilder<ContentItemSaveBuilder>(this);
            _variantBuilders.Add(builder);
            return builder;
        }

        public override ContentItemSave Build()
        {
            var id = _id ?? 0;
            var parentId = _parentId ?? -1;
            var contentTypeAlias = _contentTypeAlias ?? null;
            ContentSaveAction action = _action ?? ContentSaveAction.Save;
            IEnumerable<ContentVariantSave> variants = _variantBuilders.Select(x => x.Build());

            return new TestContentItemSave()
            {
                Id = id,
                ParentId = parentId,
                ContentTypeAlias = contentTypeAlias,
                Action = action,
                Variants = variants,
            };
        }

        int? IWithIdBuilder.Id
        {
            get => _id;
            set => _id = value;
        }

        int? IWithParentIdBuilder.ParentId
        {
            get => _parentId;
            set => _parentId = value;
        }

        public ContentItemSaveBuilder WithAction(ContentSaveAction action)
        {
            _action = action;
            return this;
        }

        public ContentItemSaveBuilder WithContentTypeAlias(string contentTypeAlias)
        {
            _contentTypeAlias = contentTypeAlias;
            return this;
        }
    }

    public class TestContentItemSave : ContentItemSave
    {
    }
}
