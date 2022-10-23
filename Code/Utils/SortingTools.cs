// =============================================================================
// MIT License
// 
// Copyright (c) 2018 Valeriya Pudova (hww.github.io)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// =============================================================================

namespace XiRenameTool.Utils
{

    /// <summary>
    /// Additional sorting methods
    /// </summary>
    public static partial class SortingTools
    {
        ///--------------------------------------------------------------------
        /// <summary>Makes alphanumerical comparison of strings.</summary>
        ///
        /// <param name="x">String A.</param>
        /// <param name="y">String B.</param>
        ///
        /// <returns>An integer of comparison.</returns>
        ///--------------------------------------------------------------------

        public static int AlphanumCompare(this object x, object y)
        {
            var xx = x as RenamableObject;
            if (xx == null)
            {
                return 0;
            }
            var yy = y as RenamableObject;
            if (yy == null)
            {
                return 0;
            }

            var s1 = xx.FileName;
            if (s1 == null)
            {
                return 0;
            }
            var s2 = yy.FileName;
            if (s2 == null)
            {
                return 0;
            }

            var len1 = s1.Length;
            var len2 = s2.Length;
            var marker1 = 0;
            var marker2 = 0;

            // Walk through two the strings with two markers.
            while (marker1 < len1 && marker2 < len2)
            {
                var ch1 = s1[marker1];
                var ch2 = s2[marker2];

                // Some buffers we can build up characters in for each chunk.
                var space1 = new char[len1];
                var loc1 = 0;
                var space2 = new char[len2];
                var loc2 = 0;

                // Walk through all following characters that are digits or
                // characters in BOTH strings starting at the appropriate marker.
                // Collect char arrays.
                do
                {
                    space1[loc1++] = ch1;
                    marker1++;

                    if (marker1 < len1)
                    {
                        ch1 = s1[marker1];
                    }
                    else
                    {
                        break;
                    }
                } while (char.IsDigit(ch1) == char.IsDigit(space1[0]));

                do
                {
                    space2[loc2++] = ch2;
                    marker2++;

                    if (marker2 < len2)
                    {
                        ch2 = s2[marker2];
                    }
                    else
                    {
                        break;
                    }
                } while (char.IsDigit(ch2) == char.IsDigit(space2[0]));

                // If we have collected numbers, compare them numerically.
                // Otherwise, if we have strings, compare them alphabetically.
                var str1 = new string(space1);
                var str2 = new string(space2);

                int result;

                if (char.IsDigit(space1[0]) && char.IsDigit(space2[0]))
                {
                    var thisNumericChunk = int.Parse(str1);
                    var thatNumericChunk = int.Parse(str2);
                    result = thisNumericChunk.CompareTo(thatNumericChunk);
                }
                else
                {
                    result = str1.CompareTo(str2);
                }

                if (result != 0)
                {
                    return result;
                }
            }
            return len1 - len2;
        }
    }
}