using System;
using Umbraco.Core.Models;

namespace Umbraco.Tests.CodeFirst.Definitions
{
    public class DependencyField
    {
        public string Alias { get; set; }
        public string[] DependsOn { get; set; }
        public Lazy<IContentType> ContentType { get; set; }
    }
}