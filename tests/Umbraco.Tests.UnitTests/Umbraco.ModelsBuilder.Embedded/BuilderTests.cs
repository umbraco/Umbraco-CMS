// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Infrastructure.ModelsBuilder.Building;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.ModelsBuilder.Embedded;

[TestFixture]
public class BuilderTests
{
    [Test]
    public void GenerateSimpleType()
    {
        // Umbraco returns nice, pascal-cased names.
        var type1 = new TypeModel
        {
            Id = 1,
            Alias = "type1",
            ClrName = "Type1",
            Name = "type1Name",
            ParentId = 0,
            BaseType = null,
            ItemType = TypeModel.ItemTypes.Content,
        };
        type1.Properties.Add(new PropertyModel
        {
            Alias = "prop1",
            ClrName = "Prop1",
            Name = "prop1Name",
            ModelClrType = typeof(string),
        });

        TypeModel[] types = { type1 };

        var modelsBuilderConfig = new ModelsBuilderSettings();
        var builder = new TextBuilder(modelsBuilderConfig, types);

        var sb = new StringBuilder();
        builder.Generate(sb, builder.GetModelsToGenerate().First());
        var gen = sb.ToString();

        var version = ApiVersion.Current.Version;
        var expected = @"//------------------------------------------------------------------------------
// <auto-generated>
//   This code was generated by a tool.
//
//    Umbraco.ModelsBuilder.Embedded v" + version + @"
//
//   Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Linq.Expressions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.PublishedModels
{
	/// <summary>type1Name</summary>
	[PublishedModel(""type1"")]
	public partial class Type1 : PublishedContentModel
	{
		// helpers
#pragma warning disable 0109 // new is redundant
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                       @""")]
		public new const string ModelTypeAlias = ""type1"";
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                       @""")]
		public new const PublishedItemType ModelItemType = PublishedItemType.Content;
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                       @""")]
		[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]
		public new static IPublishedContentType GetModelContentType(IPublishedSnapshotAccessor publishedSnapshotAccessor)
			=> PublishedModelUtility.GetModelContentType(publishedSnapshotAccessor, ModelItemType, ModelTypeAlias);
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                       @""")]
		[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]
		public static IPublishedPropertyType GetModelPropertyType<TValue>(IPublishedSnapshotAccessor publishedSnapshotAccessor, Expression<Func<Type1, TValue>> selector)
			=> PublishedModelUtility.GetModelPropertyType(GetModelContentType(publishedSnapshotAccessor), selector);
