using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors
{
    using System;
    using System.Diagnostics;

    using global::umbraco;

    using Umbraco.Web.Editors.TemplateQuery;

    /// <summary>
    /// The API controller used for building content queries within the template
    /// </summary>
    [PluginController("UmbracoApi")]
    [DisableBrowserCache]
    [JsonCamelCaseFormatter]
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

        private static readonly IEnumerable<PropertyModel> _properties = new List<PropertyModel>()
            {
                new PropertyModel() { Name = "Id", Alias = "id", Type = "int"  },
                new PropertyModel() { Name = "Name", Alias = "name", Type = "string"  },
                //new PropertyModel() { Name = "Url", Alias = "url", Type = "string"  },
                new PropertyModel() { Name = "Creation Date", Alias = "createDate", Type = "datetime"  },
                new PropertyModel() { Name = "Publishing Date", Alias = "publishDate", Type = "datetime"  }

            }; 

        
        
        public QueryResultModel PostTemplateQuery(QueryModel model)
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
            if (model != null && model.Source.Id > 0)
            {
                var targetNode = umbraco.TypedContent(model.Source.Id);

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
            if (model != null && string.IsNullOrEmpty(model.ContentType.Alias) == false)
            {
                timer.Start();

                contents = pointerNode.Children.OfTypes(new[] { model.ContentType.Alias });

                timer.Stop();
                // TODO change to .Children({0})
                sb.AppendFormat(".Children(\"{0}\")", model.ContentType.Alias);
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
            foreach (var condition in model.Filters)
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

                    operation = string.Format("{0}.ToLowerInvariant().Contains(\"{1}\")", condition.Property.Name, condition.ConstraintValue.ToLowerInvariant());

                    break;

                    case Operathor.NotContains:
                    operation = string.Format("!{0}.ToLowerInvariant().Contains(\"{1}\")", condition.Property.Name, condition.ConstraintValue.ToLowerInvariant());
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

            if (model.SortExpression != null && string.IsNullOrEmpty(model.SortExpression.Property.Alias) == false)
            {
                timer.Start();

                // TODO write extension to determine if built in property or not
                contents = model.SortExpression.SortDirection == "ascending"
                               ? contents.OrderBy(x => GetDefaultPropertyValue(x, model.SortExpression.Property)).ToList()
                               : contents.OrderByDescending(x => GetDefaultPropertyValue(x, model.SortExpression.Property)).ToList();

                timer.Stop();

                var direction = model.SortExpression.SortDirection == "ascending" ? string.Empty : " desc";

                sb.AppendFormat(".OrderBy(\"{0}{1}\")", model.SortExpression.Property.Alias, direction);
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

        private object GetDefaultPropertyValue(IPublishedContent content, PropertyModel prop)
        {
            switch (prop.Alias)
            {
                case "id" :
                    return content.Id;
                case "createDate" :
                    return content.CreateDate;
                case "publishDate":
                    return content.UpdateDate;
                case "name":
                    return content.Name;
                default :
                    return content.Name;

            }
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
        /// Gets a list of all content types
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ContentTypeModel> GetContentTypes()
        {
            return
                ApplicationContext.Services.ContentTypeService.GetAllContentTypes()
                    .Select(x => new ContentTypeModel() { Alias = x.Alias, Name = x.Name }).OrderBy(x => x.Name);
        }

        /// <summary>
        /// Returns a collection of allowed properties.
        /// </summary>
        public IEnumerable<PropertyModel> GetAllowedProperties()
        {
            return _properties.OrderBy(x => x.Name);
        }

        /// <summary>
        /// Returns a collection of constraint conditions that can be used in the query
        /// </summary>
        public IEnumerable<object> GetFilterConditions()
        {
            //return _terms.Select(x => new
            //                              {
            //                                 x.Name,
            //                                 Operathor = x.Operathor.ToString(),
            //                                 x.AppliesTo
            //                              });
            return _terms;
        }


    }
}