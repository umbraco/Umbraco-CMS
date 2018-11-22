using System;
using System.Diagnostics;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// Provide an abstract base class for <c>IPublishedContent</c> implementations.
    /// </summary>
    [DebuggerDisplay("Content Id: {Id}, Name: {Name}")]
    public abstract class PublishedContentWithKeyBase : PublishedContentBase, IPublishedContentWithKey
    {
        public abstract Guid Key { get; }
    }
}
