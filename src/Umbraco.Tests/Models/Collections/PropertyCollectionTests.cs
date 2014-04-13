using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Models.Collections
{
    [TestFixture]
    public class PropertyCollectionTests : BaseUmbracoConfigurationTest
    {
        [Test]
        public void SimpleOrder_Returns_Null_On_FirstOrDefault_When_Empty()
        {
            var orders = new SimpleOrder();
            var item = orders.FirstOrDefault();

            Assert.That(item == null, Is.True);
        }

        [Test]
        public void PropertyCollection_Returns_Null_On_FirstOrDefault_When_Empty()
        {
            var list = new List<Property>();
            var collection = new PropertyCollection(list);

            var first = collection.FirstOrDefault();
            var second = collection.FirstOrDefault(x => x.Alias.InvariantEquals("Test"));
            
            Assert.That(first, Is.Null);
            Assert.That(first == null, Is.True);
            Assert.That(second == null, Is.True);
        }

        [Test]
        public void PropertyTypeCollection_Returns_Null_On_FirstOrDefault_When_Empty()
        {
            var list = new List<PropertyType>();
            var collection = new PropertyTypeCollection(list);

            Assert.That(collection.FirstOrDefault(), Is.Null);
            Assert.That(collection.FirstOrDefault(x => x.Alias.InvariantEquals("Test")) == null, Is.True);
        }

        [Test]
        public void PropertyGroupCollection_Returns_Null_On_FirstOrDefault_When_Empty()
        {
            var list = new List<PropertyGroup>();
            var collection = new PropertyGroupCollection(list);

            Assert.That(collection.FirstOrDefault(), Is.Null);
            Assert.That(collection.FirstOrDefault(x => x.Name.InvariantEquals("Test")) == null, Is.True);
        }

        [Test]
        public void PropertyGroups_Collection_FirstOrDefault_Returns_Null()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();

            Assert.That(contentType.PropertyGroups, Is.Not.Null);
            Assert.That(contentType.PropertyGroups.FirstOrDefault(x => x.Name.InvariantEquals("Content")) == null, Is.False);
            Assert.That(contentType.PropertyGroups.FirstOrDefault(x => x.Name.InvariantEquals("Test")) == null, Is.True);
            Assert.That(contentType.PropertyGroups.Any(x => x.Name.InvariantEquals("Test")), Is.False);
        }
    }

    public class SimpleOrder : KeyedCollection<int, OrderItem>, INotifyCollectionChanged
    {
        // The parameterless constructor of the base class creates a  
        // KeyedCollection with an internal dictionary. For this code  
        // example, no other constructors are exposed. 
        // 
        public SimpleOrder() : base() { }

        public SimpleOrder(IEnumerable<OrderItem> properties)
        {
            Reset(properties);
        }

        // This is the only method that absolutely must be overridden, 
        // because without it the KeyedCollection cannot extract the 
        // keys from the items. The input parameter type is the  
        // second generic type argument, in this case OrderItem, and  
        // the return value type is the first generic type argument, 
        // in this case int. 
        // 
        protected override int GetKeyForItem(OrderItem item)
        {
            // In this example, the key is the part number. 
            return item.PartNumber;
        }

        internal void Reset(IEnumerable<OrderItem> properties)
        {
            Clear();
            properties.ForEach(Add);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void SetItem(int index, OrderItem item)
        {
            base.SetItem(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        protected override void RemoveItem(int index)
        {
            var removed = this[index];
            base.RemoveItem(index);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
        }

        protected override void InsertItem(int index, OrderItem item)
        {
            base.InsertItem(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public new bool Contains(int partNumber)
        {
            return this.Any(x => x.PartNumber == partNumber);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, args);
            }
        }
    }

    public class OrderItem : Item
    {
        public readonly int PartNumber;
        public readonly string Description;
        public readonly double UnitPrice;

        private int _quantity = 0;

        public OrderItem(int partNumber, string description,
            int quantity, double unitPrice)
        {
            this.PartNumber = partNumber;
            this.Description = description;
            this.Quantity = quantity;
            this.UnitPrice = unitPrice;
        }

        public int Quantity
        {
            get { return _quantity; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Quantity cannot be negative.");

                _quantity = value;
            }
        }

        public override string ToString()
        {
            return String.Format(
                "{0,9} {1,6} {2,-12} at {3,8:#,###.00} = {4,10:###,###.00}",
                PartNumber, _quantity, Description, UnitPrice,
                UnitPrice * _quantity);
        }
    }

    public abstract class Item : IEntity, ICanBeDirty
    {
        private bool _hasIdentity;
        private int? _hash;
        private int _id;
        private Guid _key;

        protected Item()
        {
            _propertyChangedInfo = new Dictionary<string, bool>();
        }

        /// <summary>
        /// Integer Id
        /// </summary>
        [DataMember]
        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                HasIdentity = true;
            }
        }

        /// <summary>
        /// Guid based Id
        /// </summary>
        /// <remarks>The key is currectly used to store the Unique Id from the 
        /// umbracoNode table, which many of the entities are based on.</remarks>
        [DataMember]
        public Guid Key
        {
            get
            {
                if (_key == Guid.Empty)
                    return _id.ToGuid();

                return _key;
            }
            set { _key = value; }
        }

        /// <summary>
        /// Gets or sets the Created Date
        /// </summary>
        [DataMember]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Gets or sets the Modified Date
        /// </summary>
        [DataMember]
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// Gets or sets the WasCancelled flag, which is used to track
        /// whether some action against an entity was cancelled through some event.
        /// This only exists so we have a way to check if an event was cancelled through
        /// the new api, which also needs to take effect in the legacy api.
        /// </summary>
        [IgnoreDataMember]
        internal bool WasCancelled { get; set; }

        /// <summary>
        /// Property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Method to call on a property setter.
        /// </summary>
        /// <param name="propertyInfo">The property info.</param>
        protected virtual void OnPropertyChanged(PropertyInfo propertyInfo)
        {
            _propertyChangedInfo[propertyInfo.Name] = true;

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyInfo.Name));
            }
        }

        internal virtual void ResetIdentity()
        {
            _hasIdentity = false;
            _id = default(int);
        }

        /// <summary>
        /// Method to call on entity saved when first added
        /// </summary>
        internal virtual void AddingEntity()
        {
            CreateDate = DateTime.Now;
            UpdateDate = DateTime.Now;
        }

        /// <summary>
        /// Method to call on entity saved/updated
        /// </summary>
        internal virtual void UpdatingEntity()
        {
            UpdateDate = DateTime.Now;
        }

        /// <summary>
        /// Tracks the properties that have changed
        /// </summary>
        //private readonly IDictionary<string, bool> _propertyChangedInfo = new Dictionary<string, bool>();
        private IDictionary<string, bool> _propertyChangedInfo;

        /// <summary>
        /// Indicates whether a specific property on the current entity is dirty.
        /// </summary>
        /// <param name="propertyName">Name of the property to check</param>
        /// <returns>True if Property is dirty, otherwise False</returns>
        public virtual bool IsPropertyDirty(string propertyName)
        {
            return _propertyChangedInfo.Any(x => x.Key == propertyName);
        }

        /// <summary>
        /// Indicates whether the current entity is dirty.
        /// </summary>
        /// <returns>True if entity is dirty, otherwise False</returns>
        public virtual bool IsDirty()
        {
            return _propertyChangedInfo.Any();
        }

        /// <summary>
        /// Resets dirty properties by clearing the dictionary used to track changes.
        /// </summary>
        /// <remarks>
        /// Please note that resetting the dirty properties could potentially
        /// obstruct the saving of a new or updated entity.
        /// </remarks>
        public virtual void ResetDirtyProperties()
        {
            _propertyChangedInfo.Clear();
        }

        /// <summary>
        /// Indicates whether the current entity has an identity, eg. Id.
        /// </summary>
        public virtual bool HasIdentity
        {
            get
            {
                return _hasIdentity;
            }
            protected set
            {
                _hasIdentity = value;
            }
        }

        public static bool operator ==(Item left, Item right)
        {
            /*if (ReferenceEquals(null, left))
                return false;

            if(ReferenceEquals(null, right))
                return false;*/

            return ReferenceEquals(left, right);

            return left.Equals(right);
        }

        public static bool operator !=(Item left, Item right)
        {
            return !(left == right);
        }

        /*public virtual bool SameIdentityAs(IEntity other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return SameIdentityAs(other as Entity);
        }

        public virtual bool Equals(Entity other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return SameIdentityAs(other);
        }

        public virtual Type GetRealType()
        {
            return GetType();
        }

        public virtual bool SameIdentityAs(Entity other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() == other.GetRealType() && HasIdentity && other.HasIdentity)
                return other.Id.Equals(Id);

            return false;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return SameIdentityAs(obj as IEntity);
        }

        public override int GetHashCode()
        {
            if (!_hash.HasValue)
                _hash = !HasIdentity ? new int?(base.GetHashCode()) : new int?(Id.GetHashCode() * 397 ^ GetType().GetHashCode());
            return _hash.Value;
        }*/
    }
}