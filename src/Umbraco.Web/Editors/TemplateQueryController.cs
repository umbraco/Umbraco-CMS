using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Web.WebApi;
using System;
using System.Diagnostics;
using Umbraco.Web.Dynamics;
using Umbraco.Web.Models.TemplateQuery;

namespace Umbraco.Web.Editors
{
    

    /// <summary>
    /// The API controller used for building content queries within the template
    /// </summary>
    [PluginController("UmbracoApi")]
    [JsonCamelCaseFormatter]
    public class TemplateQueryController : UmbracoAuthorizedJsonController
    {
        public TemplateQueryController()
        { }

        public TemplateQueryController(UmbracoContext umbracoContext)
            :base(umbracoContext)
        { }


        private static readonly IEnumerable<OperathorTerm> Terms = new List<OperathorTerm>()
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

        private static readonly IEnumerable<PropertyModel> Properties = new List<PropertyModel>()
            {
                new PropertyModel() { Name = "Id", Alias = "Id", Type = "int"  },
                new PropertyModel() { Name = "Name", Alias = "Name", Type = "string"  },
                //new PropertyModel() { Name = "Url", Alias = "url", Type = "string"  },
                new PropertyModel() { Name = "Created Date", Alias = "CreateDate", Type = "datetime"  },
                new PropertyModel() { Name = "Last Updated Date", Alias = "UpdateDate", Type = "datetime"  }

            };

        public QueryResultModel PostTemplateQuery(QueryModel model)
        {
            var umbraco = new UmbracoHelper(UmbracoContext);

            var queryResult = new QueryResultModel();

            var sb = new StringBuilder();
            
            sb.Append("CurrentPage.Site()");
            
            var timer = new Stopwatch();
            
            timer.Start();

            var currentPage = umbraco.TypedContentAtRoot().FirstOrDefault();
            timer.Stop();


            var pointerNode = currentPage;

            // adjust the "FROM"
            if (model != null && model.Source.Id > 0)
            {
                var targetNode = umbraco.TypedContent(model.Source.Id);

                if (targetNode != null)
                {
                    var aliases = this.GetChildContentTypeAliases(targetNode, currentPage).Reverse();

                    foreach (var contentTypeAlias in aliases)
                    {
                        timer.Start();

                        pointerNode = pointerNode.FirstChild(x => x.DocumentTypeAlias == contentTypeAlias);

                        if (pointerNode == null) break;

                        timer.Stop();

                        sb.AppendFormat(".FirstChild(\"{0}\")", contentTypeAlias);
                    }

                    if (pointerNode == null || pointerNode.Id != model.Source.Id)
                    {
                        // we did not find the path
                        sb.Clear();
                        sb.AppendFormat("Umbraco.Content({0})", model.Source.Id);
                        pointerNode = targetNode;
                    }
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
            var token = 0;

            if (model != null)
            {
                model.Filters = model.Filters.Where(x => x.ConstraintValue != null);

                foreach (var condition in model.Filters)
                {
                    if(string.IsNullOrEmpty( condition.ConstraintValue)) continue;

                

                    var operation = condition.BuildCondition(token);

                    clause = string.IsNullOrEmpty(clause) ? operation : string.Concat(new[] { clause, " && ",  operation });

                    token++;
                }

                if (string.IsNullOrEmpty(clause) == false)
                {

                    timer.Start();

                    //clause = "Visible && " + clause;

                    contents = contents.AsQueryable().Where(clause, model.Filters.Select(this.GetConstraintValue).ToArray());
                    // contents = contents.Where(clause, values.ToArray());
                    contents = contents.Where(x => x.IsVisible());

                    timer.Stop();

                    clause = string.Format("\"Visible && {0}\",{1}", clause,
                        string.Join(",", model.Filters.Select(x => x.Property.Type == "string" ? 
                                                                       string.Format("\"{0}\"", x.ConstraintValue) : x.ConstraintValue).ToArray()));

                    sb.AppendFormat(".Where({0})", clause);
                }
                else
                {
                    timer.Start();

                    contents = contents.Where(x => x.IsVisible());

                    timer.Stop();

                    sb.Append(".Where(\"Visible\")");

                }

                if (model.Sort != null && string.IsNullOrEmpty(model.Sort.Property.Alias) == false)
                {
                    timer.Start();

                    contents = this.SortByDefaultPropertyValue(contents, model.Sort);

                    timer.Stop();

                    var direction = model.Sort.Direction == "ascending" ? string.Empty : " desc";

                    sb.AppendFormat(".OrderBy(\"{0}{1}\")", model.Sort.Property.Alias, direction);
                }

                if (model.Take > 0)
                {
                    timer.Start();

                    contents = contents.Take(model.Take);

                    timer.Stop();

                    sb.AppendFormat(".Take({0})", model.Take);
                }
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

        private object GetConstraintValue(QueryCondition condition)
        {
            switch (condition.Property.Type)
            {
                case "int" :
                    return int.Parse(condition.ConstraintValue);
                case "datetime":
                    DateTime dt;
                    return DateTime.TryParse(condition.ConstraintValue, out dt) ? dt : DateTime.Today;
                default:
                    return condition.ConstraintValue;
            }
        }

        private IEnumerable<IPublishedContent> SortByDefaultPropertyValue(IEnumerable<IPublishedContent> contents,  SortExpression sortExpression)
        {
            switch (sortExpression.Property.Alias)
            {
                case "id" :
                    return sortExpression.Direction == "ascending"
                               ? contents.OrderBy(x => x.Id)
                               : contents.OrderByDescending(x => x.Id);
                case "createDate" :
               
                    return sortExpression.Direction == "ascending"
                               ? contents.OrderBy(x => x.CreateDate)
                               : contents.OrderByDescending(x => x.CreateDate);
                case "publishDate":
                   
                    return sortExpression.Direction == "ascending"
                               ? contents.OrderBy(x => x.UpdateDate)
                               : contents.OrderByDescending(x => x.UpdateDate);
                case "name":
                    return sortExpression.Direction == "ascending"
                               ? contents.OrderBy(x => x.Name)
                               : contents.OrderByDescending(x => x.Name);
                default :

                    return sortExpression.Direction == "ascending"
                               ? contents.OrderBy(x => x.Name)
                               : contents.OrderByDescending(x => x.Name);

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
            var contentTypes =
                ApplicationContext.Services.ContentTypeService.GetAllContentTypes()
                    .Select(x => new ContentTypeModel() { Alias = x.Alias, Name = x.Name })
                    .OrderBy(x => x.Name).ToList();
            contentTypes.Insert(0, new ContentTypeModel() { Alias = string.Empty, Name = "Everything" });

            return contentTypes;
        }

        /// <summary>
        /// Returns a collection of allowed properties.
        /// </summary>
        public IEnumerable<PropertyModel> GetAllowedProperties()
        {
            return Properties.OrderBy(x => x.Name);
        }

        /// <summary>
        /// Returns a collection of constraint conditions that can be used in the query
        /// </summary>
        public IEnumerable<object> GetFilterConditions()
        {
            return Terms;
        }


    }
}