using System;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence
{
    internal static class UnitOfWorkExtensions
    {
        public static IPartialViewRepository CreatePartialViewRepository(this IUnitOfWork uow, PartialViewType partialViewType)
        {
            switch (partialViewType)
            {
                case PartialViewType.PartialView:
                    return uow.CreateRepository<IPartialViewRepository>();
                case PartialViewType.PartialViewMacro:
                    return uow.CreateRepository<IPartialViewMacroRepository>();
                default:
                    throw new ArgumentOutOfRangeException(nameof(partialViewType));
            }
        }

        public static IEntityContainerRepository CreateContainerRepository(this IUnitOfWork uow, Guid containerObjectType)
        {
            if (containerObjectType == Constants.ObjectTypes.DocumentTypeContainerGuid)
                return uow.CreateRepository<IDocumentTypeContainerRepository>();
            if (containerObjectType == Constants.ObjectTypes.MediaTypeContainerGuid)
                return uow.CreateRepository<IMediaTypeContainerRepository>();
            throw new ArgumentOutOfRangeException(nameof(containerObjectType));
        }
    }
}