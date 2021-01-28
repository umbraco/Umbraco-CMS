using System;

namespace Umbraco.Core.Models
{
    internal struct ReadOnlyContentBaseAdapter : IReadOnlyContentBase
    {
        private readonly IContentBase _content;

        private ReadOnlyContentBaseAdapter(IContentBase content)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public static ReadOnlyContentBaseAdapter Create(IContentBase content) => new ReadOnlyContentBaseAdapter(content);

        public int Id => _content.Id;

        public Guid Key => _content.Key;

        public DateTime CreateDate => _content.CreateDate;

        public DateTime UpdateDate => _content.UpdateDate;

        public string Name => _content.Name;

        public int CreatorId => _content.CreatorId;

        public int ParentId => _content.ParentId;

        public int Level => _content.Level;

        public string Path => _content.Path;

        public int SortOrder => _content.SortOrder;

        public int ContentTypeId => _content.ContentTypeId;

        public int WriterId => _content.WriterId;

        public int VersionId => _content.VersionId;
    }
}
