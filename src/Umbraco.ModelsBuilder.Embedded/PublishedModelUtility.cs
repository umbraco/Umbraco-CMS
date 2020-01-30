﻿using System;
using System.Linq;
using System.Linq.Expressions;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Composing;

namespace Umbraco.ModelsBuilder.Embedded
{
    /// <summary>
    /// This is called from within the generated model classes
    /// </summary>
    /// <remarks>
    /// DO NOT REMOVE - although there are not code references this is used directly by the generated models.
    /// </remarks>
    public static class PublishedModelUtility
    {
        // looks safer but probably useless... ppl should not call these methods directly
        // and if they do... they have to take care about not doing stupid things

        //public static PublishedPropertyType GetModelPropertyType2<T>(Expression<Func<T, object>> selector)
        //    where T : PublishedContentModel
        //{
        //    var type = typeof (T);
        //    var s1 = type.GetField("ModelTypeAlias", BindingFlags.Public | BindingFlags.Static);
        //    var alias = (s1.IsLiteral && s1.IsInitOnly && s1.FieldType == typeof(string)) ? (string)s1.GetValue(null) : null;
        //    var s2 = type.GetField("ModelItemType", BindingFlags.Public | BindingFlags.Static);
        //    var itemType = (s2.IsLiteral && s2.IsInitOnly && s2.FieldType == typeof(PublishedItemType)) ? (PublishedItemType)s2.GetValue(null) : 0;

        //    var contentType = PublishedContentType.Get(itemType, alias);
        //    // etc...
        //}

        public static IPublishedContentType GetModelContentType(PublishedItemType itemType, string alias)
        {
            var facade = Current.UmbracoContext.PublishedSnapshot; // fixme inject!
            switch (itemType)
            {
                case PublishedItemType.Content:
                    return facade.Content.GetContentType(alias);
                case PublishedItemType.Media:
                    return facade.Media.GetContentType(alias);
                case PublishedItemType.Member:
                    return facade.Members.GetContentType(alias);
                default:
                    throw new ArgumentOutOfRangeException(nameof(itemType));
            }
        }

        public static IPublishedPropertyType GetModelPropertyType<TModel, TValue>(IPublishedContentType contentType, Expression<Func<TModel, TValue>> selector)
        //where TModel : PublishedContentModel // fixme PublishedContentModel _or_ PublishedElementModel
        {
            // fixme therefore, missing a check on TModel here

            var expr = selector.Body as MemberExpression;

            if (expr == null)
                throw new ArgumentException("Not a property expression.", nameof(selector));

            // there _is_ a risk that contentType and T do not match
            // see note above : accepted risk...

            var attr = expr.Member
                .GetCustomAttributes(typeof(ImplementPropertyTypeAttribute), false)
                .OfType<ImplementPropertyTypeAttribute>()
                .SingleOrDefault();

            if (string.IsNullOrWhiteSpace(attr?.Alias))
                throw new InvalidOperationException($"Could not figure out property alias for property \"{expr.Member.Name}\".");

            return contentType.GetPropertyType(attr.Alias);
        }
    }
}
