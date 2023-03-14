using System.Collections;
using System.Text.RegularExpressions;

namespace Umbraco.Cms.Core.Strings;

/// <summary>
///     This Class implements the Difference Algorithm published in
///     "An O(ND) Difference Algorithm and its Variations" by Eugene Myers
///     Algorithmica Vol. 1 No. 2, 1986, p 251.
///     The algorithm itself is comparing 2 arrays of numbers so when comparing 2 text documents
///     each line is converted into a (hash) number. See DiffText().
///     diff.cs: A port of the algorithm to C#
///     Copyright (c) by Matthias Hertel, http://www.mathertel.de
///     This work is licensed under a BSD style license. See http://www.mathertel.de/License.aspx
/// </summary>
internal class Diff
{
    /// <summary>
    ///     Find the difference in 2 texts, comparing by text lines.
    /// </summary>
    /// <param name="textA">A-version of the text (usually the old one)</param>
    /// <param name="textB">B-version of the text (usually the new one)</param>
    /// <returns>Returns a array of Items that describe the differences.</returns>
    public static Item[] DiffText(string textA, string textB) =>
        DiffText(textA, textB, false, false, false); // DiffText

    /// <summary>
    ///     Find the difference in 2 texts, comparing by text lines.
    ///     This method uses the DiffInt internally by 1st converting the string into char codes
    ///     then uses the diff int method
    /// </summary>
    /// <param name="textA">A-version of the text (usually the old one)</param>
    /// <param name="textB">B-version of the text (usually the new one)</param>
    /// <returns>Returns a array of Items that describe the differences.</returns>
    public static Item[] DiffText1(string textA, string textB) =>
        DiffInt(DiffCharCodes(textA, false), DiffCharCodes(textB, false));

    /// <summary>
    ///     Find the difference in 2 text documents, comparing by text lines.
    ///     The algorithm itself is comparing 2 arrays of numbers so when comparing 2 text documents
    ///     each line is converted into a (hash) number. This hash-value is computed by storing all
    ///     text lines into a common Hashtable so i can find duplicates in there, and generating a
    ///     new number each time a new text line is inserted.
    /// </summary>
    /// <param name="textA">A-version of the text (usually the old one)</param>
    /// <param name="textB">B-version of the text (usually the new one)</param>
    /// <param name="trimSpace">
    ///     When set to true, all leading and trailing whitespace characters are stripped out before the
    ///     comparison is done.
    /// </param>
    /// <param name="ignoreSpace">
    ///     When set to true, all whitespace characters are converted to a single space character before
    ///     the comparison is done.
    /// </param>
    /// <param name="ignoreCase">
    ///     When set to true, all characters are converted to their lowercase equivalence before the
    ///     comparison is done.
    /// </param>
    /// <returns>Returns a array of Items that describe the differences.</returns>
    public static Item[] DiffText(string textA, string textB, bool trimSpace, bool ignoreSpace, bool ignoreCase)
    {
        // prepare the input-text and convert to comparable numbers.
        var h = new Hashtable(textA.Length + textB.Length);

        // The A-Version of the data (original data) to be compared.
        var dataA = new DiffData(DiffCodes(textA, h, trimSpace, ignoreSpace, ignoreCase));

        // The B-Version of the data (modified data) to be compared.
        var dataB = new DiffData(DiffCodes(textB, h, trimSpace, ignoreSpace, ignoreCase));

        h = null; // free up Hashtable memory (maybe)

        var max = dataA.Length + dataB.Length + 1;

        // vector for the (0,0) to (x,y) search
        var downVector = new int[(2 * max) + 2];

        // vector for the (u,v) to (N,M) search
        var upVector = new int[(2 * max) + 2];

        Lcs(dataA, 0, dataA.Length, dataB, 0, dataB.Length, downVector, upVector);

        Optimize(dataA);
        Optimize(dataB);
        return CreateDiffs(dataA, dataB);
    } // DiffText

    /// <summary>
    ///     Find the difference in 2 arrays of integers.
    /// </summary>
    /// <param name="arrayA">A-version of the numbers (usually the old one)</param>
    /// <param name="arrayB">B-version of the numbers (usually the new one)</param>
    /// <returns>Returns a array of Items that describe the differences.</returns>
    public static Item[] DiffInt(int[] arrayA, int[] arrayB)
    {
        // The A-Version of the data (original data) to be compared.
        var dataA = new DiffData(arrayA);

        // The B-Version of the data (modified data) to be compared.
        var dataB = new DiffData(arrayB);

        var max = dataA.Length + dataB.Length + 1;

        // vector for the (0,0) to (x,y) search
        var downVector = new int[(2 * max) + 2];

        // vector for the (u,v) to (N,M) search
        var upVector = new int[(2 * max) + 2];

        Lcs(dataA, 0, dataA.Length, dataB, 0, dataB.Length, downVector, upVector);
        return CreateDiffs(dataA, dataB);
    } // Diff