#pragma warning restore 0109

		private IPublishedValueFallback _publishedValueFallback;

		// ctor
		public Type1(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
			: base(content, publishedValueFallback)
		{
			_publishedValueFallback = publishedValueFallback;
		}

		// properties

		///<summary>
		/// prop1Name
		///</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                       @""")]
		[global::System.Diagnostics.CodeAnalysis.MaybeNull]
		[ImplementPropertyType(""prop1"")]
		public virtual string Prop1 => this.Value<string>(_publishedValueFallback, ""prop1"");
	}
}
";
        Console.WriteLine(gen);
        Assert.AreEqual(expected.ClearLf(), gen);
    }

    [Test]
    public void GenerateSimpleType_Ambiguous_Issue()
    {
        // Umbraco returns nice, pascal-cased names.
        var type1 = new TypeModel
        {
            Id = 1,
            Alias = "type1",
            ClrName = "Type1",
            ParentId = 0,
            BaseType = null,
            ItemType = TypeModel.ItemTypes.Content,
        };
        type1.Properties.Add(new PropertyModel
        {
            Alias = "foo",
            ClrName = "Foo",
            ModelClrType = typeof(IEnumerable<>).MakeGenericType(ModelType.For("foo")),
        });

        var type2 = new TypeModel
        {
            Id = 2,
            Alias = "foo",
            ClrName = "Foo",
            ParentId = 0,
            BaseType = null,
            ItemType = TypeModel.ItemTypes.Element,
        };

        TypeModel[] types = { type1, type2 };

        var modelsBuilderConfig = new ModelsBuilderSettings();
        var builder = new TextBuilder(modelsBuilderConfig, types)
        {
            ModelsNamespace = "Umbraco.Cms.Web.Common.PublishedModels",
        };

        var sb1 = new StringBuilder();
        builder.Generate(sb1, builder.GetModelsToGenerate().Skip(1).First());
        var gen1 = sb1.ToString();
        Console.WriteLine(gen1);

        var sb = new StringBuilder();
        builder.Generate(sb, builder.GetModelsToGenerate().First());
        var gen = sb.ToString();

        var version = ApiVersion.Current.Version;
        var expected = @"//------------------------------------------------------------------------------
// <auto-generated>
//   This code was generated by a tool.
//
//    Umbraco.ModelsBuilder.Embedded v" + version + @"
//
//   Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Linq.Expressions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.PublishedModels
{
	[PublishedModel(""type1"")]
	public partial class Type1 : PublishedContentModel
	{
		// helpers
#pragma warning disable 0109 // new is redundant
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                       @""")]
		public new const string ModelTypeAlias = ""type1"";
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                       @""")]
		public new const PublishedItemType ModelItemType = PublishedItemType.Content;
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                       @""")]
		[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]
		public new static IPublishedContentType GetModelContentType(IPublishedSnapshotAccessor publishedSnapshotAccessor)
			=> PublishedModelUtility.GetModelContentType(publishedSnapshotAccessor, ModelItemType, ModelTypeAlias);
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                       @""")]
		[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]
		public static IPublishedPropertyType GetModelPropertyType<TValue>(IPublishedSnapshotAccessor publishedSnapshotAccessor, Expression<Func<Type1, TValue>> selector)
			=> PublishedModelUtility.GetModelPropertyType(GetModelContentType(publishedSnapshotAccessor), selector);
