﻿namespace Umbraco.Core.Models
{
    public interface IPartialView : IFile
    {
        PartialViewType ViewType { get; }
    }
}
