using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpreadsheetEvaluator.App
{
    public class TreeNodes
    {
        public static object EvaluateFunction(object[][] sheet, string input)
        {
            //1
            //var functionRegex = new Regex(@"([A-Z]+\w*)\((.*)\)");
            //var match = functionRegex.Match(input);
            var matchObject = Match.Empty;

            var functionReference = new Regex(@"([A-Z]+\w*)\((.*)\)");
            var directReference = new Regex(@"[A-Z]\d+$");

            if (functionReference.Match(input).Success)
            {
                matchObject = functionReference.Match(input);
            }
            if (directReference.Match(input).Success)
            {
                matchObject = directReference.Match(input);

            }
            if (!matchObject.Success)
            {
                throw new ArgumentException("Invalid input string.");
            }

            //var functionName = matchObject.Groups[1].Value;
            //var parameters = ParseParameters(sheet, matchObject.Groups[2].Value);
            var functionName = "";
            var parameters = new List<object>();

            if (matchObject.Groups.Count == 1)
            {
                functionName = matchObject.Groups[0].Value;
                parameters = ParseParameters(sheet, functionName);
            }
            var concatParameters = new List<object>(); //concat
            if (matchObject.Groups.Count == 3)
            {
                functionName = matchObject.Groups[1].Value;

                /////////// concat
                var testParams = matchObject.Groups[2].Value;
                string[] splitResult = Regex.Matches(testParams, "\"(?:\\\\\"|[^\"])+\"|[^,\\s]+")
                                            .Cast<Match>()
                                            .Select(m => m.Value.Trim('"'))
                                            .ToArray();

                foreach (var value in splitResult)
                {
                    object cellValue = new();
                    Match cellReference = Regex.Match(value.Trim(), @"^([A-Z])(\d+)$");
                    if (cellReference.Success)
                    {
                        int col = cellReference.Groups[1].Value[0] - 'A';
                        int row = int.Parse(cellReference.Groups[2].Value) - 1;
                        cellValue = sheet[row][col];
                    }
                    //
                    if (cellReference.Success is false)
                    {
                        concatParameters.Add(value);
                    }
                    if (cellReference.Success)
                    {
                        concatParameters.Add(cellValue.ToString());
                    }
                }
                ////////////////// concat

                parameters = ParseParameters(sheet, matchObject.Groups[2].Value);
            }
            //2
            // Check if any of the parameters are function calls
            var functionParameters = new List<object>();
            foreach (var parameter in parameters)
            {
                if (parameter is string parameterString
                    && (functionReference.IsMatch(parameterString)
                        || directReference.IsMatch(parameterString)))
                {
                    // Evaluate the nested function call recursively
                    functionParameters.Add(EvaluateFunction(sheet, parameterString)); // FUNCTIONS
                }
                else
                {
                    functionParameters.Add(parameter); // PLAIN PARAMS
                }
            }

            //3
            switch (functionName.ToUpper()) // to upper converts Sum to SUM but not sum to SUM
            {
                case "SUM":
                    return Sum(functionParameters);
                case "MULTIPLY":
                    return Multiply(functionParameters);
                case "IF":
                    return If(functionParameters);
                case "GT":
                    return Gt(functionParameters);
                case "EQ":
                    return Eq(functionParameters);
                case "NOT":
                    return Not(functionParameters);
                case "AND":
                    return And(functionParameters);
                case "OR":
                    return Or(functionParameters);
                case "DIVIDE":
                    return Divide(functionParameters);
                case "CONCAT":
                    return Concat(concatParameters);
                case string functionValue when Regex.IsMatch(functionValue, @"^[A-Z]\d+$"):
                    return CellReference(functionParameters);
                default:
                    throw new ArgumentException("Invalid function name.");
            }
        }

        private static List<object> ParseParameters(object[][] sheet, string input)
        {
            var parameters = new List<object>();
            var parameterStrings = new List<string>();
            var startIndex = 0;
            var parenthesisCount = 0;

            // Split the input string into parameter strings
            for (var i = 0; i < input.Length; i++)
            {
                var character = input[i];
                if (character == ',' && parenthesisCount == 0)
                {
                    parameterStrings.Add(input.Substring(startIndex, i - startIndex));
                    startIndex = i + 1;
                }
                else if (character == '(')
                {
                    parenthesisCount++;
                }
                else if (character == ')')
                {
                    parenthesisCount--;
                }
            }

            parameterStrings.Add(input.Substring(startIndex));
            var functionRegex = new Regex(@"^[A-Z]+\(([^\(\)]*|(\((?<DEPTH>)|(\)(?<-DEPTH>)))*(?(DEPTH)(?!))\))$");


            //////////////////////////////////////////////////////////////////////////////
            var secondParamStringForTestingPurposes = new List<object>();
            var values = parameterStrings;
            foreach (var value in values)
            {
                object cellValue = new();
                Match cellReference = Regex.Match(value.Trim(), @"^([A-Z])(\d+)$");
                if (cellReference.Success)
                {
                    int col = cellReference.Groups[1].Value[0] - 'A';
                    int row = int.Parse(cellReference.Groups[2].Value) - 1;
                    cellValue = sheet[row][col];
                }
                //
                if (cellReference.Success is false)
                {
                    secondParamStringForTestingPurposes.Add(value.Trim());// added trim
                }
                if (cellReference.Success)
                {
                    secondParamStringForTestingPurposes.Add(cellValue);// added trim
                }

                //else
                //{
                //    return "#ERROR: Incompatible types";
                //}
            }

            // Parse each parameter string into an object
            foreach (var parameterString in secondParamStringForTestingPurposes)
            {
                if (double.TryParse(parameterString.ToString(), out var value))
                {
                    parameters.Add(value);
                }
                else if (functionRegex.IsMatch(parameterString.ToString()))
                {
                    parameters.Add(EvaluateFunction(sheet, parameterString.ToString()));
                }
                else
                {
                    parameters.Add(parameterString);
                }
            }

            return parameters;
        }

        private static object Sum(List<object> parameters)
        {
            List<double> valuesToSum = new List<double>();
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
            }

            return valuesToSum.Sum();
        }

        private static object Multiply(List<object> parameters)
        {
            List<double> valuesToMultiply = new List<double>();
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
            }

            double result = 1.0;
            foreach (var value in valuesToMultiply)
            {
                result *= value;
            }

            return result;
        }
        private static object If(List<object> parameters)
        {
            if (parameters.Count != 3)
            {
                throw new ArgumentException("IF function requires three arguments.");
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

            throw new ArgumentException("Invalid condition parameter.");
        }

        private static object Gt(List<object> parameters)
        {
            if (parameters.Count != 2)
            {
                throw new ArgumentException("GT function expects 2 parameters");
            }

            double x;
            double y;

            // Extract the first parameter and try to convert it to a double
            if (parameters[0] is double)
            {
                x = (double)parameters[0];
            }
            else if (parameters[0] is string s1 && double.TryParse(s1, out double d1))
            {
                x = d1;
            }
            else
            {
                throw new ArgumentException("Invalid parameter.");
            }

            // Extract the second parameter and try to convert it to a double
            if (parameters[1] is double)
            {
                y = (double)parameters[1];
            }
            else if (parameters[1] is string s2 && double.TryParse(s2, out double d2))
            {
                y = d2;
            }
            else
            {
                throw new ArgumentException("Invalid parameter.");
            }

            // Return the result of the comparison
            return x > y;
        }

        private static object Eq(List<object> parameters)
        {
            if (parameters.Count != 2)
            {
                throw new ArgumentException("Invalid number of parameters for EQ function. Expected 2.");
            }

            object left = parameters[0];
            object right = parameters[1];

            // check if the left and right parameters are numeric values
            if (left is double leftDouble && right is double rightDouble)
            {
                return leftDouble == rightDouble;
            }

            // check if the left and right parameters are string values
            if (left is string leftString && right is string rightString)
            {
                return leftString.Equals(rightString);
            }

            // if the types of the left and right parameters are different, then they are not equal
            return false;
        }

        private static object Not(List<object> parameters)
        {
            if (parameters.Count != 1 || !(parameters[0] is bool))
            {
                throw new ArgumentException("Invalid parameter.");
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
                else
                {
                    return "#ERROR: Incompatible types";
                }
            }
            return boolValues.All(b => b);
        }

        private static object Or(List<object> parameters)
        {
            List<bool> valuesToOr = new List<bool>();
            foreach (var parameter in parameters)
            {
                if (parameter is bool boolValue)
                {
                    valuesToOr.Add(boolValue);
                }
                else
                {
                    return "#ERROR: Incompatible types";
                }
            }

            return valuesToOr.Any(b => b);
        }

        private static object Divide(List<object> parameters)
        {
            if (parameters.Count != 2)
            {
                throw new ArgumentException("Invalid number of parameters for DIVIDE function.");
            }

            double numerator, denominator;
            if (parameters[0] is double num && parameters[1] is double denom)
            {
                numerator = num;
                denominator = denom;
            }
            else if (parameters[0] is string numString && double.TryParse(numString, out num) &&
                     parameters[1] is string denomString && double.TryParse(denomString, out denom))
            {
                numerator = num;
                denominator = denom;
            }
            else
            {
                throw new ArgumentException("Invalid parameters for DIVIDE function.");
            }

            if (denominator == 0)
            {
                throw new DivideByZeroException("Cannot divide by zero.");
            }

            return numerator / denominator;
        }

        private static object Concat(List<object> parameters)
        {

            var result = "";
            foreach (var str in parameters)
            {
                result += string.Join(" ", str);
            }
            return result;
        }

        private static object CellReference(List<object> parameters)
        {
            if (parameters.Count != 1)
            {
                throw new ArgumentException("GetCellReference function expects 1 parameter");
            }

            var cellReference = parameters[0];

            if (cellReference is null)
            {
                throw new ArgumentException("Invalid parameter.");
            }

            return cellReference;
        }
    }
}