#pragma warning restore 0109

		private IPublishedValueFallback _publishedValueFallback;

		// ctor
		public Type1(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
			: base(content, publishedValueFallback)
		{
			_publishedValueFallback = publishedValueFallback;
		}

		// properties

		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                       @""")]
		[global::System.Diagnostics.CodeAnalysis.MaybeNull]
		[ImplementPropertyType(""foo"")]
		public virtual global::System.Collections.Generic.IEnumerable<global::" + modelsBuilderConfig.ModelsNamespace +
                       @".Foo> Foo => this.Value<global::System.Collections.Generic.IEnumerable<global::" +
                       modelsBuilderConfig.ModelsNamespace + @".Foo>>(_publishedValueFallback, ""foo"");
	}
}
";
        Console.WriteLine(gen);
        Assert.AreEqual(expected.ClearLf(), gen);
    }

    [Test]
    public void GenerateAmbiguous()
    {
        var type1 = new TypeModel
        {
            Id = 1,
            Alias = "type1",
            ClrName = "Type1",
            ParentId = 0,
            BaseType = null,
            ItemType = TypeModel.ItemTypes.Content,
            IsMixin = true,
        };
        type1.Properties.Add(new PropertyModel
        {
            Alias = "prop1",
            ClrName = "Prop1",
            ModelClrType = typeof(IPublishedContent),
        });
        type1.Properties.Add(new PropertyModel
        {
            Alias = "prop2",
            ClrName = "Prop2",
            ModelClrType = typeof(StringBuilder),
        });
        type1.Properties.Add(new PropertyModel
        {
            Alias = "prop3",
            ClrName = "Prop3",
            ModelClrType = typeof(BootFailedException),
        });
        TypeModel[] types = { type1 };

        var modelsBuilderConfig = new ModelsBuilderSettings();
        var builder = new TextBuilder(modelsBuilderConfig, types)
        {
            ModelsNamespace = "Umbraco.ModelsBuilder.Models", // forces conflict with Umbraco.ModelsBuilder.Umbraco
        };

        var sb = new StringBuilder();
        foreach (var model in builder.GetModelsToGenerate())
        {
            builder.Generate(sb, model);
        }

        var gen = sb.ToString();

        Console.WriteLine(gen);

        Assert.IsTrue(gen.Contains(" global::Umbraco.Cms.Core.Models.PublishedContent.IPublishedContent Prop1"));
        Assert.IsTrue(gen.Contains(" global::System.Text.StringBuilder Prop2"));
        Assert.IsTrue(gen.Contains(" global::Umbraco.Cms.Core.Exceptions.BootFailedException Prop3"));
    }

    [Test]
    public void GenerateInheritedType()
    {
        var parentType = new TypeModel
        {
            Id = 1,
            Alias = "parentType",
            ClrName = "ParentType",
            Name = "parentTypeName",
            ParentId = 0,
            IsParent = true,
            BaseType = null,
            ItemType = TypeModel.ItemTypes.Content,
        };
        parentType.Properties.Add(new PropertyModel
        {
            Alias = "prop1",
            ClrName = "Prop1",
            Name = "prop1Name",
            ModelClrType = typeof(string),
        });

        var childType = new TypeModel
        {
            Id = 2,
            Alias = "childType",
            ClrName = "ChildType",
            Name = "childTypeName",
            ParentId = 1,
            BaseType = parentType,
            ItemType = TypeModel.ItemTypes.Content,
        };

        TypeModel[] docTypes = { parentType, childType };

        var modelsBuilderConfig = new ModelsBuilderSettings();
        var builder = new TextBuilder(modelsBuilderConfig, docTypes);

        var sb = new StringBuilder();
        builder.Generate(sb, builder.GetModelsToGenerate().First());
        var genParent = sb.ToString();

        var version = ApiVersion.Current.Version;

        var expectedParent = @"//------------------------------------------------------------------------------
// <auto-generated>
//   This code was generated by a tool.
//
//    Umbraco.ModelsBuilder.Embedded v" + version + @"
//
//   Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Linq.Expressions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.PublishedModels
{
	/// <summary>parentTypeName</summary>
	[PublishedModel(""parentType"")]
	public partial class ParentType : PublishedContentModel
	{
		// helpers
#pragma warning disable 0109 // new is redundant
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                             @""")]
		public new const string ModelTypeAlias = ""parentType"";
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                             @""")]
		public new const PublishedItemType ModelItemType = PublishedItemType.Content;
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                             @""")]
		[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]
		public new static IPublishedContentType GetModelContentType(IPublishedSnapshotAccessor publishedSnapshotAccessor)
			=> PublishedModelUtility.GetModelContentType(publishedSnapshotAccessor, ModelItemType, ModelTypeAlias);
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                             @""")]
		[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]
		public static IPublishedPropertyType GetModelPropertyType<TValue>(IPublishedSnapshotAccessor publishedSnapshotAccessor, Expression<Func<ParentType, TValue>> selector)
			=> PublishedModelUtility.GetModelPropertyType(GetModelContentType(publishedSnapshotAccessor), selector);
#pragma warning restore 0109

		private IPublishedValueFallback _publishedValueFallback;

		// ctor
		public ParentType(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
			: base(content, publishedValueFallback)
		{
			_publishedValueFallback = publishedValueFallback;
		}

		// properties

		///<summary>
		/// prop1Name
		///</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                             @""")]
		[global::System.Diagnostics.CodeAnalysis.MaybeNull]
		[ImplementPropertyType(""prop1"")]
		public virtual string Prop1 => this.Value<string>(_publishedValueFallback, ""prop1"");
	}
}
";
        Console.WriteLine(genParent);
        Assert.AreEqual(expectedParent.ClearLf(), genParent);

        var sb2 = new StringBuilder();
        builder.Generate(sb2, builder.GetModelsToGenerate().Skip(1).First());
        var genChild = sb2.ToString();

        var expectedChild = @"//------------------------------------------------------------------------------
