using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Editors
{
    using System.Diagnostics;
    using System.Threading;
    using System.Web.Services.Description;

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
                    ContentTypeAlias = "umbTextPage",
                    Id = 1068,
                    Wheres = new List<IQueryCondition>()
                                 {
                                     new QueryCondition()
                                         {
                                             ConstraintValue = "Getting",
                                             FieldName = "Name",
                                             Term = _terms.FirstOrDefault(x => x.Name == "contains")
                                         }
                                 }
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

            // adjust the "FROM"
            if (model != null && model.Id > 0)
            {
                var fromTypeAlias = umbraco.TypedContent(model.Id).DocumentTypeAlias;

                timer.Start();

                currentPage = currentPage.DescendantsOrSelf(fromTypeAlias).FirstOrDefault();
                
                timer.Stop();
                
                sb.AppendFormat(".DescendantsOrSelf(\"{0}\").First()", fromTypeAlias);
            }
                
            // TYPE to return if filtered by type            
            IEnumerable<IPublishedContent> contents;
            if (model != null && string.IsNullOrEmpty(model.ContentTypeAlias) == false)
            {
                timer.Start();
                
                contents = currentPage.Descendants(model.ContentTypeAlias);

                timer.Stop();
                sb.AppendFormat(".Descendants(\"{0}\")", model.ContentTypeAlias);
            }
            else
            {
                timer.Start();
                contents = currentPage.Descendants();
                timer.Stop();
                sb.Append(".Descendants()");
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
  
        /// <summary>
        /// Returns a collection of constraint conditions that can be used in the query
        /// </summary>
        public IEnumerable<OperathorTerm> GetFilterConditions()
        {
            return _terms;
        }


    }
}