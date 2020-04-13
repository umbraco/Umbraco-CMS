using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Strings;
using Umbraco.Tests.Common.Builders.Extensions;
using Umbraco.Tests.Common.Builders.Interfaces;

namespace Umbraco.Tests.Common.Builders
{
    public class MacroBuilder
        : BuilderBase<Macro>,
            IWithIdBuilder,
            IWithKeyBuilder,
            IWithAliasBuilder,
            IWithNameBuilder
    {
        private List<MacroPropertyBuilder> _propertyBuilders = new List<MacroPropertyBuilder>();

        private int? _id;
        private Guid? _key;
        private string _alias;
        private string _name;
        private bool? _useInEditor;
        private int? _cacheDuration;
        private bool? _cacheByPage;
        private bool? _cacheByMember;
        private bool? _dontRender;
        private string _macroSource;

        public MacroBuilder WithUseInEditor(bool useInEditor)
        {
            _useInEditor = useInEditor;
            return this;
        }

        public MacroBuilder WithCacheDuration(int cacheDuration)
        {
            _cacheDuration = cacheDuration;
            return this;
        }

        public MacroBuilder WithCacheByPage(bool cacheByPage)
        {
            _cacheByPage = cacheByPage;
            return this;
        }

        public MacroBuilder WithCacheByMember(bool cacheByMember)
        {
            _cacheByMember = cacheByMember;
            return this;
        }

        public MacroBuilder WithDontRender(bool dontRender)
        {
            _dontRender = dontRender;
            return this;
        }

        public MacroBuilder WithSource(string macroSource)
        {
            _macroSource = macroSource;
            return this;
        }

        public MacroPropertyBuilder AddProperty()
        {
            var builder = new MacroPropertyBuilder(this);

            _propertyBuilders.Add(builder);

            return builder;
        }

        public override Macro Build()
        {
            var id = _id ?? 1;
            var name = _name ?? Guid.NewGuid().ToString();
            var alias = _alias ?? name.ToCamelCase();
            var key = _key ?? Guid.NewGuid();
            var useInEditor = _useInEditor ?? false;
            var cacheDuration = _cacheDuration ?? 0;
            var cacheByPage = _cacheByPage ?? false;
            var cacheByMember = _cacheByMember ?? false;
            var dontRender = _dontRender ?? false;
            var macroSource = _macroSource ?? string.Empty;

            var shortStringHelper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig());

            var macro = new Macro(shortStringHelper, id, key, useInEditor, cacheDuration, alias, name, cacheByPage, cacheByMember, dontRender, macroSource);

            foreach (var property in _propertyBuilders.Select(x => x.Build()))
            {
                macro.Properties.Add(property);
            }

            Reset();
            return macro;
        }

        protected override void Reset()
        {
            _id = null;
            _key = null;
            _alias = null;
            _name = null;
            _useInEditor = null;
            _cacheDuration = null;
            _cacheByPage = null;
            _cacheByMember = null;
            _dontRender = null;
            _macroSource = null;
            _propertyBuilders = new List<MacroPropertyBuilder>();
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
    }
}