    /// <summary>
    ///     Diffs the char codes.
    /// </summary>
    /// <param name="aText">A text.</param>
    /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
    /// <returns></returns>
    private static int[] DiffCharCodes(string aText, bool ignoreCase)
    {
        if (ignoreCase)
        {
            aText = aText.ToUpperInvariant();
        }

        var codes = new int[aText.Length];

        for (var n = 0; n < aText.Length; n++)
        {
            codes[n] = aText[n];
        }

        return codes;
    } // DiffCharCodes

    /// <summary>
    ///     If a sequence of modified lines starts with a line that contains the same content
    ///     as the line that appends the changes, the difference sequence is modified so that the
    ///     appended line and not the starting line is marked as modified.
    ///     This leads to more readable diff sequences when comparing text files.
    /// </summary>
    /// <param name="data">A Diff data buffer containing the identified changes.</param>
    private static void Optimize(DiffData data)
    {
        var startPos = 0;
        while (startPos < data.Length)
        {
            while (startPos < data.Length && data.Modified[startPos] == false)
            {
                startPos++;
            }

            var endPos = startPos;
            while (endPos < data.Length && data.Modified[endPos])
            {
                endPos++;
            }

            if (endPos < data.Length && data.Data[startPos] == data.Data[endPos])
            {
                data.Modified[startPos] = false;
                data.Modified[endPos] = true;
            }
            else
            {
                startPos = endPos;
            } // if
        } // while
    } // Optimize

    /// <summary>
    ///     This function converts all text lines of the text into unique numbers for every unique text line
    ///     so further work can work only with simple numbers.
    /// </summary>
    /// <param name="aText">the input text</param>
    /// <param name="h">This extern initialized Hashtable is used for storing all ever used text lines.</param>
    /// <param name="trimSpace">ignore leading and trailing space characters</param>
    /// <param name="ignoreSpace"></param>
    /// <param name="ignoreCase"></param>
    /// <returns>a array of integers.</returns>
    private static int[] DiffCodes(string aText, IDictionary h, bool trimSpace, bool ignoreSpace, bool ignoreCase)
    {
        // get all codes of the text
        var lastUsedCode = h.Count;

        // strip off all cr, only use lf as text line separator.
        aText = aText.Replace("\r", string.Empty);
        var lines = aText.Split(Constants.CharArrays.LineFeed);

        var codes = new int[lines.Length];

        for (var i = 0; i < lines.Length; ++i)
        {
            var s = lines[i];
            if (trimSpace)
            {
                s = s.Trim();
            }

            if (ignoreSpace)
            {
                s = Regex.Replace(s, "\\s+", " "); // TODO: optimization: faster blank removal.
            }

            if (ignoreCase)
            {
                s = s.ToLower();
            }

            var aCode = h[s];
            if (aCode == null)
            {
                lastUsedCode++;
                h[s] = lastUsedCode;
                codes[i] = lastUsedCode;
            }
            else
            {
                codes[i] = (int)aCode;
            } // if
        } // for

        return codes;
    } // DiffCodes

