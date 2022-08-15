using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Umbraco.Cms.Core.Media.Exif;

/// <summary>
///     Represents a collection of <see cref="ExifProperty" /> objects.
/// </summary>
internal class ExifPropertyCollection : IDictionary<ExifTag, ExifProperty>
{
    #region Constructor

    internal ExifPropertyCollection(ImageFile parentFile)
    {
        parent = parentFile;
        items = new Dictionary<ExifTag, ExifProperty>();
    }

    #endregion

    #region Member Variables

    private readonly ImageFile parent;
    private readonly Dictionary<ExifTag, ExifProperty> items;

    #endregion

    #region Properties

    /// <summary>
    ///     Gets the number of elements contained in the collection.
    /// </summary>
    public int Count => items.Count;

    /// <summary>
    ///     Gets a collection containing the keys in this collection.
    /// </summary>
    public ICollection<ExifTag> Keys => items.Keys;

    /// <summary>
    ///     Gets a collection containing the values in this collection.
    /// </summary>
    public ICollection<ExifProperty> Values => items.Values;

    /// <summary>
    ///     Gets or sets the <see cref="ExifProperty" /> with the specified key.
    /// </summary>
    public ExifProperty this[ExifTag key]
    {
        get => items[key];
        set
        {
            if (items.ContainsKey(key))
            {
                items.Remove(key);
            }

            items.Add(key, value);
        }
    }

    #endregion

    #region ExifProperty Collection Setters

    /// <summary>
    ///     Sets the <see cref="ExifProperty" /> with the specified key.
    /// </summary>
    /// <param name="key">The tag to set.</param>
    /// <param name="value">The value of tag.</param>
    public void Set(ExifTag key, byte value)
    {
        if (items.ContainsKey(key))
        {
            items.Remove(key);
        }

        items.Add(key, new ExifByte(key, value));
    }

    /// <summary>
    ///     Sets the <see cref="ExifProperty" /> with the specified key.
    /// </summary>
    /// <param name="key">The tag to set.</param>
    /// <param name="value">The value of tag.</param>
    public void Set(ExifTag key, string value)
    {
        if (items.ContainsKey(key))
        {
            items.Remove(key);
        }

        if (key == ExifTag.WindowsTitle || key == ExifTag.WindowsComment || key == ExifTag.WindowsAuthor ||
            key == ExifTag.WindowsKeywords || key == ExifTag.WindowsSubject)
        {
            items.Add(key, new WindowsByteString(key, value));
        }
        else
        {
            items.Add(key, new ExifAscii(key, value, parent.Encoding));
        }
    }

    /// <summary>
    ///     Sets the <see cref="ExifProperty" /> with the specified key.
    /// </summary>
    /// <param name="key">The tag to set.</param>
    /// <param name="value">The value of tag.</param>
    public void Set(ExifTag key, ushort value)
    {
        if (items.ContainsKey(key))
        {
            items.Remove(key);
        }

        items.Add(key, new ExifUShort(key, value));
    }

    /// <summary>
    ///     Sets the <see cref="ExifProperty" /> with the specified key.
    /// </summary>
    /// <param name="key">The tag to set.</param>
    /// <param name="value">The value of tag.</param>
    public void Set(ExifTag key, int value)
    {
        if (items.ContainsKey(key))
        {
            items.Remove(key);
        }

        items.Add(key, new ExifSInt(key, value));
    }

    /// <summary>
    ///     Sets the <see cref="ExifProperty" /> with the specified key.
    /// </summary>
    /// <param name="key">The tag to set.</param>
    /// <param name="value">The value of tag.</param>
    public void Set(ExifTag key, uint value)
    {
        if (items.ContainsKey(key))
        {
            items.Remove(key);
        }

        items.Add(key, new ExifUInt(key, value));
    }

    /// <summary>
    ///     Sets the <see cref="ExifProperty" /> with the specified key.
    /// </summary>
    /// <param name="key">The tag to set.</param>
    /// <param name="value">The value of tag.</param>
    public void Set(ExifTag key, float value)
    {
        if (items.ContainsKey(key))
        {
            items.Remove(key);
        }

        items.Add(key, new ExifURational(key, new MathEx.UFraction32(value)));
    }

    /// <summary>
    ///     Sets the <see cref="ExifProperty" /> with the specified key.
    /// </summary>
    /// <param name="key">The tag to set.</param>
    /// <param name="value">The value of tag.</param>
    public void Set(ExifTag key, double value)
    {
        if (items.ContainsKey(key))
        {
            items.Remove(key);
        }

        items.Add(key, new ExifURational(key, new MathEx.UFraction32(value)));
    }