// <auto-generated>
//   This code was generated by a tool.
//
//    Umbraco.ModelsBuilder.Embedded v" + version + @"
//
//   Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Linq.Expressions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.PublishedModels
{
	/// <summary>childTypeName</summary>
	[PublishedModel(""childType"")]
	public partial class ChildType : ParentType
	{
		// helpers
#pragma warning disable 0109 // new is redundant
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                            @""")]
		public new const string ModelTypeAlias = ""childType"";
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                            @""")]
		public new const PublishedItemType ModelItemType = PublishedItemType.Content;
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                            @""")]
		[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]
		public new static IPublishedContentType GetModelContentType(IPublishedSnapshotAccessor publishedSnapshotAccessor)
			=> PublishedModelUtility.GetModelContentType(publishedSnapshotAccessor, ModelItemType, ModelTypeAlias);
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                            @""")]
		[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]
		public static IPublishedPropertyType GetModelPropertyType<TValue>(IPublishedSnapshotAccessor publishedSnapshotAccessor, Expression<Func<ChildType, TValue>> selector)
			=> PublishedModelUtility.GetModelPropertyType(GetModelContentType(publishedSnapshotAccessor), selector);
#pragma warning restore 0109

		private IPublishedValueFallback _publishedValueFallback;

		// ctor
		public ChildType(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
			: base(content, publishedValueFallback)
		{
			_publishedValueFallback = publishedValueFallback;
		}

		// properties
	}
}
";

        Console.WriteLine(genChild);
        Assert.AreEqual(expectedChild.ClearLf(), genChild);
    }

    [Test]
    public void GenerateComposedType()
    {
        // Umbraco returns nice, pascal-cased names.
        var composition1 = new TypeModel
        {
            Id = 2,
            Alias = "composition1",
            ClrName = "Composition1",
            Name = "composition1Name",
            ParentId = 0,
            BaseType = null,
            ItemType = TypeModel.ItemTypes.Content,
            IsMixin = true,
        };
        composition1.Properties.Add(new PropertyModel
        {
            Alias = "compositionProp",
            ClrName = "CompositionProp",
            Name = "compositionPropName",
            ModelClrType = typeof(string),
            ClrTypeName = typeof(string).FullName,
        });

        var type1 = new TypeModel
        {
            Id = 1,
            Alias = "type1",
            ClrName = "Type1",
            Name = "type1Name",
            ParentId = 0,
            BaseType = null,
            ItemType = TypeModel.ItemTypes.Content,
        };
        type1.Properties.Add(new PropertyModel
        {
            Alias = "prop1",
            ClrName = "Prop1",
            Name = "prop1Name",
            ModelClrType = typeof(string),
        });
        type1.MixinTypes.Add(composition1);

        TypeModel[] types = { type1, composition1 };

        var modelsBuilderConfig = new ModelsBuilderSettings();
        var builder = new TextBuilder(modelsBuilderConfig, types);

        var version = ApiVersion.Current.Version;

        var sb = new StringBuilder();
        builder.Generate(sb, builder.GetModelsToGenerate().First());
        var genComposed = sb.ToString();

        var expectedComposed = @"//------------------------------------------------------------------------------
// <auto-generated>
//   This code was generated by a tool.
//
//    Umbraco.ModelsBuilder.Embedded v" + version + @"
//
//   Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Linq.Expressions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.PublishedModels
{
	/// <summary>type1Name</summary>
	[PublishedModel(""type1"")]
	public partial class Type1 : PublishedContentModel, IComposition1
	{
		// helpers
#pragma warning disable 0109 // new is redundant
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                               @""")]
		public new const string ModelTypeAlias = ""type1"";
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                               @""")]
		public new const PublishedItemType ModelItemType = PublishedItemType.Content;
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                               @""")]
		[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]
		public new static IPublishedContentType GetModelContentType(IPublishedSnapshotAccessor publishedSnapshotAccessor)
			=> PublishedModelUtility.GetModelContentType(publishedSnapshotAccessor, ModelItemType, ModelTypeAlias);
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                               @""")]
		[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]
		public static IPublishedPropertyType GetModelPropertyType<TValue>(IPublishedSnapshotAccessor publishedSnapshotAccessor, Expression<Func<Type1, TValue>> selector)
			=> PublishedModelUtility.GetModelPropertyType(GetModelContentType(publishedSnapshotAccessor), selector);
