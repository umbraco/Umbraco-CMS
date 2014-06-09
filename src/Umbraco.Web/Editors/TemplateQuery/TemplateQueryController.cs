using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Editors
{
    using System;
    using System.Diagnostics;

    using global::umbraco;

    /// <summary>
    /// The API controller used for building content queries within the template
    /// </summary>
    [PluginController("UmbracoApi")]
    [DisableBrowserCache]
    public class TemplateQueryController : UmbracoAuthorizedJsonController
    {
        public TemplateQueryController()
        { }

        public TemplateQueryController(UmbracoContext umbracoContext)
            :base(umbracoContext)
        { }

        private static readonly IEnumerable<OperathorTerm> _terms = new List<OperathorTerm>()
            {
                new OperathorTerm("is", Operathor.Equals, new [] {"string"}),
                new OperathorTerm("is not", Operathor.NotEquals, new [] {"string"}),
                new OperathorTerm("before", Operathor.LessThan, new [] {"datetime"}),
                new OperathorTerm("before (including selected date)", Operathor.LessThanEqualTo, new [] {"datetime"}),
                new OperathorTerm("after", Operathor.GreaterThan, new [] {"datetime"}),
                new OperathorTerm("after (including selected date)", Operathor.GreaterThanEqualTo, new [] {"datetime"}),
                new OperathorTerm("equals", Operathor.Equals, new [] {"int"}),
                new OperathorTerm("does not equal", Operathor.NotEquals, new [] {"int"}),
                new OperathorTerm("contains", Operathor.Contains, new [] {"string"}),
                new OperathorTerm("does not contain", Operathor.NotContains, new [] {"string"}),
                new OperathorTerm("greater than", Operathor.GreaterThan, new [] {"int"}),
                new OperathorTerm("greater than or equal to", Operathor.GreaterThanEqualTo, new [] {"int"}),
                new OperathorTerm("less than", Operathor.LessThan, new [] {"int"}),
                new OperathorTerm("less than or equal to", Operathor.LessThanEqualTo, new [] {"int"})
            };


        public IQueryResultModel GetTemplateQuery2()
        {
            return GetTemplateQuery(new QueryModel()
                {
                    ContentTypeAlias = "umbNewsItem",
                    Id = 1073,
                    Wheres = new List<IQueryCondition>(),
                    //SortExpression = new SortExpression()
                    //                     {
                    //                         FieldName = "nodeName",
                    //                         SortDirection = "descending"
                    //                     },
                    Take = 3
                                 //{
                                 //    new QueryCondition()
                                 //        {
                                 //            ConstraintValue = "Getting",
                                 //            FieldName = "Name",
                                 //            Term = _terms.FirstOrDefault(x => x.Name == "contains")
                                 //        }
                                 //}
                });
        }
        
        public IQueryResultModel GetTemplateQuery(IQueryModel model)
        {
            var umbraco = new UmbracoHelper(UmbracoContext);

            var queryResult = new QueryResultModel();

            var sb = new StringBuilder();
            sb.Append(queryResult.QueryExpression);

            var timer = new Stopwatch();
            
            timer.Start();

            var currentPage = umbraco.TypedContentAtRoot().FirstOrDefault();
            
            timer.Stop();


            var pointerNode = currentPage;

            // adjust the "FROM"
            if (model != null && model.Id > 0)
            {
                var targetNode = umbraco.TypedContent(model.Id);

                var aliases = this.GetChildContentTypeAliases(targetNode, currentPage).Reverse();
                
                foreach (var contentTypeAlias in aliases)
                {
                    timer.Start();

                    pointerNode = pointerNode.Children.OfTypes(contentTypeAlias).First();

                    timer.Stop();

                    sb.AppendFormat(".FirstChild(\"{0}\")", contentTypeAlias);
                }

            }
                
            // TYPE to return if filtered by type            
            IEnumerable<IPublishedContent> contents;
            if (model != null && string.IsNullOrEmpty(model.ContentTypeAlias) == false)
            {
                timer.Start();

                contents = pointerNode.Children.OfTypes(new[] { model.ContentTypeAlias });

                timer.Stop();
                // TODO change to .Children({0})
                sb.AppendFormat(".Children(\"{0}\")", model.ContentTypeAlias);
            }
            else
            {
                timer.Start();
                contents = pointerNode.Children;
                timer.Stop();
                sb.Append(".Children");
            }

            var clause = string.Empty;

            // WHERE
            foreach (var condition in model.Wheres)
            {
                if(string.IsNullOrEmpty( condition.ConstraintValue)) continue;

                var operation = string.Empty;
                

                switch (condition.Term.Operathor)
                {
                    case Operathor.Equals:
                        operation = condition.MakeBinaryOperation(" == ");                        
                        break;

                    case Operathor.NotEquals:
                        operation = condition.MakeBinaryOperation(" != ");
                        break;
                    
                    case Operathor.GreaterThan:
                        operation = condition.MakeBinaryOperation(" > ");
                        break;
                    
                    case Operathor.GreaterThanEqualTo:
                        operation = condition.MakeBinaryOperation(" >= ");
                        break;

                    case Operathor.LessThan:
                        operation = condition.MakeBinaryOperation(" < ");
                    break;

                    case Operathor.LessThanEqualTo:
                        operation = condition.MakeBinaryOperation(" <= ");
                    break;

                    case Operathor.Contains:
                        // .Where()
                        operation = string.Format("{0}.Contains(\"{1}\")", condition.FieldName, condition.ConstraintValue);

                    break;

                    case Operathor.NotContains:
                        operation = string.Format("!{0}.Contains(\"{1}\")", condition.FieldName, condition.ConstraintValue);
                    break;
                }

                clause = string.IsNullOrEmpty(clause) ? operation : string.Concat(new[] { clause, " && ", operation });

            }

            if(string.IsNullOrEmpty(clause) == false)
            { 
                timer.Start();
                
                contents = contents.Where(clause);
                
                timer.Stop();

                sb.AppendFormat(".Where(\"{0}\")", clause);
            }

            if (model.SortExpression != null && string.IsNullOrEmpty(model.SortExpression.FieldName) == false)
            {
                timer.Start();

                // TODO write extension to determine if built in property or not
                contents = model.SortExpression.SortDirection == "ascending"
                               ? contents.OrderBy(x => x.GetPropertyValue<string>(model.SortExpression.FieldName)).ToList()
                               : contents.OrderByDescending(x => x.GetPropertyValue<string>(model.SortExpression.FieldName)).ToList();

                timer.Stop();

                var direction = model.SortExpression.SortDirection == "ascending" ? string.Empty : " desc";

                sb.AppendFormat(".OrderBy(\"{0}{1}\")", model.SortExpression.FieldName, direction);
            }

            if (model.Take > 0)
            {
                timer.Start();

                contents = contents.Take(model.Take);

                timer.Stop();

                sb.AppendFormat(".Take({0})", model.Take);
            }

            queryResult.QueryExpression = sb.ToString();
            queryResult.ExecutionTime = timer.ElapsedMilliseconds;
            queryResult.ResultCount = contents.Count();
            queryResult.SampleResults = contents.Take(20).Select(x => new TemplateQueryResult()
                                                                 {
                                                                     Icon = "icon-file",
                                                                     Name = x.Name
                                                                 });


            return queryResult;
        }

        private IEnumerable<string> GetChildContentTypeAliases(IPublishedContent targetNode, IPublishedContent current)
        {
            var aliases = new List<string>();
    
            if (targetNode.Id == current.Id) return aliases;
            if (targetNode.Id != current.Id)
            {
                aliases.Add(targetNode.DocumentTypeAlias);

            }

            aliases.AddRange(this.GetChildContentTypeAliases(targetNode.Parent, current));

            return aliases;
        }
        
        

        /// <summary>
        /// Returns a collection of constraint conditions that can be used in the query
        /// </summary>
        public IEnumerable<OperathorTerm> GetFilterConditions()
        {
            return _terms;
        }


    }
}