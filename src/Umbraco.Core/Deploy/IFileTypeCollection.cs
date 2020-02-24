﻿namespace Umbraco.Core.Deploy
{
    public interface IFileTypeCollection
    {
        IFileType this[string entityType] { get; }

        bool Contains(string entityType);
    }
}