#pragma warning restore 0109

		private IPublishedValueFallback _publishedValueFallback;

		// ctor
		public Type1(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
			: base(content, publishedValueFallback)
		{
			_publishedValueFallback = publishedValueFallback;
		}

		// properties

		///<summary>
		/// prop1Name
		///</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                               @""")]
		[global::System.Diagnostics.CodeAnalysis.MaybeNull]
		[ImplementPropertyType(""prop1"")]
		public virtual string Prop1 => this.Value<string>(_publishedValueFallback, ""prop1"");

		///<summary>
		/// compositionPropName
		///</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                               @""")]
		[global::System.Diagnostics.CodeAnalysis.MaybeNull]
		[ImplementPropertyType(""compositionProp"")]
		public virtual string CompositionProp => global::Umbraco.Cms.Web.Common.PublishedModels.Composition1.GetCompositionProp(this, _publishedValueFallback);
	}
}
";
        Console.WriteLine(genComposed);
        Assert.AreEqual(expectedComposed.ClearLf(), genComposed);

        var sb2 = new StringBuilder();
        builder.Generate(sb2, builder.GetModelsToGenerate().Skip(1).First());
        var genComposition = sb2.ToString();

        var expectedComposition = @"//------------------------------------------------------------------------------
// <auto-generated>
//   This code was generated by a tool.
//
//    Umbraco.ModelsBuilder.Embedded v" + version + @"
//
//   Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Linq.Expressions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.PublishedModels
{
	// Mixin Content Type with alias ""composition1""
	/// <summary>composition1Name</summary>
	public partial interface IComposition1 : IPublishedContent
	{
		/// <summary>compositionPropName</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                                  @""")]
		[global::System.Diagnostics.CodeAnalysis.MaybeNull]
		string CompositionProp { get; }
	}

	/// <summary>composition1Name</summary>
	[PublishedModel(""composition1"")]
	public partial class Composition1 : PublishedContentModel, IComposition1
	{
		// helpers
#pragma warning disable 0109 // new is redundant
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                                  @""")]
		public new const string ModelTypeAlias = ""composition1"";
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                                  @""")]
		public new const PublishedItemType ModelItemType = PublishedItemType.Content;
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                                  @""")]
		[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]
		public new static IPublishedContentType GetModelContentType(IPublishedSnapshotAccessor publishedSnapshotAccessor)
			=> PublishedModelUtility.GetModelContentType(publishedSnapshotAccessor, ModelItemType, ModelTypeAlias);
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                                  @""")]
		[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]
		public static IPublishedPropertyType GetModelPropertyType<TValue>(IPublishedSnapshotAccessor publishedSnapshotAccessor, Expression<Func<Composition1, TValue>> selector)
			=> PublishedModelUtility.GetModelPropertyType(GetModelContentType(publishedSnapshotAccessor), selector);