    /// <summary>
    ///     Sets the <see cref="ExifProperty" /> with the specified key.
    /// </summary>
    /// <param name="key">The tag to set.</param>
    /// <param name="value">The value of tag.</param>
    public void Set(ExifTag key, object value)
    {
        Type type = value.GetType();
        if (type.IsEnum)
        {
            Type etype = typeof(ExifEnumProperty<>).MakeGenericType(type);
            var prop = Activator.CreateInstance(etype, key, value);
            if (items.ContainsKey(key))
            {
                items.Remove(key);
            }

            if (prop is ExifProperty exifProperty)
            {
                items.Add(key, exifProperty);
            }
        }
        else
        {
            throw new ArgumentException("No exif property exists for this tag.", "value");
        }
    }

    /// <summary>
    ///     Sets the <see cref="ExifProperty" /> with the specified key.
    /// </summary>
    /// <param name="key">The tag to set.</param>
    /// <param name="value">The value of tag.</param>
    /// <param name="encoding">String encoding.</param>
    public void Set(ExifTag key, string value, Encoding encoding)
    {
        if (items.ContainsKey(key))
        {
            items.Remove(key);
        }

        items.Add(key, new ExifEncodedString(key, value, encoding));
    }

    /// <summary>
    ///     Sets the <see cref="ExifProperty" /> with the specified key.
    /// </summary>
    /// <param name="key">The tag to set.</param>
    /// <param name="value">The value of tag.</param>
    public void Set(ExifTag key, DateTime value)
    {
        if (items.ContainsKey(key))
        {
            items.Remove(key);
        }

        items.Add(key, new ExifDateTime(key, value));
    }

    /// <summary>
    ///     Sets the <see cref="ExifProperty" /> with the specified key.
    /// </summary>
    /// <param name="key">The tag to set.</param>
    /// <param name="d">Angular degrees (or clock hours for a timestamp).</param>
    /// <param name="m">Angular minutes (or clock minutes for a timestamp).</param>
    /// <param name="s">Angular seconds (or clock seconds for a timestamp).</param>
    public void Set(ExifTag key, float d, float m, float s)
    {
        if (items.ContainsKey(key))
        {
            items.Remove(key);
        }

        items.Add(key, new ExifURationalArray(key, new[] { new(d), new MathEx.UFraction32(m), new MathEx.UFraction32(s) }));
    }

    #endregion

    #region Instance Methods

    /// <summary>
    ///     Adds the specified item to the collection.
    /// </summary>
    /// <param name="item">The <see cref="ExifProperty" /> to add to the collection.</param>
    public void Add(ExifProperty item)
    {
        ExifProperty? oldItem = null;
        if (items.TryGetValue(item.Tag, out oldItem))
        {
            items[item.Tag] = item;
        }
        else
        {
            items.Add(item.Tag, item);
        }
    }

    /// <summary>
    ///     Removes all items from the collection.
    /// </summary>
    public void Clear() => items.Clear();

    /// <summary>
    ///     Determines whether the collection contains an element with the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the collection.</param>
    /// <returns>
    ///     true if the collection contains an element with the key; otherwise, false.
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException">
    ///     <paramref name="key" /> is null.
    /// </exception>
    public bool ContainsKey(ExifTag key) => items.ContainsKey(key);

    /// <summary>
    ///     Removes the element with the specified key from the collection.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns>
    ///     true if the element is successfully removed; otherwise, false.  This method also returns false if
    ///     <paramref name="key" /> was not found in the original collection.
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException">
    ///     <paramref name="key" /> is null.
    /// </exception>
    public bool Remove(ExifTag key) => items.Remove(key);

    /// <summary>
    ///     Removes all items with the given IFD from the collection.
    /// </summary>
    /// <param name="ifd">The IFD section to remove.</param>
    public void Remove(IFD ifd)
    {
        var toRemove = new List<ExifTag>();
        foreach (KeyValuePair<ExifTag, ExifProperty> item in items)
        {
            if (item.Value.IFD == ifd)
            {
                toRemove.Add(item.Key);
            }
        }

        foreach (ExifTag tag in toRemove)
        {
            items.Remove(tag);
        }
    }

    /// <summary>
    ///     Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose value to get.</param>
    /// <param name="value">
    ///     When this method returns, the value associated with the specified key, if the key is found;
    ///     otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed
    ///     uninitialized.
    /// </param>
    /// <returns>
    ///     true if the collection contains an element with the specified key; otherwise, false.
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException">
    ///     <paramref name="key" /> is null.
    /// </exception>
    public bool TryGetValue(ExifTag key, [MaybeNullWhen(false)] out ExifProperty value) =>
        items.TryGetValue(key, out value);

