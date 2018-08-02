using System;

namespace Umbraco.Web.Models.ContentEditing
{
    [Serializable]
    public class SortNode
    {
        public int Id { get; set; }
        public int SortOrder { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public SortNode[] SortNodes { get; set; }

        public SortNode()
        {
        }

        public SortNode(int id, int sortOrder, string name, DateTime createDate)
        {
            Id = Id;
            SortOrder = sortOrder;
            Name = name;
            CreateDate = createDate;
        }
    }
}
