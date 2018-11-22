using System;
using System.Collections;
using System.Web.UI;

namespace umbraco
{
	/// <summary>
	/// Class that adapts an <see cref="AttributeCollection"/> to the <see cref="IDictionary"/> interface.
	/// </summary>
	public class AttributeCollectionAdapter : IDictionary
	{
		private readonly AttributeCollection _collection;

		/// <summary>
		/// Initializes a new instance of the <see cref="AttributeCollectionAdapter"/> class.
		/// </summary>
		/// <param name="collection">The collection.</param>
		public AttributeCollectionAdapter(AttributeCollection collection)
		{
			_collection = collection;
		}

		#region IDictionary Members

		/// <summary>
		/// Adds an element with the provided key and value to the <see cref="T:System.Collections.IDictionary"/> object.
		/// </summary>
		/// <param name="key">The <see cref="T:System.Object"/> to use as the key of the element to add.</param>
		/// <param name="value">The <see cref="T:System.Object"/> to use as the value of the element to add.</param>
		public void Add(object key, object value)
		{
			_collection.Add(key.ToString(), value.ToString());
		}

		/// <summary>
		/// Removes all elements from the <see cref="T:System.Collections.IDictionary"/> object.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.IDictionary"/> object is read-only.
		/// </exception>
		public void Clear()
		{
			_collection.Clear();
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.IDictionary"/> object contains an element with the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="T:System.Collections.IDictionary"/> object.</param>
		/// <returns>
		/// true if the <see cref="T:System.Collections.IDictionary"/> contains an element with the key; otherwise, false.
		/// </returns>
		public bool Contains(object key)
		{
			return _collection[key.ToString()] != null;
		}

		/// <summary>
		/// Returns an <see cref="T:System.Collections.IDictionaryEnumerator"/> object for the <see cref="T:System.Collections.IDictionary"/> object.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IDictionaryEnumerator"/> object for the <see cref="T:System.Collections.IDictionary"/> object.
		/// </returns>
		public IDictionaryEnumerator GetEnumerator()
		{
			return new AttributeCollectionAdapterEnumerator(this);
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.IDictionary"/> object has a fixed size.
		/// </summary>
		/// <value></value>
		/// <returns>true if the <see cref="T:System.Collections.IDictionary"/> object has a fixed size; otherwise, false.
		/// </returns>
		public bool IsFixedSize
		{
			get { return false; }
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.IDictionary"/> object is read-only.
		/// </summary>
		/// <value></value>
		/// <returns>true if the <see cref="T:System.Collections.IDictionary"/> object is read-only; otherwise, false.
		/// </returns>
		public bool IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.ICollection"/> object containing the keys of the <see cref="T:System.Collections.IDictionary"/> object.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// An <see cref="T:System.Collections.ICollection"/> object containing the keys of the <see cref="T:System.Collections.IDictionary"/> object.
		/// </returns>
		public ICollection Keys
		{
			get { return _collection.Keys; }
		}

		/// <summary>
		/// Removes the element with the specified key from the <see cref="T:System.Collections.IDictionary"/> object.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		public void Remove(object key)
		{
			_collection.Remove(key.ToString());
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.ICollection"/> object containing the values in the <see cref="T:System.Collections.IDictionary"/> object.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// An <see cref="T:System.Collections.ICollection"/> object containing the values in the <see cref="T:System.Collections.IDictionary"/> object.
		/// </returns>
		public ICollection Values
		{
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Gets or sets the <see cref="System.Object"/> with the specified key.
		/// </summary>
		/// <value></value>
		public object this[object key]
		{
			get { return _collection[key.ToString()]; }
			set { _collection[key.ToString()] = value.ToString(); }
		}

		#endregion

		#region ICollection Members

		/// <summary>Not implemented.</summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
		/// <param name="index">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		public void CopyTo(Array array, int index)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.ICollection"/>.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// The number of elements contained in the <see cref="T:System.Collections.ICollection"/>.
		/// </returns>
		public int Count
		{
			get { return _collection.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe).
		/// </summary>
		/// <value></value>
		/// <returns>true if access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe); otherwise, false.
		/// </returns>
		public bool IsSynchronized
		{
			get { return false; }
		}

		/// <summary>
		/// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
		/// </returns>
		public object SyncRoot
		{
			get { return _collection; }
		}

		#endregion

		#region IEnumerable Members

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			foreach (object key in _collection.Keys)
				yield return _collection[(string)key];
		}

		#endregion


		/// <summary>
		/// <see cref="IDictionaryEnumerator"/> for the <see cref="AttributeCollectionAdapter"/> class.
		/// </summary>
		private class AttributeCollectionAdapterEnumerator : IDictionaryEnumerator
		{
			private readonly AttributeCollectionAdapter _adapter;
			private readonly IEnumerator _enumerator;

			/// <summary>
			/// Initializes a new instance of the <see cref="AttributeCollectionAdapterEnumerator"/> class.
			/// </summary>
			/// <param name="adapter">The adapter.</param>
			public AttributeCollectionAdapterEnumerator(AttributeCollectionAdapter adapter)
			{
				_adapter = adapter;
				_enumerator = ((IEnumerable)adapter).GetEnumerator();
			}

			#region IDictionaryEnumerator Members

			/// <summary>
			/// Gets both the key and the value of the current dictionary entry.
			/// </summary>
			/// <value></value>
			/// <returns>
			/// A <see cref="T:System.Collections.DictionaryEntry"/> containing both the key and the value of the current dictionary entry.
			/// </returns>
			/// <exception cref="T:System.InvalidOperationException">
			/// The <see cref="T:System.Collections.IDictionaryEnumerator"/> is positioned before the first entry of the dictionary or after the last entry.
			/// </exception>
			public DictionaryEntry Entry
			{
				get { return new DictionaryEntry(Key, Value); }
			}

			/// <summary>
			/// Gets the key of the current dictionary entry.
			/// </summary>
			/// <value></value>
			/// <returns>
			/// The key of the current element of the enumeration.
			/// </returns>
			/// <exception cref="T:System.InvalidOperationException">
			/// The <see cref="T:System.Collections.IDictionaryEnumerator"/> is positioned before the first entry of the dictionary or after the last entry.
			/// </exception>
			public object Key
			{
				get { return _enumerator.Current; }
			}

			/// <summary>
			/// Gets the value of the current dictionary entry.
			/// </summary>
			/// <value></value>
			/// <returns>
			/// The value of the current element of the enumeration.
			/// </returns>
			/// <exception cref="T:System.InvalidOperationException">
			/// The <see cref="T:System.Collections.IDictionaryEnumerator"/> is positioned before the first entry of the dictionary or after the last entry.
			/// </exception>
			public object Value
			{
				get { return _adapter[_enumerator.Current]; }
			}

			#endregion

			#region IEnumerator Members

			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			/// <value></value>
			/// <returns>
			/// The current element in the collection.
			/// </returns>
			/// <exception cref="T:System.InvalidOperationException">
			/// The enumerator is positioned before the first element of the collection or after the last element.
			/// </exception>
			public object Current
			{
				get { return _enumerator.Current; }
			}

			/// <summary>
			/// Advances the enumerator to the next element of the collection.
			/// </summary>
			/// <returns>
			/// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
			/// </returns>
			/// <exception cref="T:System.InvalidOperationException">
			/// The collection was modified after the enumerator was created.
			/// </exception>
			public bool MoveNext()
			{
				return _enumerator.MoveNext();
			}

			/// <summary>
			/// Sets the enumerator to its initial position, which is before the first element in the collection.
			/// </summary>
			/// <exception cref="T:System.InvalidOperationException">
			/// The collection was modified after the enumerator was created.
			/// </exception>
			public void Reset()
			{
				_enumerator.Reset();
			}

			#endregion
		}
	}
}