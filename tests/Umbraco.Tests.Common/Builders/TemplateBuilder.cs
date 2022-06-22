// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders
{
    public class TemplateBuilder
        : ChildBuilderBase<ContentTypeBuilder, ITemplate>,
            IWithIdBuilder,
            IWithKeyBuilder,
            IWithAliasBuilder,
            IWithNameBuilder,
            IWithCreateDateBuilder,
            IWithUpdateDateBuilder,
            IWithPathBuilder
    {
        private int? _id;
        private Guid? _key;
        private string _alias;
        private string _name;
        private DateTime? _createDate;
        private DateTime? _updateDate;
        private string _path;
        private string _content;
        private bool? _isMasterTemplate;
        private string _masterTemplateAlias;
        private Lazy<int> _masterTemplateId;

        public TemplateBuilder()
            : base(null)
        {
        }

        public TemplateBuilder(ContentTypeBuilder parentBuilder)
            : base(parentBuilder)
        {
        }

        public TemplateBuilder WithContent(string content)
        {
            _content = content;
            return this;
        }

        public TemplateBuilder AsMasterTemplate(string masterTemplateAlias, int masterTemplateId)
        {
            _isMasterTemplate = true;
            _masterTemplateAlias = masterTemplateAlias;
            _masterTemplateId = new Lazy<int>(() => masterTemplateId);
            return this;
        }

        public override ITemplate Build()
        {
            var id = _id ?? 0;
            Guid key = _key ?? Guid.NewGuid();
            var name = _name ?? Guid.NewGuid().ToString();
            var alias = _alias ?? name.ToCamelCase();
            DateTime createDate = _createDate ?? DateTime.Now;
            DateTime updateDate = _updateDate ?? DateTime.Now;
            var path = _path ?? $"-1,{id}";
            var content = _content;
            var isMasterTemplate = _isMasterTemplate ?? false;
            var masterTemplateAlias = _masterTemplateAlias ?? string.Empty;
            Lazy<int> masterTemplateId = _masterTemplateId ?? new Lazy<int>(() => -1);

            var shortStringHelper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig());

            var template = new Template(shortStringHelper, name, alias)
            {
                Id = id,
                Key = key,
                CreateDate = createDate,
                UpdateDate = updateDate,
                Path = path,
                Content = content,
                IsMasterTemplate = isMasterTemplate,
                MasterTemplateAlias = masterTemplateAlias,
                MasterTemplateId = masterTemplateId,
            };

            return template;
        }

        public static Template CreateTextPageTemplate(string alias = "textPage") =>
            (Template)new TemplateBuilder()
                .WithAlias(alias)
                .WithName("Text page")
                .Build();

        int? IWithIdBuilder.Id
        {
            get => _id;
            set => _id = value;
        }

        Guid? IWithKeyBuilder.Key
        {
            get => _key;
            set => _key = value;
        }

        string IWithAliasBuilder.Alias
        {
            get => _alias;
            set => _alias = value;
        }

        string IWithNameBuilder.Name
        {
            get => _name;
            set => _name = value;
        }

        DateTime? IWithCreateDateBuilder.CreateDate
        {
            get => _createDate;
            set => _createDate = value;
        }

        DateTime? IWithUpdateDateBuilder.UpdateDate
        {
            get => _updateDate;
            set => _updateDate = value;
        }

        string IWithPathBuilder.Path
        {
            get => _path;
            set => _path = value;
        }
    }
}