    /// <summary>
    ///     This is the algorithm to find the Shortest Middle Snake (SMS).
    /// </summary>
    /// <param name="dataA">sequence A</param>
    /// <param name="lowerA">lower bound of the actual range in DataA</param>
    /// <param name="upperA">upper bound of the actual range in DataA (exclusive)</param>
    /// <param name="dataB">sequence B</param>
    /// <param name="lowerB">lower bound of the actual range in DataB</param>
    /// <param name="upperB">upper bound of the actual range in DataB (exclusive)</param>
    /// <param name="downVector">a vector for the (0,0) to (x,y) search. Passed as a parameter for speed reasons.</param>
    /// <param name="upVector">a vector for the (u,v) to (N,M) search. Passed as a parameter for speed reasons.</param>
    /// <returns>a MiddleSnakeData record containing x,y and u,v</returns>
    private static Smsrd Sms(DiffData dataA, int lowerA, int upperA, DiffData dataB, int lowerB, int upperB, int[] downVector, int[] upVector)
    {
        var max = dataA.Length + dataB.Length + 1;

        var downK = lowerA - lowerB; // the k-line to start the forward search
        var upK = upperA - upperB; // the k-line to start the reverse search

        var delta = upperA - lowerA - (upperB - lowerB);
        var oddDelta = (delta & 1) != 0;

        // The vectors in the publication accepts negative indexes. the vectors implemented here are 0-based
        // and are access using a specific offset: UpOffset UpVector and DownOffset for DownVektor
        var downOffset = max - downK;
        var upOffset = max - upK;

        var maxD = ((upperA - lowerA + upperB - lowerB) / 2) + 1;

        // Debug.Write(2, "SMS", String.Format("Search the box: A[{0}-{1}] to B[{2}-{3}]", LowerA, UpperA, LowerB, UpperB));

        // init vectors
        downVector[downOffset + downK + 1] = lowerA;
        upVector[upOffset + upK - 1] = upperA;

        for (var d = 0; d <= maxD; d++)
        {
            // Extend the forward path.
            Smsrd ret;
            for (var k = downK - d; k <= downK + d; k += 2)
            {
                // Debug.Write(0, "SMS", "extend forward path " + k.ToString());

                // find the only or better starting point
                int x, y;
                if (k == downK - d)
                {
                    x = downVector[downOffset + k + 1]; // down
                }
                else
                {
                    x = downVector[downOffset + k - 1] + 1; // a step to the right
                    if (k < downK + d && downVector[downOffset + k + 1] >= x)
                    {
                        x = downVector[downOffset + k + 1]; // down
                    }
                }

                y = x - k;

                // find the end of the furthest reaching forward D-path in diagonal k.
                while (x < upperA && y < upperB && dataA.Data[x] == dataB.Data[y])
                {
                    x++;
                    y++;
                }

                downVector[downOffset + k] = x;

                // overlap ?
                if (oddDelta && upK - d < k && k < upK + d)
                {
                    if (upVector[upOffset + k] <= downVector[downOffset + k])
                    {
                        ret.X = downVector[downOffset + k];
                        ret.Y = downVector[downOffset + k] - k;

                        // ret.u = UpVector[UpOffset + k];      // 2002.09.20: no need for 2 points
                        // ret.v = UpVector[UpOffset + k] - k;
                        return ret;
                    } // if
                } // if
            } // for k

            // Extend the reverse path.
            for (var k = upK - d; k <= upK + d; k += 2)
            {
                // Debug.Write(0, "SMS", "extend reverse path " + k.ToString());

                // find the only or better starting point
                int x, y;
                if (k == upK + d)
                {
                    x = upVector[upOffset + k - 1]; // up
                }
                else
                {
                    x = upVector[upOffset + k + 1] - 1; // left
                    if (k > upK - d && upVector[upOffset + k - 1] < x)
                    {
                        x = upVector[upOffset + k - 1]; // up
                    }
                } // if

                y = x - k;

                while (x > lowerA && y > lowerB && dataA.Data[x - 1] == dataB.Data[y - 1])
                {
                    x--;
                    y--; // diagonal
                }

                upVector[upOffset + k] = x;

                // overlap ?
                if (!oddDelta && downK - d <= k && k <= downK + d)
                {
                    if (upVector[upOffset + k] <= downVector[downOffset + k])
                    {
                        ret.X = downVector[downOffset + k];
                        ret.Y = downVector[downOffset + k] - k;

                        // ret.u = UpVector[UpOffset + k];     // 2002.09.20: no need for 2 points
                        // ret.v = UpVector[UpOffset + k] - k;
                        return ret;
                    } // if
                } // if
            } // for k
        } // for D

        throw new ApplicationException("the algorithm should never come here.");
    } // SMS

