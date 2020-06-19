using System.Web;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Represents an item in an array that stores its own index and the total count.
    /// </summary>
    /// <typeparam name="TContent">The type of the content.</typeparam>
    public class IndexedArrayItem<TContent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IndexedArrayItem{TContent}" /> class.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="index">The index.</param>
        public IndexedArrayItem(TContent content, int index)
        {
            Content = content;
            Index = index;
        }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public TContent Content { get; }

        /// <summary>
        /// Gets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public int Index { get; }

        /// <summary>
        /// Gets the total count.
        /// </summary>
        /// <value>
        /// The total count.
        /// </value>
        public int TotalCount { get; internal set; }

        /// <summary>
        /// Determines whether this item is the first.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this item is the first; otherwise, <c>false</c>.
        /// </returns>
        public bool IsFirst()
        {
            return Index == 0;
        }

        /// <summary>
        /// If this item is the first, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <see cref="string.Empty" />.
        /// </summary>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public HtmlString IsFirst(string valueIfTrue)
        {
            return IsFirst(valueIfTrue, string.Empty);
        }

        /// <summary>
        /// If this item is the first, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
        /// </summary>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <param name="valueIfFalse">The value if <c>false</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public HtmlString IsFirst(string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(HttpUtility.HtmlEncode(IsFirst() ? valueIfTrue : valueIfFalse));
        }

        /// <summary>
        /// Determines whether this item is not the first.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this item is not the first; otherwise, <c>false</c>.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public bool IsNotFirst()
        {
            return IsFirst() == false;
        }

        /// <summary>
        /// If this item is not the first, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <see cref="string.Empty" />.
        /// </summary>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public HtmlString IsNotFirst(string valueIfTrue)
        {
            return IsNotFirst(valueIfTrue, string.Empty);
        }

        /// <summary>
        /// If this item is not the first, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
        /// </summary>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <param name="valueIfFalse">The value if <c>false</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public HtmlString IsNotFirst(string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(HttpUtility.HtmlEncode(IsNotFirst() ? valueIfTrue : valueIfFalse));
        }

        /// <summary>
        /// Determines whether this item is at the specified <paramref name="index" />.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        ///   <c>true</c> if this item is at the specified <paramref name="index" />; otherwise, <c>false</c>.
        /// </returns>
        public bool IsIndex(int index)
        {
            return Index == index;
        }

        /// <summary>
        /// If this item is at the specified <paramref name="index" />, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <see cref="string.Empty" />.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public HtmlString IsIndex(int index, string valueIfTrue)
        {
            return IsIndex(index, valueIfTrue, string.Empty);
        }

        /// <summary>
        /// If this item is at the specified <paramref name="index" />, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <param name="valueIfFalse">The value if <c>false</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public HtmlString IsIndex(int index, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(HttpUtility.HtmlEncode(IsIndex(index) ? valueIfTrue : valueIfFalse));
        }

        /// <summary>
        /// Determines whether this item is at an index that can be divided by the specified <paramref name="modulus" />.
        /// </summary>
        /// <param name="modulus">The modulus.</param>
        /// <returns>
        ///   <c>true</c> if this item is at an index that can be divided by the specified <paramref name="modulus" />; otherwise, <c>false</c>.
        /// </returns>
        public bool IsModZero(int modulus)
        {
            return Index % modulus == 0;
        }

        /// <summary>
        /// If this item is at an index that can be divided by the specified <paramref name="modulus" />, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <see cref="string.Empty" />.
        /// </summary>
        /// <param name="modulus">The modulus.</param>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public HtmlString IsModZero(int modulus, string valueIfTrue)
        {
            return IsModZero(modulus, valueIfTrue, string.Empty);
        }

        /// <summary>
        /// If this item is at an index that can be divided by the specified <paramref name="modulus" />, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
        /// </summary>
        /// <param name="modulus">The modulus.</param>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <param name="valueIfFalse">The value if <c>false</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public HtmlString IsModZero(int modulus, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(HttpUtility.HtmlEncode(IsModZero(modulus) ? valueIfTrue : valueIfFalse));
        }

        /// <summary>
        /// Determines whether this item is not at an index that can be divided by the specified <paramref name="modulus" />.
        /// </summary>
        /// <param name="modulus">The modulus.</param>
        /// <returns>
        ///   <c>true</c> if this item is not at an index that can be divided by the specified <paramref name="modulus" />; otherwise, <c>false</c>.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public bool IsNotModZero(int modulus)
        {
            return IsModZero(modulus) == false;
        }

        /// <summary>
        /// If this item is not at an index that can be divided by the specified <paramref name="modulus" />, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <see cref="string.Empty" />.
        /// </summary>
        /// <param name="modulus">The modulus.</param>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public HtmlString IsNotModZero(int modulus, string valueIfTrue)
        {
            return IsNotModZero(modulus, valueIfTrue, string.Empty);
        }

        /// <summary>
        /// If this item is not at an index that can be divided by the specified <paramref name="modulus" />, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
        /// </summary>
        /// <param name="modulus">The modulus.</param>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <param name="valueIfFalse">The value if <c>false</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public HtmlString IsNotModZero(int modulus, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(HttpUtility.HtmlEncode(IsNotModZero(modulus) ? valueIfTrue : valueIfFalse));
        }

        /// <summary>
        /// Determines whether this item is not at the specified <paramref name="index" />.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        ///   <c>true</c> if this item is not at the specified <paramref name="index" />; otherwise, <c>false</c>.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public bool IsNotIndex(int index)
        {
            return IsIndex(index) == false;
        }

        /// <summary>
        /// If this item is not at the specified <paramref name="index" />, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <see cref="string.Empty" />.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public HtmlString IsNotIndex(int index, string valueIfTrue)
        {
            return IsNotIndex(index, valueIfTrue, string.Empty);
        }

        /// <summary>
        /// If this item is at the specified <paramref name="index" />, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <param name="valueIfFalse">The value if <c>false</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public HtmlString IsNotIndex(int index, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(HttpUtility.HtmlEncode(IsNotIndex(index) ? valueIfTrue : valueIfFalse));
        }

        /// <summary>
        /// Determines whether this item is the last.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this item is the last; otherwise, <c>false</c>.
        /// </returns>
        public bool IsLast()
        {
            return Index == TotalCount - 1;
        }

        /// <summary>
        /// If this item is the last, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <see cref="string.Empty" />.
        /// </summary>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public HtmlString IsLast(string valueIfTrue)
        {
            return IsLast(valueIfTrue, string.Empty);
        }

        /// <summary>
        /// If this item is the last, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
        /// </summary>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <param name="valueIfFalse">The value if <c>false</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public HtmlString IsLast(string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(HttpUtility.HtmlEncode(IsLast() ? valueIfTrue : valueIfFalse));
        }

        /// <summary>
        /// Determines whether this item is not the last.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this item is not the last; otherwise, <c>false</c>.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public bool IsNotLast()
        {
            return IsLast() == false;
        }

        /// <summary>
        /// If this item is not the last, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <see cref="string.Empty" />.
        /// </summary>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public HtmlString IsNotLast(string valueIfTrue)
        {
            return IsNotLast(valueIfTrue, string.Empty);
        }

        /// <summary>
        /// If this item is not the last, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
        /// </summary>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <param name="valueIfFalse">The value if <c>false</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public HtmlString IsNotLast(string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(HttpUtility.HtmlEncode(IsNotLast() ? valueIfTrue : valueIfFalse));
        }

        /// <summary>
        /// Determines whether this item is at an even index.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this item is at an even index; otherwise, <c>false</c>.
        /// </returns>
        public bool IsEven()
        {
            return Index % 2 == 0;
        }

        /// <summary>
        /// If this item is at an even index, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <see cref="string.Empty" />.
        /// </summary>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public HtmlString IsEven(string valueIfTrue)
        {
            return IsEven(valueIfTrue, string.Empty);
        }

        /// <summary>
        /// If this item is at an even index, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
        /// </summary>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <param name="valueIfFalse">The value if <c>false</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public HtmlString IsEven(string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(HttpUtility.HtmlEncode(IsEven() ? valueIfTrue : valueIfFalse));
        }

        /// <summary>
        /// Determines whether this item is at an odd index.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this item is at an odd index; otherwise, <c>false</c>.
        /// </returns>
        public bool IsOdd()
        {
            return Index % 2 == 1;
        }

        /// <summary>
        /// If this item is at an odd index, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <see cref="string.Empty" />.
        /// </summary>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public HtmlString IsOdd(string valueIfTrue)
        {
            return IsOdd(valueIfTrue, string.Empty);
        }

        /// <summary>
        /// If this item is at an odd index, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
        /// </summary>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <param name="valueIfFalse">The value if <c>false</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public HtmlString IsOdd(string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(HttpUtility.HtmlEncode(IsOdd() ? valueIfTrue : valueIfFalse));
        }
    }
}
