using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    internal class FileUnitOfWork : ScopeUnitOfWork
    {
        // fixme
        // soon as FileUnitOfWork inherits from ScopeUnitOfWork it does not make any sense anymore to keep this class around?!


        public FileUnitOfWork(IScopeProvider scopeProvider, IsolationLevel isolationLevel = IsolationLevel.Unspecified) 
            : base(scopeProvider, isolationLevel)
        { }
    }
}