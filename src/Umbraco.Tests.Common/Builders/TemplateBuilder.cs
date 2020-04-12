using System;
using Umbraco.Core.Models;
using Umbraco.Core.Strings;
using Umbraco.Tests.Common.Builders.Extensions;
using Umbraco.Tests.Common.Builders.Interfaces;

namespace Umbraco.Tests.Common.Builders
{
    public class TemplateBuilder
        : BuilderBase<Template>,
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

        public override Template Build()
        {
            var id = _id ?? 0;
            var key = _key ?? Guid.NewGuid();
            var name = _name ?? Guid.NewGuid().ToString();
            var alias = _alias ?? name.ToCamelCase();
            var createDate = _createDate ?? DateTime.Now;
            var updateDate = _updateDate ?? DateTime.Now;
            var path = _path ?? string.Empty;
            var content = _content;
            var isMasterTemplate = _isMasterTemplate ?? false;
            var masterTemplateAlias = _masterTemplateAlias ?? string.Empty;
            var masterTemplateId = _masterTemplateId ?? null;

            var shortStringHelper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig());

            Reset();
            return new Template(shortStringHelper, name, alias)
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
        }

        protected override void Reset()
        {
            _id = null;
            _key = null;
            _alias = null;
            _name = null;
            _createDate = null;
            _updateDate = null;
            _path = null;
            _content = null;
            _isMasterTemplate = null;
            _masterTemplateAlias = null;
            _masterTemplateId = null;
        }

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
