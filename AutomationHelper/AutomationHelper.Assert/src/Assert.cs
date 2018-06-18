using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutomationHelper.Base;

namespace AutomationHelper.Assert
{
    public class Assert
    {
        public static void Equal<T>(T expected, T actual, string errMessage = "", ILog logger = null)
        {
            try
            {
                Xunit.Assert.Equal(expected, actual);
                logger?.Pass(
                    $"{nameof(Assert)}.{MethodBase.GetCurrentMethod().Name} passed. Expected and actual values are '{expected}'");
            }
            catch (Exception ex)
            {
                throw new Exception($"{errMessage}. {ex.Message}");
            }
        }
        public static void NotEqual<T>(T expected, T actual, string errMessage = "", ILog logger = null)
        {
            try
            {
                Xunit.Assert.NotEqual(expected, actual);
                logger?.Pass($"{nameof(Assert)}.{MethodBase.GetCurrentMethod().Name} passed. Expected and actual value are different.");
            }
            catch (Exception ex)
            {
                throw new Exception($"{errMessage}. {ex.Message}");
            }
        }

        public static void Contains(string expectedSubstring, string actualString, string errMessage = "", ILog logger = null)
        {
            try
            {
                Xunit.Assert.Contains(expectedSubstring, actualString);
                logger?.Pass($"{nameof(Assert)}.{MethodBase.GetCurrentMethod().Name} passed. Substring '{expectedSubstring}' is present in string '{actualString}'");
            }
            catch (Exception ex)
            {
                throw new Exception($"{errMessage}. {ex.Message}");
            }
        }

	    public static void StartWith(string expectedStartSubstring, string actualString, string errMessage = "", ILog logger = null)
	    {
		    try
		    {
			    Xunit.Assert.StartsWith(expectedStartSubstring, actualString);
		        logger?.Pass($"{nameof(Assert)}.{MethodBase.GetCurrentMethod().Name} passed. String '{actualString}' should start with '{expectedStartSubstring}'");
		    }
		    catch (Exception ex)
		    {
			    throw new Exception($"{errMessage}. {ex.Message}");
		    }
	    }

		public static void True(bool condition, string errorMessage = "", ILog logger = null)
        {
            Xunit.Assert.True(condition, errorMessage);
            logger?.Pass($"{nameof(Assert)}.{MethodBase.GetCurrentMethod().Name} passed. (error did't occur: {errorMessage})");
        }

        public static void False(bool condition, string errorMessage = "", ILog logger = null)
        {
            Xunit.Assert.False(condition, errorMessage);
            logger?.Pass($"{nameof(Assert)}.{MethodBase.GetCurrentMethod().Name} passed. (error did't occur: {errorMessage})");
        }

        /// <summary>
        /// {a,b,c} and {b,a,c} -is not equal 
        /// </summary>
        public static void ListsEqual<T>(List<T> expected, List<T> actual, string errMessage = "", ILog logger = null)
        {
            if (expected.SequenceEqual(actual))
                logger?.Pass($"{nameof(Assert)}.{MethodBase.GetCurrentMethod().Name}. Expected and actual lists are equal");
            else
            {
                var resultMessage = "";
                var expectedTemp = new List<T>(expected);
                var actualTemp = new List<T>(actual);
                expectedTemp.RemoveAll(actual.Contains);
                actualTemp.RemoveAll(expected.Contains);
                if (expectedTemp.Any())
                    resultMessage += $". Elements expected but not present: {expectedTemp.Select(i => i.ToString()).JoinByComma()}";
                if (actualTemp.Any())
                    resultMessage += $". Elements actual but not present in expected: {actualTemp.Select(i => i.ToString()).JoinByComma()}";
                if (!expectedTemp.Any() && !actualTemp.Any())
                    resultMessage = ". Elements equal but have different sorting order";
                throw new Exception(
                    $"{errMessage} {resultMessage}");

            }
        }

        public static void ListsNotEqual<T>(List<T> expected, List<T> actual, string errMessage = "", ILog logger = null)
        {
            var temp = new List<T>(expected);
            foreach (var s in actual)
            {
                if (temp.Any(t => t.Equals(s)))
                    temp.Remove(s);
                else
                {
                    logger?.Pass(
                        $"{nameof(Assert)}.{MethodBase.GetCurrentMethod().Name}. Actual item '{s}' is not present in expected list: {expected.Select(i => i.ToString()).JoinByComma()}");
                    return;
                }
            }
            //expected that temp should not be empty
            if (!temp.Any())
                throw new Exception($"{errMessage}. Assert ListNotEquals. Expected and actual lists are equal");
            logger?.Pass(
                $" Assert ListNotEquals. Item(s) '{temp.Select(i => i.ToString()).JoinByComma()}' is (are)not present in list '{actual.Select(i => i.ToString()).JoinByComma()}'");
        }
    }
}