    /// <summary>
    ///     This is the divide-and-conquer implementation of the longest common-subsequence (LCS)
    ///     algorithm.
    ///     The published algorithm passes recursively parts of the A and B sequences.
    ///     To avoid copying these arrays the lower and upper bounds are passed while the sequences stay constant.
    /// </summary>
    /// <param name="dataA">sequence A</param>
    /// <param name="lowerA">lower bound of the actual range in DataA</param>
    /// <param name="upperA">upper bound of the actual range in DataA (exclusive)</param>
    /// <param name="dataB">sequence B</param>
    /// <param name="lowerB">lower bound of the actual range in DataB</param>
    /// <param name="upperB">upper bound of the actual range in DataB (exclusive)</param>
    /// <param name="downVector">a vector for the (0,0) to (x,y) search. Passed as a parameter for speed reasons.</param>
    /// <param name="upVector">a vector for the (u,v) to (N,M) search. Passed as a parameter for speed reasons.</param>
    private static void Lcs(DiffData dataA, int lowerA, int upperA, DiffData dataB, int lowerB, int upperB, int[] downVector, int[] upVector)
    {
        // Debug.Write(2, "LCS", String.Format("Analyze the box: A[{0}-{1}] to B[{2}-{3}]", LowerA, UpperA, LowerB, UpperB));

        // Fast walk through equal lines at the start
        while (lowerA < upperA && lowerB < upperB && dataA.Data[lowerA] == dataB.Data[lowerB])
        {
            lowerA++;
            lowerB++;
        }

        // Fast walk through equal lines at the end
        while (lowerA < upperA && lowerB < upperB && dataA.Data[upperA - 1] == dataB.Data[upperB - 1])
        {
            --upperA;
            --upperB;
        }

        if (lowerA == upperA)
        {
            // mark as inserted lines.
            while (lowerB < upperB)
            {
                dataB.Modified[lowerB++] = true;
            }
        }
        else if (lowerB == upperB)
        {
            // mark as deleted lines.
            while (lowerA < upperA)
            {
                dataA.Modified[lowerA++] = true;
            }
        }
        else
        {
            // Find the middle snake and length of an optimal path for A and B
            Smsrd smsrd = Sms(dataA, lowerA, upperA, dataB, lowerB, upperB, downVector, upVector);

            // Debug.Write(2, "MiddleSnakeData", String.Format("{0},{1}", smsrd.x, smsrd.y));

            // The path is from LowerX to (x,y) and (x,y) to UpperX
            Lcs(dataA, lowerA, smsrd.X, dataB, lowerB, smsrd.Y, downVector, upVector);
            Lcs(dataA, smsrd.X, upperA, dataB, smsrd.Y, upperB, downVector, upVector); // 2002.09.20: no need for 2 points
        }
    } // LCS()

    /// <summary>
    ///     Scan the tables of which lines are inserted and deleted,
    ///     producing an edit script in forward order.
    /// </summary>
    /// dynamic array
    private static Item[] CreateDiffs(DiffData dataA, DiffData dataB)
    {
        var a = new ArrayList();
        Item aItem;
        Item[] result;

        var lineA = 0;
        var lineB = 0;
        while (lineA < dataA.Length || lineB < dataB.Length)
        {
            if (lineA < dataA.Length && !dataA.Modified[lineA]
                                     && lineB < dataB.Length && !dataB.Modified[lineB])
            {
                // equal lines
                lineA++;
                lineB++;
            }
            else
            {
                // maybe deleted and/or inserted lines
                var startA = lineA;
                var startB = lineB;

                while (lineA < dataA.Length && (lineB >= dataB.Length || dataA.Modified[lineA]))

                // while (LineA < DataA.Length && DataA.modified[LineA])
                {
                    lineA++;
                }

                while (lineB < dataB.Length && (lineA >= dataA.Length || dataB.Modified[lineB]))

                // while (LineB < DataB.Length && DataB.modified[LineB])
                {
                    lineB++;
                }

                if (startA < lineA || startB < lineB)
                {
                    // store a new difference-item
                    aItem = new Item
                    {
                        StartA = startA,
                        StartB = startB,
                        DeletedA = lineA - startA,
                        InsertedB = lineB - startB,
                    };
                    a.Add(aItem);
                } // if
            } // if
        } // while

        result = new Item[a.Count];
        a.CopyTo(result);

        return result;
    }

    /// <summary>details of one difference.</summary>
    public struct Item
    {
        /// <summary>Start Line number in Data A.</summary>
        public int StartA;

        /// <summary>Start Line number in Data B.</summary>
        public int StartB;

        /// <summary>Number of changes in Data A.</summary>
        public int DeletedA;

        /// <summary>Number of changes in Data B.</summary>
        public int InsertedB;
    } // Item

    /// <summary>
    ///     Data on one input file being compared.
    /// </summary>
    internal class DiffData
    {
        /// <summary>Buffer of numbers that will be compared.</summary>
        internal int[] Data;

        /// <summary>Number of elements (lines).</summary>
        internal int Length;

        /// <summary>
        ///     Array of booleans that flag for modified data.
        ///     This is the result of the diff.
        ///     This means deletedA in the first Data or inserted in the second Data.
        /// </summary>
        internal bool[] Modified;

        /// <summary>
        ///     Initialize the Diff-Data buffer.
        /// </summary>
        /// <param name="initData">reference to the buffer</param>
        internal DiffData(int[] initData)
        {
            Data = initData;
            Length = initData.Length;
            Modified = new bool[Length + 2];
        } // DiffData
    } // class DiffData

    /// <summary>
    ///     Shortest Middle Snake Return Data
    /// </summary>
    private struct Smsrd
    {
        internal int X;
        internal int Y;

        // internal int u, v;  // 2002.09.20: no need for 2 points
    }
} // class Diff
