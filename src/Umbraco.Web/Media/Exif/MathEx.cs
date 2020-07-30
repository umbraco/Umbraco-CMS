﻿using System;
using System.Text;

namespace Umbraco.Web.Media.Exif
{
    /// <summary>
    /// Contains extended Math functions.
    /// </summary>
    internal static class MathEx
    {
        /// <summary>
        /// Returns the greatest common divisor of two numbers.
        /// </summary>
        /// <param name="a">First number.</param>
        /// <param name="b">Second number.</param>
        public static uint GCD(uint a, uint b)
        {
            while (b != 0)
            {
                uint rem = a % b;
                a = b;
                b = rem;
            }

            return a;
        }

        /// <summary>
        /// Returns the greatest common divisor of two numbers.
        /// </summary>
        /// <param name="a">First number.</param>
        /// <param name="b">Second number.</param>
        public static ulong GCD(ulong a, ulong b)
        {
            while (b != 0)
            {
                ulong rem = a % b;
                a = b;
                b = rem;
            }

            return a;
        }

        /// <summary>
        /// Represents a generic rational number represented by 32-bit signed numerator and denominator.
        /// </summary>
        public struct Fraction32 : IComparable, IFormattable, IComparable<Fraction32>, IEquatable<Fraction32>
        {
            #region Constants
            private const uint MaximumIterations = 10000000;
            #endregion

            #region Member Variables
            private bool mIsNegative;
            private int mNumerator;
            private int mDenominator;
            private double mError;
            #endregion

            #region Properties
            /// <summary>
            /// Gets or sets the numerator.
            /// </summary>
            public int Numerator
            {
                get
                {
                    return (mIsNegative ? -1 : 1) * mNumerator;
                }
                set
                {
                    if (value < 0)
                    {
                        mIsNegative = true;
                        mNumerator = -1 * value;
                    }
                    else
                    {
                        mIsNegative = false;
                        mNumerator = value;
                    }
                    Reduce(ref mNumerator, ref mDenominator);
                }
            }
            /// <summary>
            /// Gets or sets the denominator.
            /// </summary>
            public int Denominator
            {
                get
                {
                    return ((int)mDenominator);
                }
                set
                {
                    mDenominator = System.Math.Abs(value);
                    Reduce(ref mNumerator, ref mDenominator);
                }
            }

            /// <summary>
            /// Gets the error term.
            /// </summary>
            public double Error
            {
                get
                {
                    return mError;
                }
            }

            /// <summary>
            /// Gets or sets a value determining id the fraction is a negative value.
            /// </summary>
            public bool IsNegative
            {
                get
                {
                    return mIsNegative;
                }
                set
                {
                    mIsNegative = value;
                }
            }
            #endregion

            #region Predefined Values
            public static readonly Fraction32 NaN = new Fraction32(0, 0);
            public static readonly Fraction32 NegativeInfinity = new Fraction32(-1, 0);
            public static readonly Fraction32 PositiveInfinity = new Fraction32(1, 0);
            #endregion

            #region Static Methods
            /// <summary>
            /// Returns a value indicating whether the specified number evaluates to a value
            /// that is not a number.
            /// </summary>
            /// <param name="f">A fraction.</param>
            /// <returns>true if f evaluates to Fraction.NaN; otherwise, false.</returns>
            public static bool IsNan(Fraction32 f)
            {
                return (f.Numerator == 0 && f.Denominator == 0);
            }
            /// <summary>
            /// Returns a value indicating whether the specified number evaluates to negative
            /// infinity.
            /// </summary>
            /// <param name="f">A fraction.</param>
            /// <returns>true if f evaluates to Fraction.NegativeInfinity; otherwise, false.</returns>
            public static bool IsNegativeInfinity(Fraction32 f)
            {
                return (f.Numerator < 0 && f.Denominator == 0);
            }
            /// <summary>
            /// Returns a value indicating whether the specified number evaluates to positive
            /// infinity.
            /// </summary>
            /// <param name="f">A fraction.</param>
            /// <returns>true if f evaluates to Fraction.PositiveInfinity; otherwise, false.</returns>
            public static bool IsPositiveInfinity(Fraction32 f)
            {
                return (f.Numerator > 0 && f.Denominator == 0);
            }
            /// <summary>
            /// Returns a value indicating whether the specified number evaluates to negative
            /// or positive infinity.
            /// </summary>
            /// <param name="f">A fraction.</param>
            /// <returns>true if f evaluates to Fraction.NegativeInfinity or Fraction.PositiveInfinity; otherwise, false.</returns>
            public static bool IsInfinity(Fraction32 f)
            {
                return (f.Denominator == 0);
            }
            /// <summary>
            /// Returns the multiplicative inverse of a given value.
            /// </summary>
            /// <param name="f">A fraction.</param>
            /// <returns>Multiplicative inverse of f.</returns>
            public static Fraction32 Inverse(Fraction32 f)
            {
                return new Fraction32(f.Denominator, f.Numerator);
            }