#pragma warning restore 0109

		private IPublishedValueFallback _publishedValueFallback;

		// ctor
		public Composition1(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
			: base(content, publishedValueFallback)
		{
			_publishedValueFallback = publishedValueFallback;
		}

		// properties

		///<summary>
		/// compositionPropName
		///</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                                  @""")]
		[global::System.Diagnostics.CodeAnalysis.MaybeNull]
		[ImplementPropertyType(""compositionProp"")]
		public virtual string CompositionProp => GetCompositionProp(this, _publishedValueFallback);

		/// <summary>Static getter for compositionPropName</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Umbraco.ModelsBuilder.Embedded"", """ + version +
                                  @""")]
		[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]
		public static string GetCompositionProp(IComposition1 that, IPublishedValueFallback publishedValueFallback) => that.Value<string>(publishedValueFallback, ""compositionProp"");
	}
}
";

        Console.WriteLine(genComposition);
        Assert.AreEqual(expectedComposition.ClearLf(), genComposition);
    }

    [TestCase("int", typeof(int))]
    [TestCase("global::System.Collections.Generic.IEnumerable<int>", typeof(IEnumerable<int>))]
    [TestCase("global::Umbraco.Cms.Tests.UnitTests.Umbraco.ModelsBuilder.Embedded.BuilderTestsClass1", typeof(BuilderTestsClass1))]
    [TestCase("global::Umbraco.Cms.Tests.UnitTests.Umbraco.ModelsBuilder.Embedded.BuilderTests.Class1", typeof(Class1))]
    public void WriteClrType(string expected, Type input)
    {
        // note - these assertions differ from the original tests in MB because in the embedded version, the result of Builder.IsAmbiguousSymbol is always true
        // which means global:: syntax will be applied to most things
        var builder = new TextBuilder { ModelsNamespaceForTests = "ModelsNamespace" };
        var sb = new StringBuilder();
        builder.WriteClrType(sb, input);
        Assert.AreEqual(expected, sb.ToString());
    }

    [TestCase("int", typeof(int))]
    [TestCase("global::System.Collections.Generic.IEnumerable<int>", typeof(IEnumerable<int>))]
    [TestCase("global::Umbraco.Cms.Tests.UnitTests.Umbraco.ModelsBuilder.Embedded.BuilderTestsClass1", typeof(BuilderTestsClass1))]
    [TestCase("global::Umbraco.Cms.Tests.UnitTests.Umbraco.ModelsBuilder.Embedded.BuilderTests.Class1", typeof(Class1))]
    public void WriteClrTypeUsing(string expected, Type input)
    {
        // note - these assertions differ from the original tests in MB because in the embedded version, the result of Builder.IsAmbiguousSymbol is always true
        // which means global:: syntax will be applied to most things
        var builder = new TextBuilder();
        builder.Using.Add("Umbraco.Cms.Tests.UnitTests.Umbraco.ModelsBuilder");
        builder.ModelsNamespaceForTests = "ModelsNamespace";
        var sb = new StringBuilder();
        builder.WriteClrType(sb, input);
        Assert.AreEqual(expected, sb.ToString());
    }

    [Test]
    public void WriteClrType_WithUsing()
    {
        var builder = new TextBuilder();
        builder.Using.Add("System.Text");
        builder.ModelsNamespaceForTests = "Umbraco.Tests.UnitTests.Umbraco.ModelsBuilder.Models";
        var sb = new StringBuilder();
        builder.WriteClrType(sb, typeof(StringBuilder));

        // note - these assertions differ from the original tests in MB because in the embedded version, the result of Builder.IsAmbiguousSymbol is always true
        // which means global:: syntax will be applied to most things
        Assert.AreEqual("global::System.Text.StringBuilder", sb.ToString());
    }

    [Test]
    public void WriteClrTypeAnother_WithoutUsing()
    {
        var builder = new TextBuilder
        {
            ModelsNamespaceForTests = "Umbraco.Tests.UnitTests.Umbraco.ModelsBuilder.Models",
        };
        var sb = new StringBuilder();
        builder.WriteClrType(sb, typeof(StringBuilder));
        Assert.AreEqual("global::System.Text.StringBuilder", sb.ToString());
    }

    [Test]
    public void WriteClrType_Ambiguous1()
    {
        var builder = new TextBuilder();
        builder.Using.Add("System.Text");
        builder.Using.Add("Umbraco.Tests.UnitTests.Umbraco.ModelsBuilder.Embedded");
        builder.ModelsNamespaceForTests = "SomeRandomNamespace";
        var sb = new StringBuilder();
        builder.WriteClrType(sb, typeof(global::System.Text.ASCIIEncoding));

        // note - these assertions differ from the original tests in MB because in the embedded version, the result of Builder.IsAmbiguousSymbol is always true
        // which means global:: syntax will be applied to most things
        Assert.AreEqual("global::System.Text.ASCIIEncoding", sb.ToString());
    }

    [Test]
    public void WriteClrType_Ambiguous()
    {
        var builder = new TextBuilder();
        builder.Using.Add("System.Text");
        builder.Using.Add("Umbraco.Tests.UnitTests.Umbraco.ModelsBuilder.Embedded");
        builder.ModelsNamespaceForTests = "SomeBorkedNamespace";
        var sb = new StringBuilder();
        builder.WriteClrType(sb, typeof(global::System.Text.ASCIIEncoding));

        // note - these assertions differ from the original tests in MB because in the embedded version, the result of Builder.IsAmbiguousSymbol is always true
        // which means global:: syntax will be applied to most things
        Assert.AreEqual("global::System.Text.ASCIIEncoding", sb.ToString());
    }

    [Test]
    public void WriteClrType_Ambiguous2()
    {
        var builder = new TextBuilder();
        builder.Using.Add("System.Text");
        builder.Using.Add("Umbraco.Cms.Tests.UnitTests.Umbraco.ModelsBuilder.Embedded");
        builder.ModelsNamespaceForTests = "SomeRandomNamespace";
        var sb = new StringBuilder();
        builder.WriteClrType(sb, typeof(ASCIIEncoding));

        // note - these assertions differ from the original tests in MB because in the embedded version, the result of Builder.IsAmbiguousSymbol is always true
        // which means global:: syntax will be applied to most things
        Assert.AreEqual("global::Umbraco.Cms.Tests.UnitTests.Umbraco.ModelsBuilder.Embedded.ASCIIEncoding", sb.ToString());
    }

    [Test]
    public void WriteClrType_AmbiguousNot()
    {
        var builder = new TextBuilder();
        builder.Using.Add("System.Text");
        builder.Using.Add("Umbraco.Cms.Tests.UnitTests.Umbraco.ModelsBuilder.Embedded");
        builder.ModelsNamespaceForTests = "Umbraco.Cms.Tests.UnitTests.Umbraco.ModelsBuilder.Models";
        var sb = new StringBuilder();
        builder.WriteClrType(sb, typeof(ASCIIEncoding));

        // note - these assertions differ from the original tests in MB because in the embedded version, the result of Builder.IsAmbiguousSymbol is always true
        // which means global:: syntax will be applied to most things
        Assert.AreEqual("global::Umbraco.Cms.Tests.UnitTests.Umbraco.ModelsBuilder.Embedded.ASCIIEncoding", sb.ToString());
    }

    [Test]
    public void WriteClrType_AmbiguousWithNested()
    {
        var builder = new TextBuilder();
        builder.Using.Add("System.Text");
        builder.Using.Add("Umbraco.Cms.Tests.UnitTests.Umbraco.ModelsBuilder.Embedded");
        builder.ModelsNamespaceForTests = "SomeRandomNamespace";
        var sb = new StringBuilder();
        builder.WriteClrType(sb, typeof(ASCIIEncoding.Nested));

        // note - these assertions differ from the original tests in MB because in the embedded version, the result of Builder.IsAmbiguousSymbol is always true
        // which means global:: syntax will be applied to most things
        Assert.AreEqual("global::Umbraco.Cms.Tests.UnitTests.Umbraco.ModelsBuilder.Embedded.ASCIIEncoding.Nested", sb.ToString());
    }

    public class Class1
    {
    }
}

// make it public to be ambiguous (see above)
public class ASCIIEncoding
{
    // can we handle nested types?
    public class Nested
    {
    }
}

public class BuilderTestsClass1
{
}

public class System
{
}
