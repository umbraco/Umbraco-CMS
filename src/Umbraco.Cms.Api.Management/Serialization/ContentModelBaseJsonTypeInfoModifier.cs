// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Serialization;

/// <summary>
/// A <see cref="JsonTypeInfo"/> modifier that orders <see cref="ContentModelBase{TValueModel, TVariantModel}.Variants"/>
/// (by culture, segment) and <see cref="ContentModelBase{TValueModel, TVariantModel}.Values"/> (by culture,
/// segment, alias) deterministically on write, for any concrete implementation of
/// <see cref="ContentModelBase{TValueModel, TVariantModel}"/> - restoring the original order once the write completes.
/// </summary>
/// <remarks>
/// Reflection is only used once per resolved <see cref="JsonTypeInfo"/>, to close a strongly typed modifier over the
/// type's generic arguments; the serialization path itself is reflection-free. If the write fails, the original order
/// is not restored.
/// </remarks>
internal static class ContentModelBaseJsonTypeInfoModifier
{
    public static void Apply(JsonTypeInfo typeInfo)
    {
        if (TryGetContentModelBaseType(typeInfo.Type, out Type? contentModelBaseType) is false)
        {
            return;
        }

        Type[] genericArguments = contentModelBaseType.GetGenericArguments();
        Type modifierType = typeof(Modifier<,>).MakeGenericType(genericArguments);
        var modifier = (IModifier)Activator.CreateInstance(modifierType)!;
        modifier.Apply(typeInfo);
    }

    private static bool TryGetContentModelBaseType(Type typeToConvert, [NotNullWhen(true)] out Type? contentModelBaseType)
    {
        for (Type? current = typeToConvert; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(ContentModelBase<,>))
            {
                contentModelBaseType = current;
                return true;
            }
        }

        contentModelBaseType = null;
        return false;
    }

    private interface IModifier
    {
        void Apply(JsonTypeInfo typeInfo);
    }

    /// <summary>
    /// Does the actual sorting/restoring for a closed <c>ContentModelBase&lt;TValueModel, TVariantModel&gt;</c>.
    /// Shared by every concrete type that closes over the same two generic arguments (e.g. <c>DocumentResponseModel</c>
    /// and <c>DocumentBlueprintResponseModel</c> both close over the same value/variant pair), since the sort only
    /// ever touches members declared on <see cref="ContentModelBase{TValueModel, TVariantModel}"/> itself.
    /// </summary>
    private sealed class Modifier<TValueModel, TVariantModel> : IModifier
        where TValueModel : ValueModelBase
        where TVariantModel : VariantModelBase
    {
        private readonly ConditionalWeakTable<object, OriginalCollections> _originals = new();

        public void Apply(JsonTypeInfo typeInfo)
            => typeInfo.AddSerializationCallbacks(Sort, Restore);

        private void Sort(object obj)
        {
            var model = (ContentModelBase<TValueModel, TVariantModel>)obj;
            _originals.AddOrUpdate(model, new OriginalCollections(model.Variants, model.Values));

            model.Variants = model.Variants
                .OrderBy(variant => variant.Culture, StringComparer.Ordinal)
                .ThenBy(variant => variant.Segment, StringComparer.Ordinal)
                .ToArray();
            model.Values = model.Values
                .OrderBy(value => value.Culture, StringComparer.Ordinal)
                .ThenBy(value => value.Segment, StringComparer.Ordinal)
                .ThenBy(value => value.Alias, StringComparer.Ordinal)
                .ToArray();
        }

        private void Restore(object obj)
        {
            var model = (ContentModelBase<TValueModel, TVariantModel>)obj;
            if (_originals.TryGetValue(model, out OriginalCollections? original))
            {
                model.Variants = (IEnumerable<TVariantModel>)original.Variants;
                model.Values = (IEnumerable<TValueModel>)original.Values;
                _originals.Remove(model);
            }
        }

        private sealed record OriginalCollections(object Variants, object Values);
    }
}
