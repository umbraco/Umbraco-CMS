﻿namespace Umbraco.Web.Models.TemplateQuery
{
    public class QueryCondition
    {
        public PropertyModel Property { get; set; }
        public OperatorTerm Term { get; set; }
        public string ConstraintValue { get; set; }
    }
}
