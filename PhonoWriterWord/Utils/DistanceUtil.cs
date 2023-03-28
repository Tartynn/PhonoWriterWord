using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhonoWriterWord.Utils
{
    class DistanceUtil
    {
        public static int HotDogDistance(string a, string b)
        {
            if (a == "" & b == "")
                return 0;

            if (a.Length > 2 && b.Length > 2 && a[0] == b[1] && a[1] == b[0])
                return 1 + HotDogDistance(a.Substring(2), b.Substring(2));

            if (a.Length == 2 && b.Length == 2 && a[0] == b[1] && a[1] == b[0])
                return 1;

            if (a.Intersect(b).Count() == 0)
                return a.Length > b.Length ? a.Length : b.Length;

            string substring;
            bool found = LongestCommonSubstring(a, b, out substring) > 0;
            int startA = a.IndexOf(substring);
            int startB = b.IndexOf(substring);

            return HotDogDistance(a.Substring(0, startA), b.Substring(0, startB)) + HotDogDistance(a.Substring(startA + substring.Length), b.Substring(startB + substring.Length));
        }

        public static int LongestCommonSubstring(string str1, string str2, out string sequence)
        {
            sequence = string.Empty;

            if (String.IsNullOrEmpty(str1) || String.IsNullOrEmpty(str2))
                return 0;

            int[,] num = new int[str1.Length, str2.Length];
            int maxlen = 0;
            int lastSubsBegin = 0;
            StringBuilder sequenceBuilder = new StringBuilder();

            for (int i = 0; i < str1.Length; i++)
            {
                for (int j = 0; j < str2.Length; j++)
                {
                    if (str1[i] != str2[j])
                        num[i, j] = 0;
                    else
                    {
                        if ((i == 0) || (j == 0))
                            num[i, j] = 1;
                        else
                            num[i, j] = 1 + num[i - 1, j - 1];

                        if (num[i, j] > maxlen)
                        {
                            maxlen = num[i, j];
                            int thisSubsBegin = i - num[i, j] + 1;
                            if (lastSubsBegin == thisSubsBegin)
                            {//if the current LCS is the same as the last time this block ran
                                sequenceBuilder.Append(str1[i]);
                            }
                            else //this block resets the string builder if a different LCS is found
                            {
                                lastSubsBegin = thisSubsBegin;
                                sequenceBuilder.Length = 0; //clear it
                                sequenceBuilder.Append(str1.Substring(lastSubsBegin, (i + 1) - lastSubsBegin));
                            }
                        }
                    }
                }
            }
            sequence = sequenceBuilder.ToString();

            return maxlen;
        }

        public static double JaroDistance(string source, string target)
        {
            int m = source.Intersect(target).Count();

            if (m == 0) { return 0; }
            else
            {
                string sourceTargetIntersetAsString = "";
                string targetSourceIntersetAsString = "";
                IEnumerable<char> sourceIntersectTarget = source.Intersect(target);
                IEnumerable<char> targetIntersectSource = target.Intersect(source);
                foreach (char character in sourceIntersectTarget) { sourceTargetIntersetAsString += character; }
                foreach (char character in targetIntersectSource) { targetSourceIntersetAsString += character; }
                double t = LevenshteinDistance(sourceTargetIntersetAsString, targetSourceIntersetAsString) / 2;
                return ((m / source.Length) + (m / target.Length) + ((m - t) / m)) / 3;
            }
        }

        public static double JaroWinklerDistanceWithPrefixScale(string source, string target, double p)
        {
            double prefixScale = 0.1;

            if (p > 0.25) { prefixScale = 0.25; } // The maximu value for distance to not exceed 1
            else if (p < 0) { prefixScale = 0; } // The Jaro Distance
            else { prefixScale = p; }

            double jaroDistance = JaroDistance(source, target);
            double commonPrefixLength = CommonPrefixLength(source, target);

            return jaroDistance + (commonPrefixLength * prefixScale * (1 - jaroDistance));
        }

        private static double CommonPrefixLength(string source, string target)
        {
            int maximumPrefixLength = 4;
            int commonPrefixLength = 0;
            if (source.Length <= 4 || target.Length <= 4) { maximumPrefixLength = Math.Min(source.Length, target.Length); }

            for (int i = 0; i < maximumPrefixLength; i++)
            {
                if (source[i].Equals(target[i])) { commonPrefixLength++; }
                else { return commonPrefixLength; }
            }

            return commonPrefixLength;
        }

        public static int SimpleDistance(string source, string target)
        {
            int d = Math.Abs(source.Length - target.Length);
            int min = Math.Min(source.Length, target.Length);
            int total = 0;
            for (int i = 0; i < min; i++)
                total += source.Substring(i) == target.Substring(i) ? 0 : 1;
            return total + d;
        }

        /// <summary>
        /// Levenshtein distance
        /// Taken from : http://fuzzystring.codeplex.com/discussions/646462
        /// </summary>
        /// <param name="source">Word to analyze</param>
        /// <param name="target">Word from dictionary</param>
        /// <returns></returns>
        public static int LevenshteinDistance(string source, string target)
        {
            if (String.IsNullOrEmpty(source))
            {
                if (String.IsNullOrEmpty(target)) return 0;
                return target.Length;
            }
            if (String.IsNullOrEmpty(target)) return source.Length;

            if (source.Length > target.Length)
            {
                var temp = target;
                target = source;
                source = temp;
            }

            var m = target.Length;
            var n = source.Length;
            var distance = new int[2, m + 1];
            // Initialize the distance 'matrix'
            for (var j = 1; j <= m; j++) distance[0, j] = j;

            var currentRow = 0;
            for (var i = 1; i <= n; ++i)
            {
                currentRow = i & 1;
                distance[currentRow, 0] = i;
                var previousRow = currentRow ^ 1;
                for (var j = 1; j <= m; j++)
                {
                    var cost = (target[j - 1] == source[i - 1] ? 0 : 1);
                    distance[currentRow, j] = Math.Min(Math.Min(
                                distance[previousRow, j] + 1,
                                distance[currentRow, j - 1] + 1),
                                distance[previousRow, j - 1] + cost);
                }
            }
            return distance[currentRow, m];
        }
    }
}

