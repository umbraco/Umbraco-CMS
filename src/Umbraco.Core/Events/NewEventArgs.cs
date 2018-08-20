using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Events
{
	public class NewEventArgs<TEntity> : CancellableObjectEventArgs<TEntity>, IEquatable<NewEventArgs<TEntity>>
	{


        public NewEventArgs(TEntity eventObject, bool canCancel, string @alias, int parentId, EventMessages eventMessages)
            : base(eventObject, canCancel, eventMessages)
        {
            Alias = alias;
            ParentId = parentId;
        }

        public NewEventArgs(TEntity eventObject, bool canCancel, string @alias, TEntity parent, EventMessages eventMessages)
            : base(eventObject, canCancel, eventMessages)
        {
            Alias = alias;
            Parent = parent;
        }

        public NewEventArgs(TEntity eventObject, string @alias, int parentId, EventMessages eventMessages) 
            : base(eventObject, eventMessages)
        {
            Alias = alias;
            ParentId = parentId;
        }

        public NewEventArgs(TEntity eventObject, string @alias, TEntity parent, EventMessages eventMessages)
            : base(eventObject, eventMessages)
        {
            Alias = alias;
            Parent = parent;
        }



        public NewEventArgs(TEntity eventObject, bool canCancel, string @alias, int parentId) : base(eventObject, canCancel)
		{
			Alias = alias;
			ParentId = parentId;
		}

        public NewEventArgs(TEntity eventObject, bool canCancel, string @alias, TEntity parent)
            : base(eventObject, canCancel)
        {
            Alias = alias;
            Parent = parent;
        }

		public NewEventArgs(TEntity eventObject, string @alias, int parentId) : base(eventObject)
		{
			Alias = alias;
			ParentId = parentId;
		}

        public NewEventArgs(TEntity eventObject, string @alias, TEntity parent)
            : base(eventObject)
        {
            Alias = alias;
            Parent = parent;
        }

		/// <summary>
		/// The entity being created
		/// </summary>
		public TEntity Entity
		{
			get { return EventObject; }
		}

		/// <summary>
		/// Gets or Sets the Alias.
		/// </summary>
		public string Alias { get; private set; }

		/// <summary>
		/// Gets or Sets the Id of the parent.
		/// </summary>
		public int ParentId { get; private set; }

        /// <summary>
        /// Gets or Sets the parent IContent object.
        /// </summary>
        public TEntity Parent { get; private set; }

	    public bool Equals(NewEventArgs<TEntity> other)
	    {
	        if (ReferenceEquals(null, other)) return false;
	        if (ReferenceEquals(this, other)) return true;
	        return base.Equals(other) && string.Equals(Alias, other.Alias) && EqualityComparer<TEntity>.Default.Equals(Parent, other.Parent) && ParentId == other.ParentId;
	    }

	    public override bool Equals(object obj)
	    {
	        if (ReferenceEquals(null, obj)) return false;
	        if (ReferenceEquals(this, obj)) return true;
	        if (obj.GetType() != this.GetType()) return false;
	        return Equals((NewEventArgs<TEntity>) obj);
	    }

	    public override int GetHashCode()
	    {
	        unchecked
	        {
	            int hashCode = base.GetHashCode();
	            hashCode = (hashCode * 397) ^ Alias.GetHashCode();
	            hashCode = (hashCode * 397) ^ EqualityComparer<TEntity>.Default.GetHashCode(Parent);
	            hashCode = (hashCode * 397) ^ ParentId;
	            return hashCode;
	        }
	    }

	    public static bool operator ==(NewEventArgs<TEntity> left, NewEventArgs<TEntity> right)
	    {
	        return Equals(left, right);
	    }

	    public static bool operator !=(NewEventArgs<TEntity> left, NewEventArgs<TEntity> right)
	    {
	        return !Equals(left, right);
	    }
	}
}