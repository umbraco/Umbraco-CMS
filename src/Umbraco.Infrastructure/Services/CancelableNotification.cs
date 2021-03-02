// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Services
{
    // TODO split this file into several small classes and move to another namespace

    public interface ICancelableNotification : INotification
    {
        bool Cancel { get; set; }
    }

    public abstract class ObjectNotification<T> : INotification where T : class
    {
        protected ObjectNotification(T target, EventMessages messages)
        {
            Messages = messages;
            Target = target;
        }

        public EventMessages Messages { get; }

        protected T Target { get; }
    }

    public abstract class EnumerableObjectNotification<T> : ObjectNotification<IEnumerable<T>>
    {
        protected EnumerableObjectNotification(T target, EventMessages messages) : base(new [] {target}, messages)
        {
        }

        protected EnumerableObjectNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }
    }

    public abstract class CancelableObjectNotification<T> : ObjectNotification<T>, ICancelableNotification where T : class
    {
        protected CancelableObjectNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public bool Cancel { get; set; }

        public void CancelOperation(EventMessage cancelationMessage)
        {
            Cancel = true;
            cancelationMessage.IsDefaultEventMessage = true;
            Messages.Add(cancelationMessage);
        }
    }

    public abstract class CancelableEnumerableObjectNotification<T> : CancelableObjectNotification<IEnumerable<T>>
    {
        protected CancelableEnumerableObjectNotification(T target, EventMessages messages) : base(new [] {target}, messages)
        {
        }
        protected CancelableEnumerableObjectNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }
    }

    public class DeletingNotification<T> : CancelableEnumerableObjectNotification<T>
    {
        public DeletingNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public DeletingNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> DeletedEntities => Target;
    }

    public class DeletedNotification<T> : EnumerableObjectNotification<T>
    {
        public DeletedNotification(T target, EventMessages messages) : base(target, messages) => MediaFilesToDelete = new List<string>();

        public IEnumerable<T> DeletedEntities => Target;

        public List<string> MediaFilesToDelete { get; }
    }

    public class DeletedBlueprintNotification<T> : EnumerableObjectNotification<T>
    {
        public DeletedBlueprintNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public DeletedBlueprintNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> DeletedBlueprints => Target;
    }

    public class SortingNotification<T> : CancelableEnumerableObjectNotification<T>
    {
        public SortingNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> SortedEntities => Target;
    }

    public class SortedNotification<T> : EnumerableObjectNotification<T>
    {
        public SortedNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> SortedEntities => Target;
    }

    public class SavingNotification<T> : CancelableEnumerableObjectNotification<T>
    {
        public SavingNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public SavingNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> SavedEntities => Target;
    }

    public class SavedNotification<T> : EnumerableObjectNotification<T>
    {
        public SavedNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public SavedNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> SavedEntities => Target;
    }

    public class SavedBlueprintNotification<T> : ObjectNotification<T> where T : class
    {
        public SavedBlueprintNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public T SavedBlueprint => Target;
    }

    public class PublishingNotification<T> : CancelableEnumerableObjectNotification<T>
    {
        public PublishingNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public PublishingNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> PublishedEntities => Target;
    }

    public class PublishedNotification<T> : EnumerableObjectNotification<T>
    {
        public PublishedNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public PublishedNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> PublishedEntities => Target;
    }

    public class MovingNotification<T> : CancelableObjectNotification<IEnumerable<MoveEventInfo<T>>>
    {
        public MovingNotification(MoveEventInfo<T> target, EventMessages messages) : base(new[] {target}, messages)
        {
        }

        public MovingNotification(IEnumerable<MoveEventInfo<T>> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<MoveEventInfo<T>> MoveInfoCollection => Target;
    }

    public class MovedNotification<T> : ObjectNotification<IEnumerable<MoveEventInfo<T>>>
    {
        public MovedNotification(MoveEventInfo<T> target, EventMessages messages) : base(new[] { target }, messages)
        {
        }

        public MovedNotification(IEnumerable<MoveEventInfo<T>> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<MoveEventInfo<T>> MoveInfoCollection => Target;
    }

    public class MovingToRecycleBinNotification<T> : CancelableObjectNotification<IEnumerable<MoveEventInfo<T>>>
    {
        public MovingToRecycleBinNotification(MoveEventInfo<T> target, EventMessages messages) : base(new[] { target }, messages)
        {
        }

        public MovingToRecycleBinNotification(IEnumerable<MoveEventInfo<T>> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<MoveEventInfo<T>> MoveInfoCollection => Target;
    }

    public class MovedToRecycleBinNotification<T> : ObjectNotification<IEnumerable<MoveEventInfo<T>>>
    {
        public MovedToRecycleBinNotification(MoveEventInfo<T> target, EventMessages messages) : base(new[] { target }, messages)
        {
        }

        public MovedToRecycleBinNotification(IEnumerable<MoveEventInfo<T>> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<MoveEventInfo<T>> MoveInfoCollection => Target;
    }

    public class CopyingNotification<T> : CancelableObjectNotification<T> where T : class
    {
        public CopyingNotification(T original, T copy, int parentId, EventMessages messages) : base(original, messages)
        {
            Copy = copy;
            ParentId = parentId;
        }

        public T Original => Target;

        public T Copy { get; }

        public int ParentId { get; }
    }

    public class CopiedNotification<T> : ObjectNotification<T> where T : class
    {
        public CopiedNotification(T original, T copy, int parentId, EventMessages messages) : base(original, messages)
        {
            Copy = copy;
            ParentId = parentId;
        }

        public T Original => Target;

        public T Copy { get; }

        public int ParentId { get; }
    }

    public class RollingBackNotification<T> : CancelableObjectNotification<T> where T : class
    {
        public RollingBackNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public T Entity => Target;
    }

    public class RolledBackNotification<T> : ObjectNotification<T> where T : class
    {
        public RolledBackNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public T Entity => Target;
    }

    public class SendingToPublishNotification<T> : CancelableObjectNotification<T> where T : class
    {
        public SendingToPublishNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public T Entity => Target;
    }

    public class SentToPublishNotification<T> : ObjectNotification<T> where T : class
    {
        public SentToPublishNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public T Entity => Target;
    }


    public class UnpublishingNotification<T> : CancelableEnumerableObjectNotification<T>
    {
        public UnpublishingNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public UnpublishingNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> UnpublishedEntities => Target;
    }

    public class UnpublishedNotification<T> : EnumerableObjectNotification<T>
    {
        public UnpublishedNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public UnpublishedNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> UnpublishedEntities => Target;
    }

    public class EmptiedRecycleBinNotification<T> : INotification where T : class
    {
        public EmptiedRecycleBinNotification(EventMessages messages)
        {
            Messages = messages;
        }

        public EventMessages Messages { get; }
    }

    public class EmptyingRecycleBinNotification<T> : EmptiedRecycleBinNotification<T>, ICancelableNotification where T : class
    {
        public EmptyingRecycleBinNotification(EventMessages messages)
            : base(messages)
        {
        }

        public bool Cancel { get; set; }
    }

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

    public class DeletingVersionsNotification<T> : DeletedVersionsNotification<T>, ICancelableNotification where T : class
    {
        public DeletingVersionsNotification(int id, EventMessages messages, int specificVersion = default, bool deletePriorVersions = false, DateTime dateToRetain = default)
            : base(id, messages, specificVersion, deletePriorVersions, dateToRetain)
        {
        }

        public bool Cancel { get; set; }
    }

    public static class ContentNotificationExtensions
    {
        /// <summary>
        /// Determines whether a culture is being saved, during a Saving notification
        /// </summary>
        public static bool IsSavingCulture<T>(this SavingNotification<T> notification, T content, string culture) where T : IContentBase
            => content.CultureInfos.TryGetValue(culture, out ContentCultureInfos cultureInfo) && cultureInfo.IsDirty();

        /// <summary>
        /// Determines whether a culture has been saved, during a Saved notification
        /// </summary>
        public static bool HasSavedCulture<T>(this SavedNotification<T> notification, T content, string culture) where T : IContentBase
            => content.WasPropertyDirty(ContentBase.ChangeTrackingPrefix.UpdatedCulture + culture);

        /// <summary>
        /// Determines whether a culture is being published, during a Publishing notification
        /// </summary>
        public static bool IsPublishingCulture(this PublishingNotification<IContent> notification, IContent content, string culture)
            => content.PublishCultureInfos.TryGetValue(culture, out ContentCultureInfos cultureInfo) && cultureInfo.IsDirty();

        /// <summary>
        /// Determines whether a culture is being unpublished, during a Publishing notification
        /// </summary>
        public static bool IsUnpublishingCulture(this UnpublishingNotification<IContent> notification, IContent content, string culture)
            => content.IsPropertyDirty(ContentBase.ChangeTrackingPrefix.UnpublishedCulture + culture); //bit of a hack since we know that the content implementation tracks changes this way

        /// <summary>
        /// Determines whether a culture has been published, during a Published notification
        /// </summary>
        public static bool HasPublishedCulture(this PublishedNotification<IContent> notification, IContent content, string culture)
            => content.WasPropertyDirty(ContentBase.ChangeTrackingPrefix.ChangedCulture + culture);

        /// <summary>
        /// Determines whether a culture has been unpublished, during a Published notification
        /// </summary>
        public static bool HasUnpublishedCulture(this UnpublishedNotification<IContent> notification, IContent content, string culture)
            => content.WasPropertyDirty(ContentBase.ChangeTrackingPrefix.UnpublishedCulture + culture);

    }
}