            /// <summary>
            /// Converts the string representation of a fraction to a fraction object.
            /// </summary>
            /// <param name="s">A string formatted as numerator/denominator</param>
            /// <returns>A fraction object converted from s.</returns>
            /// <exception cref="System.ArgumentNullException">s is null</exception>
            /// <exception cref="System.FormatException">s is not in the correct format</exception>
            /// <exception cref="System.OverflowException">
            /// s represents a number less than System.UInt32.MinValue or greater than
            /// System.UInt32.MaxValue.
            /// </exception>
            public static Fraction32 Parse(string s)
            {
                return FromString(s);
            }

            /// <summary>
            /// Converts the string representation of a fraction to a fraction object.
            /// A return value indicates whether the conversion succeeded.
            /// </summary>
            /// <param name="s">A string formatted as numerator/denominator</param>
            /// <returns>true if s was converted successfully; otherwise, false.</returns>
            public static bool TryParse(string s, out Fraction32 f)
            {
                try
                {
                    f = Parse(s);
                    return true;
                }
                catch
                {
                    f = new Fraction32();
                    return false;
                }
            }
            #endregion

            #region Operators
            #region Arithmetic Operators
            // Multiplication
            public static Fraction32 operator *(Fraction32 f, int n)
            {
                return new Fraction32(f.Numerator * n, f.Denominator * System.Math.Abs(n));
            }
            public static Fraction32 operator *(int n, Fraction32 f)
            {
                return f * n;
            }
            public static Fraction32 operator *(Fraction32 f, float n)
            {
                return new Fraction32(((float)f) * n);
            }
            public static Fraction32 operator *(float n, Fraction32 f)
            {
                return f * n;
            }
            public static Fraction32 operator *(Fraction32 f, double n)
            {
                return new Fraction32(((double)f) * n);
            }
            public static Fraction32 operator *(double n, Fraction32 f)
            {
                return f * n;
            }
            public static Fraction32 operator *(Fraction32 f1, Fraction32 f2)
            {
                return new Fraction32(f1.Numerator * f2.Numerator, f1.Denominator * f2.Denominator);
            }
            // Division
            public static Fraction32 operator /(Fraction32 f, int n)
            {
                return new Fraction32(f.Numerator / n, f.Denominator / System.Math.Abs(n));
            }
            public static Fraction32 operator /(Fraction32 f, float n)
            {
                return new Fraction32(((float)f) / n);
            }
            public static Fraction32 operator /(Fraction32 f, double n)
            {
                return new Fraction32(((double)f) / n);
            }
            public static Fraction32 operator /(Fraction32 f1, Fraction32 f2)
            {
                return f1 * Inverse(f2);
            }
            // Addition
            public static Fraction32 operator +(Fraction32 f, int n)
            {
                return f + new Fraction32(n, 1);
            }
            public static Fraction32 operator +(int n, Fraction32 f)
            {
                return f + n;
            }
            public static Fraction32 operator +(Fraction32 f, float n)
            {
                return new Fraction32(((float)f) + n);
            }
            public static Fraction32 operator +(float n, Fraction32 f)
            {
                return f + n;
            }
            public static Fraction32 operator +(Fraction32 f, double n)
            {
                return new Fraction32(((double)f) + n);
            }
            public static Fraction32 operator +(double n, Fraction32 f)
            {
                return f + n;
            }
            public static Fraction32 operator +(Fraction32 f1, Fraction32 f2)
            {
                int n1 = f1.Numerator, d1 = f1.Denominator;
                int n2 = f2.Numerator, d2 = f2.Denominator;

                return new Fraction32(n1 * d2 + n2 * d1, d1 * d2);
            }
            // Subtraction
            public static Fraction32 operator -(Fraction32 f, int n)
            {
                return f - new Fraction32(n, 1);
            }
            public static Fraction32 operator -(int n, Fraction32 f)
            {
                return new Fraction32(n, 1) - f;
            }
            public static Fraction32 operator -(Fraction32 f, float n)
            {
                return new Fraction32(((float)f) - n);
            }
            public static Fraction32 operator -(float n, Fraction32 f)
            {
                return new Fraction32(n) - f;
            }
            public static Fraction32 operator -(Fraction32 f, double n)
            {
                return new Fraction32(((double)f) - n);
            }
            public static Fraction32 operator -(double n, Fraction32 f)
            {
                return new Fraction32(n) - f;
            }
            public static Fraction32 operator -(Fraction32 f1, Fraction32 f2)
            {
                int n1 = f1.Numerator, d1 = f1.Denominator;
                int n2 = f2.Numerator, d2 = f2.Denominator;

                return new Fraction32(n1 * d2 - n2 * d1, d1 * d2);
            }
            // Increment
            public static Fraction32 operator ++(Fraction32 f)
            {
                return f + new Fraction32(1, 1);
            }
            // Decrement
            public static Fraction32 operator --(Fraction32 f)
            {
                return f - new Fraction32(1, 1);
            }
            #endregion
            #region Casts To Integral Types
            public static explicit operator int(Fraction32 f)
            {
                return f.Numerator / f.Denominator;
            }
            public static explicit operator float(Fraction32 f)
            {
                return ((float)f.Numerator) / ((float)f.Denominator);
            }
            public static explicit operator double(Fraction32 f)
            {
                return ((double)f.Numerator) / ((double)f.Denominator);
            }
            #endregion
            #region Comparison Operators
            public static bool operator ==(Fraction32 f1, Fraction32 f2)
            {
                return (f1.Numerator == f2.Numerator) && (f1.Denominator == f2.Denominator);
            }
            public static bool operator !=(Fraction32 f1, Fraction32 f2)
            {
                return (f1.Numerator != f2.Numerator) || (f1.Denominator != f2.Denominator);
            }
            public static bool operator <(Fraction32 f1, Fraction32 f2)
            {
                return (f1.Numerator * f2.Denominator) < (f2.Numerator * f1.Denominator);
            }
            public static bool operator >(Fraction32 f1, Fraction32 f2)
            {
                return (f1.Numerator * f2.Denominator) > (f2.Numerator * f1.Denominator);
            }
            #endregion
            #endregion

