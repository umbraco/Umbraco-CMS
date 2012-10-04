using System;
using System.Reflection;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Mappers
{
    internal class ModelDtoMapper : IMapper
    {
        public void GetTableInfo(Type t, TableInfo ti)
        { }

        public bool MapPropertyToColumn(PropertyInfo pi, ref string columnName, ref bool resultColumn)
        {
            if (pi.DeclaringType == typeof(Content) || pi.DeclaringType == typeof(IContent))
            {
                switch (pi.Name)
                {
                    case "Trashed":
                        columnName = "[umbracoNode].[trashed]";
                        return true;
                    case "ParentId":
                        columnName = "[umbracoNode].[parentID]";
                        return true;
                    case "UserId":
                        columnName = "[umbracoNode].[nodeUser]";
                        return true;
                    case "Level":
                        columnName = "[umbracoNode].[level]";
                        return true;
                    case "Path":
                        columnName = "[umbracoNode].[path]";
                        return true;
                    case "SortOrder":
                        columnName = "[umbracoNode].[sortOrder]";
                        return true;
                    case "NodeId":
                        columnName = "[umbracoNode].[id]";
                        return true;
                    case "Published":
                        columnName = "[cmsDocument].[published]";
                        return true;
                    case "Key":
                        columnName = "[umbracoNode].[uniqueID]";
                        return true;
                    case "CreatedDate":
                        columnName = "[umbracoNode].[createDate]";
                        return true;
                    case "Name":
                        columnName = "[umbracoNode].[text]";
                        return true;
                }
            }

            if (pi.DeclaringType == typeof(ContentType) || pi.DeclaringType == typeof(IContentType))
            {
                switch (pi.Name)
                {
                    case "Alias":
                        columnName = "[cmsContentType].[alias]";
                        return true;
                    case "Icon":
                        columnName = "[cmsContentType].[icon]";
                        return true;
                    case "Thumbnail":
                        columnName = "[cmsContentType].[thumbnail]";
                        return true;
                    case "Description":
                        columnName = "[cmsContentType].[description]";
                        return true;
                }
            }

            return true;
        }

        public Func<object, object> GetFromDbConverter(PropertyInfo pi, Type sourceType)
        {
            return null;
        }

        public Func<object, object> GetToDbConverter(Type sourceType)
        {
            return null;
        }
    }
}