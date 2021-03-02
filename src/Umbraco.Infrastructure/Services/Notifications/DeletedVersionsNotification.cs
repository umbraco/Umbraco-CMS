// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public class DeletedVersionsNotification<T> : INotification where T : class
    {
        public DeletedVersionsNotification(int id, EventMessages messages, int specificVersion = default, bool deletePriorVersions = false, DateTime dateToRetain = default)
        {
            Id = id;
            Messages = messages;
            SpecificVersion = specificVersion;
            DeletePriorVersions = deletePriorVersions;
            DateToRetain = dateToRetain;
        }

        public int Id { get; }

        public EventMessages Messages { get; }

        public int SpecificVersion { get; }

        public bool DeletePriorVersions { get; }

        public DateTime DateToRetain { get; }
    }
}
