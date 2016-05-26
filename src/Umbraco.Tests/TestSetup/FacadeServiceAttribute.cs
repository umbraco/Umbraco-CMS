using System;

namespace Umbraco.Tests.TestSetup
{
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class FacadeServiceAttribute : Attribute
    {
        public bool EnableRepositoryEvents { get; set; }
    }
}