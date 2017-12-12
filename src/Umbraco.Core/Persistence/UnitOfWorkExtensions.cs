using System;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence
{
    internal static class UnitOfWorkExtensions
    {
        public static IEntityContainerRepository CreateContainerRepository(this IUnitOfWork uow, Guid containerObjectType)
        {
            if (containerObjectType == Constants.ObjectTypes.DocumentTypeContainer)
                return uow.CreateRepository<IDocumentTypeContainerRepository>();
            if (containerObjectType == Constants.ObjectTypes.MediaTypeContainer)
                return uow.CreateRepository<IMediaTypeContainerRepository>();
            throw new ArgumentOutOfRangeException(nameof(containerObjectType));
        }
    }
}
