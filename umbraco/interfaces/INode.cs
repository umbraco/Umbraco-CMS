using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace umbraco.interfaces
{
    public interface INode
    {
        INode Parent { get; }
        int Id { get; }
        int template { get; }
        int SortOrder { get; }
        string Name { get; }
        string Url { get; }
        string UrlName { get; }
        string NodeTypeAlias { get; }
        string WriterName { get; }
        string CreatorName { get; }
        int WriterID { get; }
        int CreatorID { get; }
        string Path { get; }
        DateTime CreateDate { get; }
        DateTime UpdateDate { get; }
        Guid Version { get; }
        string NiceUrl { get; }
        int Level { get; }
        List<IProperty> PropertiesAsList { get; }
        List<INode> ChildrenAsList { get; }
        IProperty GetProperty(string Alias);
        DataTable ChildrenAsTable();
        DataTable ChildrenAsTable(string nodeTypeAliasFilter);
    }

    public interface IProperty
    {
        string Alias { get; }
        string Value { get; }
        Guid Version { get; }
        string ToString();
    }
}
