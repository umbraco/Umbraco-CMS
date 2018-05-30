using System;
using System.Linq.Expressions;
using System.Reflection;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;

namespace Umbraco.ModelsBuilder
{
    /// <summary>
    /// Provides extension methods to models.
    /// </summary>
    public static class PublishedElementExtensions
    {
        /// <summary>
        /// Gets the value of a property.
        /// </summary>
        public static TValue Value<TModel, TValue>(this TModel model, Expression<Func<TModel, TValue>> property, string culture = ".", string segment = ".")
            where TModel : IPublishedElement
        {
            var alias = GetAlias(model, property);
            return model.Value<TValue>(alias, culture, segment);
        }

        private static string GetAlias<TModel, TValue>(TModel model, Expression<Func<TModel, TValue>> property)
        {
            if (property.NodeType != ExpressionType.Lambda)
                throw new ArgumentException("Not a proper lambda expression (lambda).", nameof(property));

            var lambda = (LambdaExpression) property;
            var lambdaBody = lambda.Body;

            if (lambdaBody.NodeType != ExpressionType.MemberAccess)
                throw new ArgumentException("Not a proper lambda expression (body).", nameof(property));

            var memberExpression = (MemberExpression) lambdaBody;
            if (memberExpression.Expression.NodeType != ExpressionType.Parameter)
                throw new ArgumentException("Not a proper lambda expression (member).", nameof(property));

            var member = memberExpression.Member;

            var attribute = member.GetCustomAttribute<ImplementPropertyTypeAttribute>();
            if (attribute == null)
                throw new InvalidOperationException("Property is not marked with ImplementPropertyType attribute.");
            
            return attribute.Alias;
        }
    }
}
