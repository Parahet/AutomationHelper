using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AutomationHelper.Base
{
    public static class LinqExtensions
    {
        public static IEnumerable<string> NaturalSort(this IEnumerable<string> list)
        {
            int maxLen = list.Select(s => s.Length).Max();
            char Func(string s) => char.IsDigit(s[0]) ? ' ' : char.MaxValue;

            return list
                .Select(s =>
                    new
                    {
                        OrgStr = s,
                        SortStr = Regex.Replace(s, @"(\d+)|(\D+)", m => m.Value.PadLeft(maxLen, Func(m.Value)))
                    })
                .OrderBy(x => x.SortStr)
                .Select(x => x.OrgStr);
        }

        #region LINQ EXTENSIONS

        public static T GetRandomElement<T>(this IEnumerable<T> values)
        {
            var elementsCount = values.Count();
            var random = new Random();
            var randomIndex = random.Next(elementsCount);
            return values.ElementAt(randomIndex);
        }

        public static T GetRandomElement<T>(this IEnumerable<T> values, string err)
        {
            try
            {
                var elementsCount = values.Count();
                var random = new Random();
                var randomIndex = random.Next(elementsCount);
                return values.ElementAt(randomIndex);
            }
            catch (Exception ex)
            {
                throw new Exception(err, ex);
            }
        }

        public static T FirstEx<T>(this IEnumerable<T> values)
        {
            return First(values, e => true, $"Can't find first '{typeof(T).FullName}' item in collection");
        }

        public static T First<T>(this IEnumerable<T> values, string err)
        {
            return First(values, e => true, err);
        }

        public static T First<T>(this IEnumerable<T> values, Func<T, bool> predicate, string err)
        {
            try
            {
                return values.First(predicate);
            }
            catch (Exception e)
            {
                throw new Exception(err + ". " + e);
            }
        }

        public static T Second<T>(this IEnumerable<T> values, string err)
        {
            return Second(values, e => true, err);
        }

        public static T Second<T>(this IEnumerable<T> values, Func<T, bool> predicate, string err)
        {
            try
            {
                return values.Where(predicate).ElementAt(1);
            }
            catch (Exception e)
            {
                throw new Exception(err + ". " + e);
            }

        }

        public static T LastEx<T>(this IEnumerable<T> values)
        {
            return Last(values, e => true, $"Can't find last '{typeof(T).FullName}' item in collection");
        }

        public static T Last<T>(this IEnumerable<T> values, string err)
        {
            return Last(values, e => true, err);
        }

        public static T Last<T>(this IEnumerable<T> values, Func<T, bool> predicate, string err)
        {
            try
            {
                return values.Last(predicate);
            }
            catch (Exception e)
            {
                throw new Exception(err + ". " + e);
            }

        }

        public static T ElementAtEx<T>(this IEnumerable<T> values, int index)
        {
            try
            {
                return values.ElementAt(index);
            }
            catch (Exception e)
            {
                throw new Exception($"Element with index {index} is not present in collection {typeof(T).FullName}. " +
                                    e);
            }
        }

        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source)
        {
            Random rnd = new Random();
            return source.OrderBy<T, int>((item) => rnd.Next());
        }

        public static IEnumerable<T> FindMin<T>(this IEnumerable<T> values, Func<T, int> selector)
        {
            var min = values.Min(selector);
            return values.Where(v => selector(v) == min);
        }

        public static IEnumerable<T> FindMin<T>(this IEnumerable<T> values, Func<T, double> selector)
        {
            var min = values.Min(selector);
            return values.Where(v => selector(v) == min);
        }

        public static IEnumerable<T> FindMax<T>(this IEnumerable<T> values, Func<T, int> selector)
        {
            var max = values.Max(selector);
            return values.Where(v => selector(v) == max);
        }

        public static IEnumerable<T> FindMax<T>(this IEnumerable<T> values, Func<T, double> selector)
        {
            var max = values.Max(selector);
            return values.Where(v => selector(v) == max);
        }

        public static string JoinByComma(this IEnumerable<string> values)
        {
            return String.Join(", ", values);
        }

        public static string JoinBySpace(this IEnumerable<string> values)
        {
            return String.Join(" ", values);
        }

        #endregion
    }
}
