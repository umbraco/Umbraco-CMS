using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Enum for the various statuses a Content object can have
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public enum ContentStatus
    {
        Unpublished, Published, Expired, Trashed, AwaitingRelease
    }
}