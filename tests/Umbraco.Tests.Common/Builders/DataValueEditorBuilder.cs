// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Moq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.Common.Builders;

public class DataValueEditorBuilder<TParent> : ChildBuilderBase<TParent, IDataValueEditor>
{
    private bool? _hideLabel;
    private string _valueType;
    private string _view;

    public DataValueEditorBuilder(TParent parentBuilder)
        : base(parentBuilder)
    {
    }

    public DataValueEditorBuilder<TParent> WithView(string view)
    {
        _view = view;
        return this;
    }

    public DataValueEditorBuilder<TParent> WithHideLabel(bool hideLabel)
    {
        _hideLabel = hideLabel;
        return this;
    }

    public DataValueEditorBuilder<TParent> WithValueType(string valueType)
    {
        _valueType = valueType;
        return this;
    }

    public override IDataValueEditor Build()
    {
        var valueType = _valueType ?? Guid.NewGuid().ToString();

        return new DataValueEditor(
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IJsonSerializer>())
        {
            ValueType = valueType
        };
    }
}
