using SpreadsheetEvaluator.Api;
using System.Data;
using System.Text.RegularExpressions;

namespace SpreadsheetEvaluator.App
{
    public static class Functions
    {
        public static List<SheetData> CloneSheetData(Spreadsheet spreadSheet)
        {
            var result = new List<SheetData>();
            foreach (var sheet in spreadSheet.Sheets)
            {
                var clonedSheetData = new SheetData
                {
                    Id = sheet.Id,
                    Data = sheet.Data.Select(data => data.ToArray()).ToArray()
                };
                result.Add(clonedSheetData);
            }

            return result;
        }

        public static object Function(string functionName, List<object> evaluatedParameters)
        {
            switch (functionName)
            {
                case "SUM":
                    return Sum(evaluatedParameters);
                case "MULTIPLY":
                    return Multiply(evaluatedParameters);
                case "IF":
                    return If(evaluatedParameters);
                case "GT":
                    return Gt(evaluatedParameters);
                case "EQ":
                    return Eq(evaluatedParameters);
                case "NOT":
                    return Not(evaluatedParameters);
                case "AND":
                    return And(evaluatedParameters);
                case "OR":
                    return Or(evaluatedParameters);
                case "DIVIDE":
                    return Divide(evaluatedParameters);
                case "CONCAT":
                    return Concat(evaluatedParameters);
                case string nameValue when Regex.IsMatch(nameValue, @"^[A-Z]\d+$"):
                    return CellReference(evaluatedParameters);
                default:
                    return "##Error: invalid function name.";
            }
        }

        private static object Sum(List<object> parameters)
        {
            var valuesToSum = new List<double>();
            foreach (var parameter in parameters)
            {
                if (parameter is double doubleValue)
                {
                    valuesToSum.Add(doubleValue);
                }
                if (parameter is string stringValue
                    && double.TryParse(stringValue, out doubleValue))
                {
                    valuesToSum.Add(doubleValue);
                }
                if (parameter is string || parameter is bool)
                {
                    return "#ERROR: =SUM incompatible type.";
                }
            }

            return valuesToSum.Sum();
        }

        private static object Multiply(List<object> parameters)
        {
            var valuesToMultiply = new List<double>();
            foreach (var parameter in parameters)
            {
                if (parameter is double doubleValue)
                {
                    valuesToMultiply.Add(doubleValue);
                }
                if (parameter is string stringValue
                    && double.TryParse(stringValue, out doubleValue))
                {
                    valuesToMultiply.Add(doubleValue);
                }
                if (parameter is string || parameter is bool)
                {
                    return "#ERROR: =MULTIPLY incompatible type.";
                }
            }

            double result = 1;
            foreach (var value in valuesToMultiply)
            {
                result *= value;
            }

            return result;
        }

        private static object Divide(List<object> parameters)
        {
            if (parameters.Count != 2)
            {
                return "#ERROR: =DIVIDE requires 2 parameters.";
            }

            double numerator = 0;
            double denominator = 0;

            if (parameters[0] is double divident
                && parameters[1] is double divisor)
            {
                numerator = divident;
                denominator = divisor;
            }
            if (parameters[0] is string dividentString && double.TryParse(dividentString, out divident)
                && parameters[1] is string divisorString && double.TryParse(divisorString, out divisor))
            {
                numerator = divident;
                denominator = divisor;
            }

            if (parameters[0] is string || parameters[0] is bool)
            {
                return "#ERROR: =DIVIDE incompatible numerator.";
            }
            if (parameters[1] is string || parameters[1] is bool)
            {
                return "#ERROR: =DIVIDE incompatible denominator.";
            }
            if (denominator == 0)
            {
                return "#ERROR: =DIVIDE cannot divide by zero.";
            }

            return numerator / denominator;
        }

        private static object Gt(List<object> parameters)
        {
            if (parameters.Count != 2)
            {
                return "#ERROR: =GT requires 2 parameters.";
            }

            double firstValue = 0;
            double secondValue = 0;

            if (parameters[0] is double
                && parameters[1] is double)
            {
                firstValue = (double)parameters[0];
                secondValue = (double)parameters[1];
            }
            if (parameters[0] is string firstString && double.TryParse(firstString, out double firstDouble)
                && parameters[1] is string secondString && double.TryParse(secondString, out double secondDouble))
            {
                firstValue = firstDouble;
                secondValue = secondDouble;
            }

            if (parameters[0] is string || parameters[0] is bool)
            {
                return "#ERROR: =GT incompatible type (first).";
            }
            if (parameters[1] is string || parameters[0] is bool)
            {
                return "#ERROR: =GT incompatible type (second).";
            }

            return firstValue > secondValue;
        }

        private static object Eq(List<object> parameters)
        {
            if (parameters.Count != 2)
            {
                return "#ERROR: =EQ function requires 2 parameters";
            }

            var left = parameters[0];
            var right = parameters[1];

            if ((left is string && right is double)
                || (left is double && right is string)
                || (left is bool && right is bool))
            {
                return "#ERROR: =EQ incompatible types.";
            }

            if (left is bool leftBool && right is bool rightBool)
            {
                return leftBool == rightBool;
            }
            if (left is string leftString && right is string rightString)
            {
                return leftString.Trim().Equals(rightString.Trim());
            }
            if (left is double leftDouble && right is double rightDouble)
            {
                return leftDouble == rightDouble;
            }

            return false;
        }

        private static object Not(List<object> parameters)
        {
            if (parameters.Count != 1 || (parameters[0] is not bool))
            {
                return "#ERROR: =NOT incompatible type.";
            }

            return !(bool)parameters[0];
        }

        private static object And(List<object> parameters)
        {
            var boolValues = new List<bool>();
            foreach (var parameter in parameters)
            {
                if (parameter is bool boolValue)
                {
                    boolValues.Add(boolValue);
                }
                if (parameter is not bool)
                {
                    return "#ERROR: =AND incompatible type";
                }
            }

            return boolValues.All(b => b);
        }

        private static object Or(List<object> parameters)
        {
            var valuesToOr = new List<bool>();
            foreach (var parameter in parameters)
            {
                if (parameter is bool boolValue)
                {
                    valuesToOr.Add(boolValue);
                }
                if (parameter is not bool)
                {
                    return "#ERROR: =OR incompatible type";
                }
            }

            return valuesToOr.Any(b => b);
        }

        private static object If(List<object> parameters)
        {
            if (parameters.Count != 3)
            {
                return "#ERROR: =IF requires 3 parameters.";
            }

            var condition = parameters[0];
            var trueResult = parameters[1];
            var falseResult = parameters[2];

            if (condition is bool conditionValue)
            {
                return conditionValue ? trueResult : falseResult;
            }

            if (condition is string conditionString
                && double.TryParse(conditionString, out var conditionValueAsDouble))
            {
                return conditionValueAsDouble != 0 ? trueResult : falseResult;
            }

            return "#ERROR: =IF incompatible type.";
        }

        private static object Concat(List<object> parameters) =>
            string.Concat(parameters.Select(parameter => string.Join(" ", parameter)));

        private static object CellReference(List<object> parameters)
        {
            if (parameters.Count != 1)
            {
                return "#ERROR: =CellReference requires 1 parameter";
            }

            var cellReference = parameters[0];

            if (cellReference is null)
            {
                return "#ERROR: =CellReference is empty.";
            }

            return cellReference;
        }
    }
}