            #region Constructors
            private Fraction32(int numerator, int denominator, double error)
            {
                mIsNegative = false;
                if (numerator < 0)
                {
                    numerator = -numerator;
                    mIsNegative = !mIsNegative;
                }
                if (denominator < 0)
                {
                    denominator = -denominator;
                    mIsNegative = !mIsNegative;
                }

                mNumerator = numerator;
                mDenominator = denominator;
                mError = error;

                if (mDenominator != 0)
                    Reduce(ref mNumerator, ref mDenominator);
            }

            public Fraction32(int numerator, int denominator)
                : this(numerator, denominator, 0)
            {

            }

            public Fraction32(int numerator)
                : this(numerator, (int)1)
            {

            }

            public Fraction32(Fraction32 f)
                : this(f.Numerator, f.Denominator, f.Error)
            {

            }

            public Fraction32(float value)
                : this((double)value)
            {

            }

            public Fraction32(double value)
                : this(FromDouble(value))
            {

            }

            public Fraction32(string s)
                : this(FromString(s))
            {

            }
            #endregion

            #region Instance Methods
            /// <summary>
            /// Sets the value of this instance to the fraction represented
            /// by the given numerator and denominator.
            /// </summary>
            /// <param name="numerator">The new numerator.</param>
            /// <param name="denominator">The new denominator.</param>
            public void Set(int numerator, int denominator)
            {
                mIsNegative = false;
                if (numerator < 0)
                {
                    mIsNegative = !mIsNegative;
                    numerator = -numerator;
                }
                if (denominator < 0)
                {
                    mIsNegative = !mIsNegative;
                    denominator = -denominator;
                }

                mNumerator = numerator;
                mDenominator = denominator;

                if (mDenominator != 0)
                    Reduce(ref mNumerator, ref mDenominator);
            }

            /// <summary>
            /// Indicates whether this instance and a specified object are equal value-wise.
            /// </summary>
            /// <param name="obj">Another object to compare to.</param>
            /// <returns>true if obj and this instance are the same type and represent
            /// the same value; otherwise, false.</returns>
            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;

                if (obj is Fraction32)
                    return Equals((Fraction32)obj);
                else
                    return false;
            }

            /// <summary>
            /// Indicates whether this instance and a specified object are equal value-wise.
            /// </summary>
            /// <param name="obj">Another fraction object to compare to.</param>
            /// <returns>true if obj and this instance represent the same value;
            /// otherwise, false.</returns>
            public bool Equals(Fraction32 obj)
            {
                return (mIsNegative == obj.IsNegative) && (mNumerator == obj.Numerator) && (mDenominator == obj.Denominator);
            }

            /// <summary>
            /// Returns the hash code for this instance.
            /// </summary>
            /// <returns> A 32-bit signed integer that is the hash code for this instance.</returns>
            public override int GetHashCode()
            {
                return mDenominator ^ ((mIsNegative ? -1 : 1) * mNumerator);
            }

            /// <summary>
            /// Returns a string representation of the fraction.
            /// </summary>
            /// <param name="format">A numeric format string.</param>
            /// <param name="formatProvider">
            /// An System.IFormatProvider that supplies culture-specific
            /// formatting information.
            /// </param>
            /// <returns>
            /// The string representation of the value of this instance as
            /// specified by format and provider.
            /// </returns>
            /// <exception cref="System.FormatException">
            /// format is invalid or not supported.
            /// </exception>
            public string ToString(string format, IFormatProvider formatProvider)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(((mIsNegative ? -1 : 1) * mNumerator).ToString(format, formatProvider));
                sb.Append('/');
                sb.Append(mDenominator.ToString(format, formatProvider));
                return sb.ToString();
            }