//public Dictionary<string, Func<object[][], string, object>> TestDict { get; set; }
//    = new Dictionary<string, Func<object[][], string, object>>()
//    {
//        { "SUM", Sum },
//        { "MULTIPLY", Multiply },
//        { "CONCAT", Concat },
//        { "DIVIDE", Divide },
//        { "GT", Gt },
//        { "EQ", Eq },
//        { "NOT", Not },
//        { "AND", And },
//        { "OR", Or },
//        { "IF", If }
//    };

//private static object EvaluateFormula(object[][] sheet, string formula)
//{
//    if (formula.StartsWith("="))
//    {
//        formula = formula.Substring(1);
//    }

//    // Match A1 notation
//    Match cellReference = Regex.Match(formula, @"^([A-Z])(\d+)$");
//    if (cellReference.Success)
//    {
//        int col = cellReference.Groups[1].Value[0] - 'A';
//        int row = int.Parse(cellReference.Groups[2].Value) - 1;
//        var cell = sheet[row][col];
//        return cell;
//    }
//    var testDik = new TreeNodes().TestDict;
//    foreach (var operatorFunc in testDik)
//    {
//        if (formula.StartsWith(operatorFunc.Key))
//        {
//            return operatorFunc.Value(sheet, formula.Substring(operatorFunc.Key.Length));
//        }
//    }