    /// <summary>
    ///     Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
    /// </returns>
    public IEnumerator<ExifProperty> GetEnumerator() => Values.GetEnumerator();

    #endregion

    #region Hidden Interface

    /// <summary>
    ///     Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
    /// </summary>
    /// <param name="key">The object to use as the key of the element to add.</param>
    /// <param name="value">The object to use as the value of the element to add.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     <paramref name="key" /> is null.
    /// </exception>
    /// <exception cref="T:System.ArgumentException">
    ///     An element with the same key already exists in the
    ///     <see cref="T:System.Collections.Generic.IDictionary`2" />.
    /// </exception>
    /// <exception cref="T:System.NotSupportedException">
    ///     The <see cref="T:System.Collections.Generic.IDictionary`2" /> is
    ///     read-only.
    /// </exception>
    void IDictionary<ExifTag, ExifProperty>.Add(ExifTag key, ExifProperty value) => Add(value);

    /// <summary>
    ///     Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
    /// </summary>
    /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
    /// <exception cref="T:System.NotSupportedException">
    ///     The <see cref="T:System.Collections.Generic.ICollection`1" /> is
    ///     read-only.
    /// </exception>
    void ICollection<KeyValuePair<ExifTag, ExifProperty>>.Add(KeyValuePair<ExifTag, ExifProperty> item) =>
        Add(item.Value);

    bool ICollection<KeyValuePair<ExifTag, ExifProperty>>.Contains(KeyValuePair<ExifTag, ExifProperty> item) =>
        throw new NotSupportedException();

    /// <summary>
    ///     Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an
    ///     <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
    /// </summary>
    /// <param name="array">
    ///     The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied
    ///     from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have
    ///     zero-based indexing.
    /// </param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     <paramref name="array" /> is null.
    /// </exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///     <paramref name="arrayIndex" /> is less than 0.
    /// </exception>
    /// <exception cref="T:System.ArgumentException">
    ///     <paramref name="array" /> is multidimensional.-or-<paramref name="arrayIndex" /> is equal to or greater than the
    ///     length of <paramref name="array" />.-or-The number of elements in the source
    ///     <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from
    ///     <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.-or-Type
    ///     <paramref name="T" /> cannot be cast automatically to the type of the destination <paramref name="array" />.
    /// </exception>
    void ICollection<KeyValuePair<ExifTag, ExifProperty>>.CopyTo(KeyValuePair<ExifTag, ExifProperty>[] array, int arrayIndex)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        if (arrayIndex < 0)
        {
            throw new ArgumentOutOfRangeException("arrayIndex");
        }

        if (array.Rank > 1)
        {
            throw new ArgumentException("Destination array is multidimensional.", "array");
        }

        if (arrayIndex >= array.Length)
        {
            throw new ArgumentException("arrayIndex is equal to or greater than the length of destination array", "array");
        }

        if (arrayIndex + items.Count > array.Length)
        {
            throw new ArgumentException("There is not enough space in destination array.", "array");
        }

        var i = 0;
        foreach (KeyValuePair<ExifTag, ExifProperty> item in items)
        {
            if (i >= arrayIndex)
            {
                array[i] = item;
            }

            i++;
        }
    }

    /// <summary>
    ///     Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
    /// </summary>
    /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
    bool ICollection<KeyValuePair<ExifTag, ExifProperty>>.IsReadOnly => false;

    /// <summary>
    ///     Removes the first occurrence of a specific object from the
    ///     <see cref="T:System.Collections.Generic.ICollection`1" />.
    /// </summary>
    /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
    /// <returns>
    ///     true if <paramref name="item" /> was successfully removed from the
    ///     <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if
    ///     <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
    /// </returns>
    /// <exception cref="T:System.NotSupportedException">
    ///     The <see cref="T:System.Collections.Generic.ICollection`1" /> is
    ///     read-only.
    /// </exception>
    bool ICollection<KeyValuePair<ExifTag, ExifProperty>>.Remove(KeyValuePair<ExifTag, ExifProperty> item) =>
        throw new NotSupportedException();

    /// <summary>
    ///     Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    ///     Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
    /// </returns>
    IEnumerator<KeyValuePair<ExifTag, ExifProperty>> IEnumerable<KeyValuePair<ExifTag, ExifProperty>>.GetEnumerator() =>
        items.GetEnumerator();

    #endregion
}
