using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace CabralRodrigo.Util.SqlServerInsertGenerator
{
    internal static class Extensions
    {
        /// <summary>
        /// Appends repeatedly the <paramref name="charToFill"/> char to the <paramref name="source"/> string until the <paramref name="maxLength"/> length is reached.
        /// </summary>
        /// <param name="source">The string to fill until the max length</param>
        /// <param name="charToFill">The char used to fill the string until the max length</param>
        /// <param name="maxLength">The max length of the return string</param>
        /// <returns>The new string</returns>
        public static string AppendUtilLength(this string source, char charToFill, int maxLength)
        {
            if (source.Length >= maxLength)
                return source;
            else
                return source + new string(charToFill, maxLength - source.Length);
        }

        /// <summary>
        /// Use the <paramref name="adapter"/> the fill a <seealso cref="DataSet"/> using the <seealso cref="IDataAdapter.Fill(DataSet)"/> method.
        /// </summary>
        /// <param name="adapter">The <seealso cref="SqlDataAdapter"/> to fill a new <seealso cref="DataSet"/>.</param>
        /// <returns>The filled <seealso cref="DataSet"/>.</returns>
        public static DataSet GetFilledDataSet(this SqlDataAdapter adapter)
        {
            var ds = new DataSet();
            adapter.Fill(ds);

            return ds;
        }

        /// <summary>
        /// Gets the length of the longest string in the list
        /// </summary>
        /// <typeparam name="T">The type of the item in the <paramref name="source"/> <seealso cref="IEnumerable{T}"/></typeparam>
        /// <param name="source">The source <seealso cref="IEnumerable{T}"/> to get the strings</param>
        /// <param name="selector">The selector que gets the strings in the list</param>
        /// <returns>The length of the longest string in the list</returns>
        public static int GetLengthOfLongestString<T>(this IEnumerable<T> source, Func<T, string> selector)
        {
            return source.Select(selector).Max(s => s.Length);
        }

        /// <summary>
        /// Indicates whether the specified string is null or an <see cref="string.Empty"/> string.
        /// </summary>
        /// <param name="source">The string to test.</param>
        /// <returns>True if the value parameter is null or an empty string (""); otherwise, false.</returns>
        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }
    }
}