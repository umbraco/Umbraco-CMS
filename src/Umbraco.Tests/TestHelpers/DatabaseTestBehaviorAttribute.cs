using System;

namespace Umbraco.Tests.TestHelpers
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DatabaseTestBehaviorAttribute : Attribute
    {
        public DatabaseBehavior Behavior { get; private set; }

        public DatabaseTestBehaviorAttribute(DatabaseBehavior behavior)
        {
            Behavior = behavior;
        }
    }
}