//    return "#ERROR: Unknown formula";
//}

//public static object Sum(object[][] sheet, string paramsStr)
//{
//    string[] values = paramsStr.Split(',', '(', ')');
//    int result = 0;
//    foreach (string value in values)
//    {

//        /////
//        object cellValue = new();
//        Match cellReference = Regex.Match(value.Trim(), @"^([A-Z])(\d+)$");
//        if (cellReference.Success)
//        {
//            int col = cellReference.Groups[1].Value[0] - 'A';
//            int row = int.Parse(cellReference.Groups[2].Value) - 1;
//            cellValue = sheet[row][col];
//        }
//        //
//        if (cellReference.Success is false
//            && int.TryParse(value, out int number))
//        {
//            result += number;
//        }
//        if (cellReference.Success
//            && cellValue is int intValue)
//        {
//            result += intValue;
//        }

//        //else
//        //{
//        //    return "#ERROR: Incompatible types";
//        //}
//    }
//    return result;
//}

//public static object Multiply(object[][] sheet, string paramsStr)
//{
//    string[] values = paramsStr.Split(',', '(', ')');
//    int result = 1;
//    foreach (string value in values)
//    {

//        /////
//        object cellValue = new();
//        Match cellReference = Regex.Match(value.Trim(), @"^([A-Z])(\d+)$");
//        if (cellReference.Success)
//        {
//            int col = cellReference.Groups[1].Value[0] - 'A';
//            int row = int.Parse(cellReference.Groups[2].Value) - 1;
//            cellValue = sheet[row][col];
//        }
//        //
//        if (cellReference.Success is false
//            && int.TryParse(value, out int number))
//        {
//            result *= number;
//        }
//        if (cellReference.Success
//            && cellValue is int intValue)
//        {
//            result *= intValue;
//        }

