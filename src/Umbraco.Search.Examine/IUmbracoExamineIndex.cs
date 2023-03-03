using Examine;

namespace Umbraco.Search.Examine;

public interface IUmbracoExamineIndex : IIndex
{
    bool EnableDefaultEventHandler { get; set; }
}