            /// <summary>
            /// Returns a string representation of the fraction.
            /// </summary>
            /// <param name="format">A numeric format string.</param>
            /// <returns>
            /// The string representation of the value of this instance as
            /// specified by format.
            /// </returns>
            /// <exception cref="System.FormatException">
            /// format is invalid or not supported.
            /// </exception>
            public string ToString(string format)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(((mIsNegative ? -1 : 1) * mNumerator).ToString(format));
                sb.Append('/');
                sb.Append(mDenominator.ToString(format));
                return sb.ToString();
            }

            /// <summary>
            /// Returns a string representation of the fraction.
            /// </summary>
            /// <param name="formatProvider">
            /// An System.IFormatProvider that supplies culture-specific
            /// formatting information.
            /// </param>
            /// <returns>
            /// The string representation of the value of this instance as
            /// specified by provider.
            /// </returns>
            public string ToString(IFormatProvider formatProvider)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(((mIsNegative ? -1 : 1) * mNumerator).ToString(formatProvider));
                sb.Append('/');
                sb.Append(mDenominator.ToString(formatProvider));
                return sb.ToString();
            }

            /// <summary>
            /// Returns a string representation of the fraction.
            /// </summary>
            /// <returns>A string formatted as numerator/denominator.</returns>
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(((mIsNegative ? -1 : 1) * mNumerator).ToString());
                sb.Append('/');
                sb.Append(mDenominator.ToString());
                return sb.ToString();
            }

            /// <summary>
            /// Compares this instance to a specified object and returns an indication of
            /// their relative values.
            /// </summary>
            /// <param name="obj">An object to compare, or null.</param>
            /// <returns>
            /// A signed number indicating the relative values of this instance and value.
            /// Less than zero: This instance is less than obj.
            /// Zero: This instance is equal to obj.
            /// Greater than zero: This instance is greater than obj or obj is null.
            /// </returns>
            /// <exception cref="System.ArgumentException">obj is not a Fraction.</exception>
            public int CompareTo(object obj)
            {
                if (!(obj is Fraction32))
                    throw new ArgumentException("obj must be of type Fraction", "obj");

                return CompareTo((Fraction32)obj);
            }

            /// <summary>
            /// Compares this instance to a specified object and returns an indication of
            /// their relative values.
            /// </summary>
            /// <param name="obj">An fraction to compare with this instance.</param>
            /// <returns>
            /// A signed number indicating the relative values of this instance and value.
            /// Less than zero: This instance is less than obj.
            /// Zero: This instance is equal to obj.
            /// Greater than zero: This instance is greater than obj or obj is null.
            /// </returns>
            public int CompareTo(Fraction32 obj)
            {
                if (this < obj)
                    return -1;
                else if (this > obj)
                    return 1;
                return 0;
            }
            #endregion

            #region Private Helper Methods
            /// <summary>
            /// Converts the given floating-point number to its rational representation.
            /// </summary>
            /// <param name="value">The floating-point number to be converted.</param>
            /// <returns>The rational representation of value.</returns>
            private static Fraction32 FromDouble(double value)
            {
                if (double.IsNaN(value))
                    return Fraction32.NaN;
                else if (double.IsNegativeInfinity(value))
                    return Fraction32.NegativeInfinity;
                else if (double.IsPositiveInfinity(value))
                    return Fraction32.PositiveInfinity;

                bool isneg = (value < 0);
                if (isneg) value = -value;

                double f = value;
                double forg = f;
                int lnum = 0;
                int lden = 1;
                int num = 1;
                int den = 0;
                double lasterr = 1.0;
                int a = 0;
                int currIteration = 0;
                while (true)
                {
                    if (++currIteration > MaximumIterations) break;

                    a = (int)Math.Floor(f);
                    f = f - (double)a;
                    if (Math.Abs(f) < double.Epsilon)
                        break;
                    f = 1.0 / f;
                    if (double.IsInfinity(f))
                        break;
                    int cnum = num * a + lnum;
                    int cden = den * a + lden;
                    if (Math.Abs((double)cnum / (double)cden - forg) < double.Epsilon)
                        break;
                    double err = ((double)cnum / (double)cden - (double)num / (double)den) / ((double)num / (double)den);
                    // Are we converging?
                    if (err >= lasterr)
                        break;
                    lasterr = err;
                    lnum = num;
                    lden = den;
                    num = cnum;
                    den = cden;
                }

                if (den > 0)
                    lasterr = value - ((double)num / (double)den);
                else
                    lasterr = double.PositiveInfinity;

                return new Fraction32((isneg ? -1 : 1) * num, den, lasterr);
            }

            /// <summary>Converts the string representation of a fraction to a Fraction type.</summary>
            /// <param name="s">The input string formatted as numerator/denominator.</param>
            /// <exception cref="System.ArgumentNullException">s is null.</exception>
            /// <exception cref="System.FormatException">s is not formatted as numerator/denominator.</exception>
            /// <exception cref="System.OverflowException">
            /// s represents numbers less than System.Int32.MinValue or greater than
            /// System.Int32.MaxValue.
            /// </exception>
            private static Fraction32 FromString(string s)
            {
                if (s == null)
                    throw new ArgumentNullException("s");

                string[] sa = s.Split('/');
                int numerator = 1;
                int denominator = 1;

                if (sa.Length == 1)
                {
                    // Try to parse as int
                    if (int.TryParse(sa[0], out numerator))
                    {
                        denominator = 1;
                    }
                    else
                    {
                        // Parse as double
                        double dval = double.Parse(sa[0]);
                        return FromDouble(dval);
                    }
                }
                else if (sa.Length == 2)
                {
                    numerator = int.Parse(sa[0]);
                    denominator = int.Parse(sa[1]);
                }
                else
                    throw new FormatException("The input string must be formatted as n/d where n and d are integers");

                return new Fraction32(numerator, denominator);
            }

            /// <summary>
            /// Reduces the given numerator and denominator by dividing with their
            /// greatest common divisor.
            /// </summary>
            /// <param name="numerator">numerator to be reduced.</param>
            /// <param name="denominator">denominator to be reduced.</param>
            private static void Reduce(ref int numerator, ref int denominator)
            {
                uint gcd = MathEx.GCD((uint)numerator, (uint)denominator);
                if (gcd == 0) gcd = 1;
                numerator = numerator / (int)gcd;
                denominator = denominator / (int)gcd;
            }
            #endregion
        }

        /// <summary>
        /// Represents a generic rational number represented by 32-bit unsigned numerator and denominator.
        /// </summary>
        public struct UFraction32 : IComparable, IFormattable, IComparable<UFraction32>, IEquatable<UFraction32>
        {
            #region Constants
            private const uint MaximumIterations = 10000000;
            #endregion

            #region Member Variables
            private uint mNumerator;
            private uint mDenominator;
            private double mError;
            #endregion

            #region Properties
            /// <summary>
            /// Gets or sets the numerator.
            /// </summary>
            public uint Numerator
            {
                get
                {
                    return mNumerator;
                }
                set
                {
                    mNumerator = value;
                    Reduce(ref mNumerator, ref mDenominator);
                }
            }
            /// <summary>
            /// Gets or sets the denominator.
            /// </summary>
            public uint Denominator
            {
                get
                {
                    return mDenominator;
                }
                set
                {
                    mDenominator = value;
                    Reduce(ref mNumerator, ref mDenominator);
                }
            }


            /// <summary>
            /// Gets the error term.
            /// </summary>
            public double Error
            {
                get
                {
                    return mError;
                }
            }
            #endregion

            #region Predefined Values
            public static readonly UFraction32 NaN = new UFraction32(0, 0);
            public static readonly UFraction32 Infinity = new UFraction32(1, 0);
            #endregion

            #region Static Methods
            /// <summary>
            /// Returns a value indicating whether the specified number evaluates to a value
            /// that is not a number.
            /// </summary>
            /// <param name="f">A fraction.</param>
            /// <returns>true if f evaluates to Fraction.NaN; otherwise, false.</returns>
            public static bool IsNan(UFraction32 f)
            {
                return (f.Numerator == 0 && f.Denominator == 0);
            }
            /// <summary>
            /// Returns a value indicating whether the specified number evaluates to infinity.
            /// </summary>
            /// <param name="f">A fraction.</param>
            /// <returns>true if f evaluates to Fraction.Infinity; otherwise, false.</returns>
            public static bool IsInfinity(UFraction32 f)
            {
                return (f.Denominator == 0);
            }

            /// <summary>
            /// Converts the string representation of a fraction to a fraction object.
            /// </summary>
            /// <param name="s">A string formatted as numerator/denominator</param>
            /// <returns>A fraction object converted from s.</returns>
            /// <exception cref="System.ArgumentNullException">s is null</exception>
            /// <exception cref="System.FormatException">s is not in the correct format</exception>
            /// <exception cref="System.OverflowException">
            /// s represents a number less than System.UInt32.MinValue or greater than
            /// System.UInt32.MaxValue.
            /// </exception>
            public static UFraction32 Parse(string s)
            {
                return FromString(s);
            }

            /// <summary>
            /// Converts the string representation of a fraction to a fraction object.
            /// A return value indicates whether the conversion succeeded.
            /// </summary>
            /// <param name="s">A string formatted as numerator/denominator</param>
            /// <returns>true if s was converted successfully; otherwise, false.</returns>
            public static bool TryParse(string s, out UFraction32 f)
            {
                try
                {
                    f = Parse(s);
                    return true;
                }
                catch
                {
                    f = new UFraction32();
                    return false;
                }
            }
            #endregion

            #region Operators
            #region Arithmetic Operators
            // Multiplication
            public static UFraction32 operator *(UFraction32 f, uint n)
            {
                return new UFraction32(f.Numerator * n, f.Denominator * n);
            }
            public static UFraction32 operator *(uint n, UFraction32 f)
            {
                return f * n;
            }
            public static UFraction32 operator *(UFraction32 f, float n)
            {
                return new UFraction32(((float)f) * n);
            }
            public static UFraction32 operator *(float n, UFraction32 f)
            {
                return f * n;
            }
            public static UFraction32 operator *(UFraction32 f, double n)
            {
                return new UFraction32(((double)f) * n);
            }
            public static UFraction32 operator *(double n, UFraction32 f)
            {
                return f * n;
            }
            public static UFraction32 operator *(UFraction32 f1, UFraction32 f2)
            {
                return new UFraction32(f1.Numerator * f2.Numerator, f1.Denominator * f2.Denominator);
            }
            // Division
            public static UFraction32 operator /(UFraction32 f, uint n)
            {
                return new UFraction32(f.Numerator / n, f.Denominator / n);
            }
            public static UFraction32 operator /(UFraction32 f, float n)
            {
                return new UFraction32(((float)f) / n);
            }
            public static UFraction32 operator /(UFraction32 f, double n)
            {
                return new UFraction32(((double)f) / n);
            }
            public static UFraction32 operator /(UFraction32 f1, UFraction32 f2)
            {
                return f1 * Inverse(f2);
            }
            // Addition
            public static UFraction32 operator +(UFraction32 f, uint n)
            {
                return f + new UFraction32(n, 1);
            }
            public static UFraction32 operator +(uint n, UFraction32 f)
            {
                return f + n;
            }
            public static UFraction32 operator +(UFraction32 f, float n)
            {
                return new UFraction32(((float)f) + n);
            }
            public static UFraction32 operator +(float n, UFraction32 f)
            {
                return f + n;
            }
            public static UFraction32 operator +(UFraction32 f, double n)
            {
                return new UFraction32(((double)f) + n);
            }
            public static UFraction32 operator +(double n, UFraction32 f)
            {
                return f + n;
            }
            public static UFraction32 operator +(UFraction32 f1, UFraction32 f2)
            {
                uint n1 = f1.Numerator, d1 = f1.Denominator;
                uint n2 = f2.Numerator, d2 = f2.Denominator;

                return new UFraction32(n1 * d2 + n2 * d1, d1 * d2);
            }
            // Subtraction
            public static UFraction32 operator -(UFraction32 f, uint n)
            {
                return f - new UFraction32(n, 1);
            }
            public static UFraction32 operator -(uint n, UFraction32 f)
            {
                return new UFraction32(n, 1) - f;
            }
            public static UFraction32 operator -(UFraction32 f, float n)
            {
                return new UFraction32(((float)f) - n);
            }
            public static UFraction32 operator -(float n, UFraction32 f)
            {
                return new UFraction32(n) - f;
            }
            public static UFraction32 operator -(UFraction32 f, double n)
            {
                return new UFraction32(((double)f) - n);
            }
            public static UFraction32 operator -(double n, UFraction32 f)
            {
                return new UFraction32(n) - f;
            }
            public static UFraction32 operator -(UFraction32 f1, UFraction32 f2)
            {
                uint n1 = f1.Numerator, d1 = f1.Denominator;
                uint n2 = f2.Numerator, d2 = f2.Denominator;

                return new UFraction32(n1 * d2 - n2 * d1, d1 * d2);
            }
            // Increment
            public static UFraction32 operator ++(UFraction32 f)
            {
                return f + new UFraction32(1, 1);
            }
            // Decrement
            public static UFraction32 operator --(UFraction32 f)
            {
                return f - new UFraction32(1, 1);
            }
            #endregion
            #region Casts To Integral Types
            public static explicit operator uint(UFraction32 f)
            {
                return ((uint)f.Numerator) / ((uint)f.Denominator);
            }
            public static explicit operator float(UFraction32 f)
            {
                return ((float)f.Numerator) / ((float)f.Denominator);
            }
            public static explicit operator double(UFraction32 f)
            {
                return ((double)f.Numerator) / ((double)f.Denominator);
            }
            #endregion
            #region Comparison Operators
            public static bool operator ==(UFraction32 f1, UFraction32 f2)
            {
                return (f1.Numerator == f2.Numerator) && (f1.Denominator == f2.Denominator);
            }
            public static bool operator !=(UFraction32 f1, UFraction32 f2)
            {
                return (f1.Numerator != f2.Numerator) || (f1.Denominator != f2.Denominator);
            }
            public static bool operator <(UFraction32 f1, UFraction32 f2)
            {
                return (f1.Numerator * f2.Denominator) < (f2.Numerator * f1.Denominator);
            }
            public static bool operator >(UFraction32 f1, UFraction32 f2)
            {
                return (f1.Numerator * f2.Denominator) > (f2.Numerator * f1.Denominator);
            }
            #endregion
            #endregion

            #region Constructors
            public UFraction32(uint numerator, uint denominator, double error)
            {
                mNumerator = numerator;
                mDenominator = denominator;
                mError = error;

                if (mDenominator != 0)
                    Reduce(ref mNumerator, ref mDenominator);
            }

            public UFraction32(uint numerator, uint denominator)
                : this(numerator, denominator, 0)
            {

            }

            public UFraction32(uint numerator)
                : this(numerator, (uint)1)
            {

            }

            public UFraction32(UFraction32 f)
                : this(f.Numerator, f.Denominator, f.Error)
            {

            }

            public UFraction32(float value)
                : this((double)value)
            {

            }

            public UFraction32(double value)
                : this(FromDouble(value))
            {

            }

            public UFraction32(string s)
                : this(FromString(s))
            {

            }
            #endregion

            #region Instance Methods
            /// <summary>
            /// Sets the value of this instance to the fraction represented
            /// by the given numerator and denominator.
            /// </summary>
            /// <param name="numerator">The new numerator.</param>
            /// <param name="denominator">The new denominator.</param>
            public void Set(uint numerator, uint denominator)
            {
                mNumerator = numerator;
                mDenominator = denominator;

                if (mDenominator != 0)
                    Reduce(ref mNumerator, ref mDenominator);
            }

            /// <summary>
            /// Returns the multiplicative inverse of a given value.
            /// </summary>
            /// <param name="f">A fraction.</param>
            /// <returns>Multiplicative inverse of f.</returns>
            public static UFraction32 Inverse(UFraction32 f)
            {
                return new UFraction32(f.Denominator, f.Numerator);
            }

            /// <summary>
            /// Indicates whether this instance and a specified object are equal value-wise.
            /// </summary>
            /// <param name="obj">Another object to compare to.</param>
            /// <returns>true if obj and this instance are the same type and represent
            /// the same value; otherwise, false.</returns>
            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;

                if (obj is UFraction32)
                    return Equals((UFraction32)obj);
                else
                    return false;
            }

            /// <summary>
            /// Indicates whether this instance and a specified object are equal value-wise.
            /// </summary>
            /// <param name="obj">Another fraction object to compare to.</param>
            /// <returns>true if obj and this instance represent the same value;
            /// otherwise, false.</returns>
            public bool Equals(UFraction32 obj)
            {
                return (mNumerator == obj.Numerator) && (mDenominator == obj.Denominator);
            }

            /// <summary>
            /// Returns the hash code for this instance.
            /// </summary>
            /// <returns> A 32-bit signed integer that is the hash code for this instance.</returns>
            public override int GetHashCode()
            {
                return ((int)mDenominator) ^ ((int)mNumerator);
            }

            /// <summary>
            /// Returns a string representation of the fraction.
            /// </summary>
            /// <param name="format">A numeric format string.</param>
            /// <param name="formatProvider">
            /// An System.IFormatProvider that supplies culture-specific
            /// formatting information.
            /// </param>
            /// <returns>
            /// The string representation of the value of this instance as
            /// specified by format and provider.
            /// </returns>
            /// <exception cref="System.FormatException">
            /// format is invalid or not supported.
            /// </exception>
            public string ToString(string format, IFormatProvider formatProvider)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(mNumerator.ToString(format, formatProvider));
                sb.Append('/');
                sb.Append(mDenominator.ToString(format, formatProvider));
                return sb.ToString();
            }

            /// <summary>
            /// Returns a string representation of the fraction.
            /// </summary>
            /// <param name="format">A numeric format string.</param>
            /// <returns>
            /// The string representation of the value of this instance as
            /// specified by format.
            /// </returns>
            /// <exception cref="System.FormatException">
            /// format is invalid or not supported.
            /// </exception>
            public string ToString(string format)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(mNumerator.ToString(format));
                sb.Append('/');
                sb.Append(mDenominator.ToString(format));
                return sb.ToString();
            }

            /// <summary>
            /// Returns a string representation of the fraction.
            /// </summary>
            /// <param name="formatProvider">
            /// An System.IFormatProvider that supplies culture-specific
            /// formatting information.
            /// </param>
            /// <returns>
            /// The string representation of the value of this instance as
            /// specified by provider.
            /// </returns>
            public string ToString(IFormatProvider formatProvider)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(mNumerator.ToString(formatProvider));
                sb.Append('/');
                sb.Append(mDenominator.ToString(formatProvider));
                return sb.ToString();
            }

            /// <summary>
            /// Returns a string representation of the fraction.
            /// </summary>
            /// <returns>A string formatted as numerator/denominator.</returns>
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(mNumerator.ToString());
                sb.Append('/');
                sb.Append(mDenominator.ToString());
                return sb.ToString();
            }

            /// <summary>
            /// Compares this instance to a specified object and returns an indication of
            /// their relative values.
            /// </summary>
            /// <param name="obj">An object to compare, or null.</param>
            /// <returns>
            /// A signed number indicating the relative values of this instance and value.
            /// Less than zero: This instance is less than obj.
            /// Zero: This instance is equal to obj.
            /// Greater than zero: This instance is greater than obj or obj is null.
            /// </returns>
            /// <exception cref="System.ArgumentException">obj is not a Fraction.</exception>
            public int CompareTo(object obj)
            {
                if (!(obj is UFraction32))
                    throw new ArgumentException("obj must be of type UFraction32", "obj");

                return CompareTo((UFraction32)obj);
            }

            /// <summary>
            /// Compares this instance to a specified object and returns an indication of
            /// their relative values.
            /// </summary>
            /// <param name="obj">An fraction to compare with this instance.</param>
            /// <returns>
            /// A signed number indicating the relative values of this instance and value.
            /// Less than zero: This instance is less than obj.
            /// Zero: This instance is equal to obj.
            /// Greater than zero: This instance is greater than obj or obj is null.
            /// </returns>
            public int CompareTo(UFraction32 obj)
            {
                if (this < obj)
                    return -1;
                else if (this > obj)
                    return 1;
                return 0;
            }
            #endregion

            #region Private Helper Methods
            /// <summary>
            /// Converts the given floating-point number to its rational representation.
            /// </summary>
            /// <param name="value">The floating-point number to be converted.</param>
            /// <returns>The rational representation of value.</returns>
            private static UFraction32 FromDouble(double value)
            {
                if (value < 0)
                    throw new ArgumentException("value cannot be negative.", "value");

                if (double.IsNaN(value))
                    return UFraction32.NaN;
                else if (double.IsInfinity(value))
                    return UFraction32.Infinity;

                double f = value;
                double forg = f;
                uint lnum = 0;
                uint lden = 1;
                uint num = 1;
                uint den = 0;
                double lasterr = 1.0;
                uint a = 0;
                int currIteration = 0;
                while (true)
                {
                    if (++currIteration > MaximumIterations) break;

                    a = (uint)Math.Floor(f);
                    f = f - (double)a;
                    if (Math.Abs(f) < double.Epsilon)
                        break;
                    f = 1.0 / f;
                    if (double.IsInfinity(f))
                        break;
                    uint cnum = num * a + lnum;
                    uint cden = den * a + lden;
                    if (Math.Abs((double)cnum / (double)cden - forg) < double.Epsilon)
                        break;
                    double err = ((double)cnum / (double)cden - (double)num / (double)den) / ((double)num / (double)den);
                    // Are we converging?
                    if (err >= lasterr)
                        break;
                    lasterr = err;
                    lnum = num;
                    lden = den;
                    num = cnum;
                    den = cden;
                }
                uint fnum = num * a + lnum;
                uint fden = den * a + lden;

                if (fden > 0)
                    lasterr = value - ((double)fnum / (double)fden);
                else
                    lasterr = double.PositiveInfinity;

                return new UFraction32(fnum, fden, lasterr);
            }

            /// <summary>Converts the string representation of a fraction to a Fraction type.</summary>
            /// <param name="s">The input string formatted as numerator/denominator.</param>
            /// <exception cref="System.ArgumentNullException">s is null.</exception>
            /// <exception cref="System.FormatException">s is not formatted as numerator/denominator.</exception>
            /// <exception cref="System.OverflowException">
            /// s represents numbers less than System.UInt32.MinValue or greater than
            /// System.UInt32.MaxValue.
            /// </exception>
            private static UFraction32 FromString(string s)
            {
                if (s == null)
                    throw new ArgumentNullException("s");

                string[] sa = s.Split('/');
                uint numerator = 1;
                uint denominator = 1;

                if (sa.Length == 1)
                {
                    // Try to parse as uint
                    if (uint.TryParse(sa[0], out numerator))
                    {
                        denominator = 1;
                    }
                    else
                    {
                        // Parse as double
                        double dval = double.Parse(sa[0]);
                        return FromDouble(dval);
                    }
                }
                else if (sa.Length == 2)
                {
                    numerator = uint.Parse(sa[0]);
                    denominator = uint.Parse(sa[1]);
                }
                else
                    throw new FormatException("The input string must be formatted as n/d where n and d are integers");

                return new UFraction32(numerator, denominator);
            }

            /// <summary>
            /// Reduces the given numerator and denominator by dividing with their
            /// greatest common divisor.
            /// </summary>
            /// <param name="numerator">numerator to be reduced.</param>
            /// <param name="denominator">denominator to be reduced.</param>
            private static void Reduce(ref uint numerator, ref uint denominator)
            {
                uint gcd = MathEx.GCD(numerator, denominator);
                numerator = numerator / gcd;
                denominator = denominator / gcd;
            }
            #endregion
        }
    }
}