//        //else
//        //{
//        //    return "#ERROR: Incompatible types";
//        //}
//    }
//    return result;
//}

//public static object Divide(object[][] sheet, string paramsStr)
//{
//    string[] values = paramsStr.Split(',', '(', ')');

//    List<int> resultList = new List<int>();

//    foreach (string value in values)
//    {
//        /////
//        object cellValue = new();
//        Match cellReference = Regex.Match(value.Trim(), @"^([A-Z])(\d+)$");
//        if (cellReference.Success)
//        {
//            int col = cellReference.Groups[1].Value[0] - 'A';
//            int row = int.Parse(cellReference.Groups[2].Value) - 1;
//            cellValue = sheet[row][col];
//        }
//        //
//        if (cellReference.Success is false
//            && int.TryParse(value, out int number))
//        {
//            resultList.Add(number);
//        }
//        if (cellReference.Success
//            && cellValue is int intValue)
//        {
//            resultList.Add(intValue);
//        }


//        //else
//        //{
//        //    return "#ERROR: Incompatible types";
//        //}
//    }
//    double result = resultList[0];
//    foreach (var item in resultList.Skip(1))
//    {
//        result = result / item;
//    }

//    return result;
//}

//public static object Gt(object[][] sheet, string paramsStr)
//{
//    string[] values = paramsStr.Split(',', '(', ')');

//    List<int> resultList = new List<int>();

//    foreach (string value in values)
//    {
//        /////=GT(A1,B1) =GT(1,B1) 
//        object cellValue = new();
//        Match cellReference = Regex.Match(value.Trim(), @"^([A-Z])(\d+)$");
//        if (cellReference.Success)
//        {
//            int col = cellReference.Groups[1].Value[0] - 'A';
//            int row = int.Parse(cellReference.Groups[2].Value) - 1;
//            cellValue = sheet[row][col];
//        }
//        //
//        if (cellReference.Success is false
//            && int.TryParse(value, out int number))
//        {
//            resultList.Add(number);
//        }
//        if (cellReference.Success
//            && cellValue is int intValue)
//        {
//            resultList.Add(intValue);
//        }


//        //else
//        //{
//        //    return "#ERROR: Incompatible types";
//        //}
//    }

//    if (resultList[0] > resultList[1])
//    {
//        return true;
//    }

//    return false;
//}

//public static object Eq(object[][] sheet, string paramsStr)
//{
//    string[] values = paramsStr.Split(',', '(', ')');

//    List<int> resultList = new List<int>();

//    foreach (string value in values)
//    {
//        /////=GT(A1,B1) =GT(1,B1) 
//        object cellValue = new();
//        Match cellReference = Regex.Match(value.Trim(), @"^([A-Z])(\d+)$");
//        if (cellReference.Success)
//        {
//            int col = cellReference.Groups[1].Value[0] - 'A';
//            int row = int.Parse(cellReference.Groups[2].Value) - 1;
//            cellValue = sheet[row][col];
//        }
//        //
//        if (cellReference.Success is false
//            && int.TryParse(value, out int number))
//        {
//            resultList.Add(number);
//        }
//        if (cellReference.Success
//            && cellValue is int intValue)
//        {
//            resultList.Add(intValue);
//        }


//        //else
//        //{
//        //    return "#ERROR: Incompatible types";
//        //}
//    }

//    if (resultList[0] == resultList[1])
//    {
//        return true;
//    }

//    return false;
//}

//public static object Not(object[][] sheet, string paramsStr)
//{
//    string[] values = paramsStr.Split(',', '(', ')');

//    foreach (string value in values)
//    {
//        /////=GT(A1,B1) =GT(1,B1) 
//        object cellValue = new();
//        Match cellReference = Regex.Match(value.Trim(), @"^([A-Z])(\d+)$");
//        if (cellReference.Success)
//        {
//            int col = cellReference.Groups[1].Value[0] - 'A';
//            int row = int.Parse(cellReference.Groups[2].Value) - 1;
//            cellValue = sheet[row][col];
//        }
//        //
//        if (cellReference.Success is false)
//        {
//            if (value == "true")
//            {
//                return false;
//            }
//            if (value == "false")
//            {
//                return true;
//            }
//        }
//        if (cellReference.Success
//            && cellValue is bool boolValue)
//        {
//            return !boolValue;
//        }


