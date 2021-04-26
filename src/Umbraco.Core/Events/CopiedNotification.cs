// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Events
{
    public abstract class CopiedNotification<T> : ObjectNotification<T> where T : class
    {
        protected CopiedNotification(T original, T copy, int parentId, bool relateToOriginal, EventMessages messages) : base(original, messages)
        {
            Copy = copy;
            ParentId = parentId;
            RelateToOriginal = relateToOriginal;
        }

        public T Original => Target;

        public T Copy { get; }

        public int ParentId { get; }
        public bool RelateToOriginal { get; }
    }
}
