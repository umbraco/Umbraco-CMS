using System;

namespace Umbraco.Core.Exceptions
{
    internal class DataOperationException<T> : Exception
    {
        public T Operation { get; private set; }

        public DataOperationException(T operation, string message)
            :base(message)
        {
            Operation = operation;
        }

        public DataOperationException(T operation)
            : base("Data operation exception: " + operation)
        {
            Operation = operation;
        }
    }
}