using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using CUE4Parse.Encryption.Aes;

namespace CUE4Parse.Utils
{
    public static class StringUtils
    {
        public static int NumOccurrences(this string s, char delimiter)
        {
            int Counter = 0;
            foreach (var item in s.ToCharArray())
                if (item == delimiter)
                    Counter++;
            return Counter;
        }

        // Creates string of specified size - Use for writing binary
        public static string SizedString(string name, int size)
        {
            char[] Chars = name.ToCharArray();
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                if (Chars.Length > i)
                {
                    builder.Append(Chars[i]);
                }
                else
                {
                    builder.Append('\x00');
                }
            }
            return builder.ToString();
        }

        public static string PathName(this string path)
        {
            return path.SubstringAfterLast(@"/").SubstringBefore(".");
        }

        public static string NormalizePath(this string path)
        {
            path = Path.ChangeExtension(path, ".uasset"); // Ends in ".uasset"
            path = path.StartsWith("/") ? path.Substring(1) : path; // Removes first slash
            path = path.StartsWith(@"Game/") ? "FortniteGame/Content/" + path.Substring(5) : path; // Starts with "FortniteGame/Content/"
            return path;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FAesKey ParseAesKey(this string s)
        {
            return new FAesKey(s);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringBefore(this string s, char delimiter)
        {
            var index = s.IndexOf(delimiter);
            return index == -1 ? s : s.Substring(0, index);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringBefore(this string s, string delimiter, StringComparison comparisonType = StringComparison.Ordinal)
        {
            var index = s.IndexOf(delimiter, comparisonType);
            return index == -1 ? s : s.Substring(0, index);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringAfter(this string s, char delimiter)
        {
            var index = s.IndexOf(delimiter);
            return index == -1 ? s : s.Substring(index + 1, s.Length - index - 1);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringAfter(this string s, string delimiter, StringComparison comparisonType = StringComparison.Ordinal)
        {
            var index = s.IndexOf(delimiter, comparisonType);
            return index == -1 ? s : s.Substring(index + delimiter.Length, s.Length - index - delimiter.Length);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringBeforeLast(this string s, char delimiter)
        {
            var index = s.LastIndexOf(delimiter);
            return index == -1 ? s : s.Substring(0, index);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringBeforeWithLast(this string s, char delimiter)
        {
            var index = s.LastIndexOf(delimiter);
            return index == -1 ? s : s.Substring(0, index + 1);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringBeforeLast(this string s, string delimiter, StringComparison comparisonType = StringComparison.Ordinal)
        {
            var index = s.LastIndexOf(delimiter, comparisonType);
            return index == -1 ? s : s.Substring(0, index);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringAfterLast(this string s, char delimiter)
        {
            var index = s.LastIndexOf(delimiter);
            return index == -1 ? s : s.Substring(index + 1, s.Length - index - 1);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringAfterWithLast(this string s, char delimiter)
        {
            var index = s.LastIndexOf(delimiter);
            return index == -1 ? s : s.Substring(index, s.Length - index);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringAfterLast(this string s, string delimiter, StringComparison comparisonType = StringComparison.Ordinal)
        {
            var index = s.LastIndexOf(delimiter, comparisonType);
            return index == -1 ? s : s.Substring(index + delimiter.Length, s.Length - index - delimiter.Length);
        }

        public static string SubstringReverse(this string s, int length, bool dots = false)
        {
            if (s == null) return string.Empty;
            if (length > s.Length) return s;
            return (dots ? "..." : "") + s.Substring(s.Length - length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this string orig, string value, StringComparison comparisonType) =>
            orig.IndexOf(value, comparisonType) >= 0;
    }
}