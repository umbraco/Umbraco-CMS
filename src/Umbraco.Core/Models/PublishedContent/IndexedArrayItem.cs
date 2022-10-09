using System.Net;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Represents an item in an array that stores its own index and the total count.
/// </summary>
/// <typeparam name="TContent">The type of the content.</typeparam>
public class IndexedArrayItem<TContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="IndexedArrayItem{TContent}" /> class.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="index">The index.</param>
    public IndexedArrayItem(TContent content, int index)
    {
        Content = content;
        Index = index;
    }

    /// <summary>
    ///     Gets the content.
    /// </summary>
    /// <value>
    ///     The content.
    /// </value>
    public TContent Content { get; }

    /// <summary>
    ///     Gets the index.
    /// </summary>
    /// <value>
    ///     The index.
    /// </value>
    public int Index { get; }

    /// <summary>
    ///     Gets the total count.
    /// </summary>
    /// <value>
    ///     The total count.
    /// </value>
    public int TotalCount { get; set; }

    /// <summary>
    ///     Determines whether this item is the first.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if this item is the first; otherwise, <c>false</c>.
    /// </returns>
    public bool IsFirst() => Index == 0;

    /// <summary>
    ///     If this item is the first, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise,
    ///     <see cref="string.Empty" />.
    /// </summary>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public IHtmlEncodedString IsFirst(string valueIfTrue) => IsFirst(valueIfTrue, string.Empty);

    /// <summary>
    ///     If this item is the first, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise,
    ///     <paramref name="valueIfFalse" />.
    /// </summary>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <param name="valueIfFalse">The value if <c>false</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public IHtmlEncodedString IsFirst(string valueIfTrue, string valueIfFalse) =>
        new HtmlEncodedString(WebUtility.HtmlEncode(IsFirst() ? valueIfTrue : valueIfFalse));

    /// <summary>
    ///     Determines whether this item is not the first.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if this item is not the first; otherwise, <c>false</c>.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public bool IsNotFirst() => IsFirst() == false;

    /// <summary>
    ///     If this item is not the first, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise,
    ///     <see cref="string.Empty" />.
    /// </summary>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public IHtmlEncodedString IsNotFirst(string valueIfTrue) => IsNotFirst(valueIfTrue, string.Empty);

    /// <summary>
    ///     If this item is not the first, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise,
    ///     <paramref name="valueIfFalse" />.
    /// </summary>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <param name="valueIfFalse">The value if <c>false</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public IHtmlEncodedString IsNotFirst(string valueIfTrue, string valueIfFalse) =>
        new HtmlEncodedString(WebUtility.HtmlEncode(IsNotFirst() ? valueIfTrue : valueIfFalse));

    /// <summary>
    ///     Determines whether this item is at the specified <paramref name="index" />.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>
    ///     <c>true</c> if this item is at the specified <paramref name="index" />; otherwise, <c>false</c>.
    /// </returns>
    public bool IsIndex(int index) => Index == index;

    /// <summary>
    ///     If this item is at the specified <paramref name="index" />, the HTML encoded <paramref name="valueIfTrue" /> will
    ///     be returned; otherwise, <see cref="string.Empty" />.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public IHtmlEncodedString IsIndex(int index, string valueIfTrue) => IsIndex(index, valueIfTrue, string.Empty);

    /// <summary>
    ///     If this item is at the specified <paramref name="index" />, the HTML encoded <paramref name="valueIfTrue" /> will
    ///     be returned; otherwise, <paramref name="valueIfFalse" />.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <param name="valueIfFalse">The value if <c>false</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public IHtmlEncodedString IsIndex(int index, string valueIfTrue, string valueIfFalse) =>
        new HtmlEncodedString(WebUtility.HtmlEncode(IsIndex(index) ? valueIfTrue : valueIfFalse));

    /// <summary>
    ///     Determines whether this item is at an index that can be divided by the specified <paramref name="modulus" />.
    /// </summary>
    /// <param name="modulus">The modulus.</param>
    /// <returns>
    ///     <c>true</c> if this item is at an index that can be divided by the specified <paramref name="modulus" />;
    ///     otherwise, <c>false</c>.
    /// </returns>
    public bool IsModZero(int modulus) => Index % modulus == 0;

    /// <summary>
    ///     If this item is at an index that can be divided by the specified <paramref name="modulus" />, the HTML encoded
    ///     <paramref name="valueIfTrue" /> will be returned; otherwise, <see cref="string.Empty" />.
    /// </summary>
    /// <param name="modulus">The modulus.</param>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public IHtmlEncodedString IsModZero(int modulus, string valueIfTrue) =>
        IsModZero(modulus, valueIfTrue, string.Empty);

    /// <summary>
    ///     If this item is at an index that can be divided by the specified <paramref name="modulus" />, the HTML encoded
    ///     <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
    /// </summary>
    /// <param name="modulus">The modulus.</param>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <param name="valueIfFalse">The value if <c>false</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public IHtmlEncodedString IsModZero(int modulus, string valueIfTrue, string valueIfFalse) =>
        new HtmlEncodedString(WebUtility.HtmlEncode(IsModZero(modulus) ? valueIfTrue : valueIfFalse));

    /// <summary>
    ///     Determines whether this item is not at an index that can be divided by the specified <paramref name="modulus" />.
    /// </summary>
    /// <param name="modulus">The modulus.</param>
    /// <returns>
    ///     <c>true</c> if this item is not at an index that can be divided by the specified <paramref name="modulus" />;
    ///     otherwise, <c>false</c>.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public bool IsNotModZero(int modulus) => IsModZero(modulus) == false;

    /// <summary>
    ///     If this item is not at an index that can be divided by the specified <paramref name="modulus" />, the HTML encoded
    ///     <paramref name="valueIfTrue" /> will be returned; otherwise, <see cref="string.Empty" />.
    /// </summary>
    /// <param name="modulus">The modulus.</param>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public IHtmlEncodedString IsNotModZero(int modulus, string valueIfTrue) =>
        IsNotModZero(modulus, valueIfTrue, string.Empty);

    /// <summary>
    ///     If this item is not at an index that can be divided by the specified <paramref name="modulus" />, the HTML encoded
    ///     <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
    /// </summary>
    /// <param name="modulus">The modulus.</param>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <param name="valueIfFalse">The value if <c>false</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public IHtmlEncodedString IsNotModZero(int modulus, string valueIfTrue, string valueIfFalse) =>
        new HtmlEncodedString(WebUtility.HtmlEncode(IsNotModZero(modulus) ? valueIfTrue : valueIfFalse));

    /// <summary>
    ///     Determines whether this item is not at the specified <paramref name="index" />.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>
    ///     <c>true</c> if this item is not at the specified <paramref name="index" />; otherwise, <c>false</c>.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public bool IsNotIndex(int index) => IsIndex(index) == false;

    /// <summary>
    ///     If this item is not at the specified <paramref name="index" />, the HTML encoded <paramref name="valueIfTrue" />
    ///     will be returned; otherwise, <see cref="string.Empty" />.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public IHtmlEncodedString IsNotIndex(int index, string valueIfTrue) => IsNotIndex(index, valueIfTrue, string.Empty);

    /// <summary>
    ///     If this item is at the specified <paramref name="index" />, the HTML encoded <paramref name="valueIfTrue" /> will
    ///     be returned; otherwise, <paramref name="valueIfFalse" />.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <param name="valueIfFalse">The value if <c>false</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public IHtmlEncodedString IsNotIndex(int index, string valueIfTrue, string valueIfFalse) =>
        new HtmlEncodedString(WebUtility.HtmlEncode(IsNotIndex(index) ? valueIfTrue : valueIfFalse));

    /// <summary>
    ///     Determines whether this item is the last.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if this item is the last; otherwise, <c>false</c>.
    /// </returns>
    public bool IsLast() => Index == TotalCount - 1;

    /// <summary>
    ///     If this item is the last, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise,
    ///     <see cref="string.Empty" />.
    /// </summary>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public IHtmlEncodedString IsLast(string valueIfTrue) => IsLast(valueIfTrue, string.Empty);

    /// <summary>
    ///     If this item is the last, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise,
    ///     <paramref name="valueIfFalse" />.
    /// </summary>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <param name="valueIfFalse">The value if <c>false</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public IHtmlEncodedString IsLast(string valueIfTrue, string valueIfFalse) =>
        new HtmlEncodedString(WebUtility.HtmlEncode(IsLast() ? valueIfTrue : valueIfFalse));

    /// <summary>
    ///     Determines whether this item is not the last.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if this item is not the last; otherwise, <c>false</c>.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public bool IsNotLast() => IsLast() == false;

    /// <summary>
    ///     If this item is not the last, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise,
    ///     <see cref="string.Empty" />.
    /// </summary>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public IHtmlEncodedString IsNotLast(string valueIfTrue) => IsNotLast(valueIfTrue, string.Empty);

    /// <summary>
    ///     If this item is not the last, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise,
    ///     <paramref name="valueIfFalse" />.
    /// </summary>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <param name="valueIfFalse">The value if <c>false</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public IHtmlEncodedString IsNotLast(string valueIfTrue, string valueIfFalse) =>
        new HtmlEncodedString(WebUtility.HtmlEncode(IsNotLast() ? valueIfTrue : valueIfFalse));

    /// <summary>
    ///     Determines whether this item is at an even index.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if this item is at an even index; otherwise, <c>false</c>.
    /// </returns>
    public bool IsEven() => Index % 2 == 0;

    /// <summary>
    ///     If this item is at an even index, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise,
    ///     <see cref="string.Empty" />.
    /// </summary>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public IHtmlEncodedString IsEven(string valueIfTrue) => IsEven(valueIfTrue, string.Empty);

    /// <summary>
    ///     If this item is at an even index, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise,
    ///     <paramref name="valueIfFalse" />.
    /// </summary>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <param name="valueIfFalse">The value if <c>false</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public IHtmlEncodedString IsEven(string valueIfTrue, string valueIfFalse) =>
        new HtmlEncodedString(WebUtility.HtmlEncode(IsEven() ? valueIfTrue : valueIfFalse));

    /// <summary>
    ///     Determines whether this item is at an odd index.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if this item is at an odd index; otherwise, <c>false</c>.
    /// </returns>
    public bool IsOdd() => Index % 2 == 1;

    /// <summary>
    ///     If this item is at an odd index, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise,
    ///     <see cref="string.Empty" />.
    /// </summary>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public IHtmlEncodedString IsOdd(string valueIfTrue) => IsOdd(valueIfTrue, string.Empty);

    /// <summary>
    ///     If this item is at an odd index, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise,
    ///     <paramref name="valueIfFalse" />.
    /// </summary>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <param name="valueIfFalse">The value if <c>false</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    // TODO: This method should be removed or moved to an extension method on HtmlHelper.
    public IHtmlEncodedString IsOdd(string valueIfTrue, string valueIfFalse) =>
        new HtmlEncodedString(WebUtility.HtmlEncode(IsOdd() ? valueIfTrue : valueIfFalse));
}