//        //else
//        //{
//        //    return "#ERROR: Incompatible types";
//        //}
//    }



//    return false;
//}

//public static object And(object[][] sheet, string paramsStr)
//{
//    string[] values = paramsStr.Split(',', '(', ')');
//    var resultList = new List<object>();
//    foreach (string value in values)
//    {
//        /////=GT(A1,B1) =GT(1,B1) 
//        object cellValue = new();
//        Match cellReference = Regex.Match(value.Trim(), @"^([A-Z])(\d+)$");
//        if (cellReference.Success)
//        {
//            int col = cellReference.Groups[1].Value[0] - 'A';
//            int row = int.Parse(cellReference.Groups[2].Value) - 1;
//            cellValue = sheet[row][col];
//        }
//        bool result;
//        if (cellReference.Success is false
//            && bool.TryParse(value, out result))
//        {
//            resultList.Add(result);
//        }
//        if (cellReference.Success
//            && cellValue is bool boolValue)
//        {
//           resultList.Add(boolValue);
//        }


//        //else
//        //{
//        //    return "#ERROR: Incompatible types";
//        //}
//    }

//    if (resultList.Contains(false))
//    {
//        return false;
//    }

//    return true;
//}

//public static object Or(object[][] sheet, string paramsStr)
//{
//    string[] values = paramsStr.Split(',', '(', ')');
//    var resultList = new List<object>();
//    foreach (string value in values)
//    {
//        /////=GT(A1,B1) =GT(1,B1) 
//        object cellValue = new();
//        Match cellReference = Regex.Match(value.Trim(), @"^([A-Z])(\d+)$");
//        if (cellReference.Success)
//        {
//            int col = cellReference.Groups[1].Value[0] - 'A';
//            int row = int.Parse(cellReference.Groups[2].Value) - 1;
//            cellValue = sheet[row][col];
//        }
//        bool result;
//        if (cellReference.Success is false
//            && bool.TryParse(value, out result))
//        {
//            resultList.Add(result);
//        }
//        if (cellReference.Success
//            && cellValue is bool boolValue)
//        {
//            resultList.Add(boolValue);
//        }


//        //else
//        //{
//        //    return "#ERROR: Incompatible types";
//        //}
//    }

//    if (resultList.Contains(true))
//    {
//        return true;
//    }

//    return false;
//}

//public static object If(object[][] sheet, string paramsStr)
//{
//    string[] values = paramsStr.Split(',');

//    foreach (string value in values)
//    {
//        object cellValue = new();
//        Match cellReference = Regex.Match(value.Trim(), @"^([A-Z])(\d+)$");
//        if (cellReference.Success)
//        {
//            int col = cellReference.Groups[1].Value[0] - 'A';
//            int row = int.Parse(cellReference.Groups[2].Value) - 1;
//            cellValue = sheet[row][col];
//        } 
//    }
//    //"=IF(GT(A1, B1), A1, B1)"
//    object testAnswer = new();
//    var trees = new TreeNodes().TestDict;
//    if (trees.Keys.Any(k => paramsStr.Contains(k)))
//    {
//        //check gpt
//    }


//    foreach (var function in trees)
//    {
//        if (paramsStr.StartsWith(function.Key))
//        {
//            testAnswer = function.Value(sheet, paramsStr.Substring(function.Key.Length));
//        }
//    }

//    return string.Join(" ", values);
//}

//public static object Concat(object[][] sheet, string paramsStr)
//{
//    string[] values = paramsStr.Split(',', '(', ')');
//    string result = "";
//    foreach (string value in values)
//    {

//        object cellValue = new();
//        Match cellReference = Regex.Match(value.Trim(), @"^([A-Z])(\d+)$");
//        if (cellReference.Success)
//        {
//            int col = cellReference.Groups[1].Value[0] - 'A';
//            int row = int.Parse(cellReference.Groups[2].Value) - 1;
//            cellValue = sheet[row][col];
//        }

//        if (cellReference.Success)
//        {
//            result = string.Concat(result, cellValue as string, " ");
//        }
//        if (!cellReference.Success)
//        {
//            result = string.Concat(result, value, " ");
//        }

//    }
//    return result.Trim();
//}