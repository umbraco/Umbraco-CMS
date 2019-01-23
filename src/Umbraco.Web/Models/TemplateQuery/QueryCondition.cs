﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Models.TemplateQuery
{
    public class QueryCondition
    {
        public PropertyModel Property { get; set; }
        public OperatorTerm Term { get; set; }
        public string ConstraintValue { get; set; }
    }

    internal static class QueryConditionExtensions
    {
        private static Lazy<MethodInfo> StringContainsMethodInfo =>
            new Lazy<MethodInfo>(() => typeof(string).GetMethod("Contains", new[] {typeof(string)}));

        public static Expression<Func<IPublishedContent, bool>> BuildCondition(this QueryCondition condition,
            string parameterAlias, IEnumerable<IPublishedContent> contents, IEnumerable<PropertyModel> properties)
        {
            object constraintValue;
            switch (condition.Property.Type)
            {
                case "string":
                    constraintValue = condition.ConstraintValue;
                    break;
                case "datetime":
                    constraintValue = DateTime.Parse(condition.ConstraintValue);
                    break;
                default:
                    constraintValue = Convert.ChangeType(condition.ConstraintValue, typeof(int));
                    break;
            }

            var parameterExpression = Expression.Parameter(typeof(IPublishedContent), parameterAlias);
            var propertyExpression = Expression.Property(parameterExpression, condition.Property.Alias);

            var valueExpression = Expression.Constant(constraintValue);
            Expression bodyExpression;
            switch (condition.Term.Operator)
            {
                case Operator.NotEquals:
                    bodyExpression = Expression.NotEqual(propertyExpression, valueExpression);
                    break;
                case Operator.GreaterThan:
                    bodyExpression = Expression.GreaterThan(propertyExpression, valueExpression);
                    break;
                case Operator.GreaterThanEqualTo:
                    bodyExpression = Expression.GreaterThanOrEqual(propertyExpression, valueExpression);
                    break;
                case Operator.LessThan:
                    bodyExpression = Expression.LessThan(propertyExpression, valueExpression);
                    break;
                case Operator.LessThanEqualTo:
                    bodyExpression = Expression.LessThanOrEqual(propertyExpression, valueExpression);
                    break;
                case Operator.Contains:
                    bodyExpression = Expression.Call(propertyExpression, StringContainsMethodInfo.Value,
                        valueExpression);
                    break;
                case Operator.NotContains:
                    var tempExpression = Expression.Call(propertyExpression, StringContainsMethodInfo.Value,
                        valueExpression);
                    bodyExpression = Expression.Equal(tempExpression, Expression.Constant(false));
                    break;
                default:
                case Operator.Equals:
                    bodyExpression = Expression.Equal(propertyExpression, valueExpression);
                    break;
            }

            var predicate =
                Expression.Lambda<Func<IPublishedContent, bool>>(bodyExpression.Reduce(), parameterExpression);

            return predicate;
        }
    }
}
