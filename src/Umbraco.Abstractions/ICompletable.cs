using System;

namespace Umbraco.Core
{
    public interface ICompletable : IDisposable
    {
        void Complete();
    }
}
