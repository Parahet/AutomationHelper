using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutomationHelper.Base
{
	public class CustomAssert
	{
		public static void CreateLoggerInstance(ILog logger = null)
		{
			_logger = logger;
		}
		private static ILog _logger;
		public static void Equal<T>(T expected, T actual, string errMessage = "", ILog logger = null)
		{
			var methodName = $"{nameof(CustomAssert)}.{MethodBase.GetCurrentMethod().Name}";
			if (expected.Equals(actual))
				(logger ?? _logger)?.Pass(
					$"{methodName} passed. Expected and actual values are '{expected}'");
			else
				throw new Exception($"{errMessage}. {methodName} Failure. Expected '{expected.ToString()}' ; actual '{actual.ToString()}'");
		}

		public static void NotEqual<T>(T expected, T actual, string errMessage = "", ILog logger = null)
		{
			var methodName = $"{nameof(CustomAssert)}.{MethodBase.GetCurrentMethod().Name}";
			if (!expected.Equals(actual))
				(logger ?? _logger)?.Pass($"{methodName} passed. Expected and actual value are different.");
			else
				throw new Exception($"{errMessage}. {methodName} Failure. Expected '{expected.ToString()}' ; actual '{actual.ToString()}'");
		}

		public static void Contains(string expectedSubstring, string actualString, string errMessage = "", ILog logger = null)
		{
			var methodName = $"{nameof(CustomAssert)}.{MethodBase.GetCurrentMethod().Name}";
			if (actualString.Contains(expectedSubstring))
				(logger ?? _logger)?.Pass(
					$"{methodName} passed. Substring '{expectedSubstring}' is present in string '{actualString}'");
			else
				throw new Exception(
					$"{errMessage}. {methodName} Failure. Not found string '{expectedSubstring}' In '{actualString}'");
		}

		public static void StartWith(string expectedStartSubstring, string actualString, string errMessage = "", ILog logger = null)
		{
			var methodName = $"{nameof(CustomAssert)}.{MethodBase.GetCurrentMethod().Name}";
			if (actualString.StartsWith(expectedStartSubstring))
				(logger ?? _logger)?.Pass(
					$"{methodName} passed. String '{actualString}' start with '{expectedStartSubstring}'");
			else
				throw new Exception(
					$"{errMessage}. {methodName} Failure. String '{actualString}' should start with '{expectedStartSubstring}'");
		}

		public static void True(bool condition, string errorMessage = "", ILog logger = null)
		{
			var methodName = $"{nameof(CustomAssert)}.{MethodBase.GetCurrentMethod().Name}";
			if (condition)
			{
				(logger ?? _logger)?.Pass($"{methodName} passed. (error did't occur: {errorMessage})");
			}
			else
				throw new Exception(
					$"{errorMessage}. {methodName} Failure. Expected 'true'; actual '{condition.ToString()}'");
		}

		public static void False(bool condition, string errorMessage = "", ILog logger = null)
		{
			var methodName = $"{nameof(CustomAssert)}.{MethodBase.GetCurrentMethod().Name}";
			if (!condition)
			{
				(logger ?? _logger)?.Pass($"{methodName} passed. (error did't occur: {errorMessage})");
			}
			else
				throw new Exception(
					$"{errorMessage}. {methodName} Failure. Expected 'false'; actual '{condition.ToString()}'");
		}

		/// <summary>
		/// {a,b,c} and {b,a,c} -is not equal 
		/// </summary>
		public static void ListsEqual<T>(List<T> expected, List<T> actual, string errMessage = "", ILog logger = null)
		{
			var methodName = $"{nameof(CustomAssert)}.{MethodBase.GetCurrentMethod().Name}";
			if (expected.SequenceEqual(actual))
				(logger ?? _logger)?.Pass($"{methodName} passed. Expected and actual lists are equal");
			else
			{
				var resultMessage = String.Empty;
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
					$"{errMessage}. {methodName} Failure{resultMessage}");

			}
		}

		public static void ListsNotEqual<T>(List<T> expected, List<T> actual, string errMessage = "", ILog logger = null)
		{
			var methodName = $"{nameof(CustomAssert)}.{MethodBase.GetCurrentMethod().Name}";
			var temp = new List<T>(expected);
			foreach (var s in actual)
			{
				if (temp.Any(t => t.Equals(s)))
					temp.Remove(s);
				else
				{
					(logger ?? _logger)?.Pass(
						$"{methodName}. Actual item '{s}' is not present in expected list: {expected.Select(i => i.ToString()).JoinByComma()}");
					return;
				}
			}
			//expected that temp should not be empty
			if (!temp.Any())
				throw new Exception($"{errMessage}. {methodName} Failure. Expected and actual lists are equal");
			(logger ?? _logger)?.Pass(
				$"{methodName} passed. Item(s) '{temp.Select(i => i.ToString()).JoinByComma()}' is (are)not present in list '{actual.Select(i => i.ToString()).JoinByComma()}'");
		}
	}
}

