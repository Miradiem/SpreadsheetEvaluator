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
        public static object EvaluateFormula(object[][] sheetData, string formula)
        {
            var formulaReference = new Regex(@"([A-Z]+\w*)\((.*)\)");
            var directReference = new Regex(@"[A-Z]\d+$");
            var matchedFormula = MatchFormula(formula, formulaReference, directReference);

            var parsedFormula = GetParsedFormula(sheetData, matchedFormula);
            var formulaName = parsedFormula.formulaName;
            var formulaParameters = parsedFormula.formulaParameters;

            var evaluatedParameters = new List<object>();
            foreach (var parameter in formulaParameters)
            {
                if (parameter is string parameterString
                    && (formulaReference.IsMatch(parameterString)
                        || directReference.IsMatch(parameterString)))
                {
                    evaluatedParameters.Add(EvaluateFormula(sheetData, parameterString));
                }
                else
                {
                    evaluatedParameters.Add(parameter);
                }
            }

            return Function(formulaName, evaluatedParameters); 
        }

        private static Match MatchFormula(string formula, Regex formulaReference, Regex directReference)
        {
            var matchedFormula = Match.Empty;

            if (formulaReference.Match(formula).Success)
            {
                matchedFormula = formulaReference.Match(formula);
            }
            if (directReference.Match(formula).Success)
            {
                matchedFormula = directReference.Match(formula);

            }
            if (!matchedFormula.Success)
            {
                throw new ArgumentException("Invalid input string.");
            }

            return matchedFormula;
        }

        private static (string formulaName, List<object> formulaParameters) GetParsedFormula(object[][] sheetData, Match matchedFormula)
        {
            var formulaName = "";
            var formulaParameters = new List<object>();

            if (matchedFormula.Groups.Count == 1)
            {
                formulaName = matchedFormula.Groups[0].Value;
                formulaParameters = ParseFormulaParameters(sheetData, formulaName);
            }
            if (matchedFormula.Groups.Count > 2)
            {
                var formulaValue = matchedFormula.Groups[2].Value;
                formulaName = matchedFormula.Groups[1].Value;
                formulaParameters = ParseFormulaParameters(sheetData, formulaValue);
            }
            if (matchedFormula.Groups.Count > 2
                && matchedFormula.Groups[1].Value == "CONCAT")
            {
                var formulaValue = matchedFormula.Groups[2].Value;
                var evaluatedConcat = EvaluateConcat(formulaValue);
                formulaName = matchedFormula.Groups[1].Value;
                formulaParameters = ParseConcat(evaluatedConcat);
            }

            return (formulaName, formulaParameters);
        }
        private static List<object> ParseFormulaParameters(object[][] sheetData, string formula)
        {
            var parsedParameters = new List<object>();
            var nestedFormulas = new Regex(@"^[A-Z]+\(([^\(\)]*|(\((?<DEPTH>)|(\)(?<-DEPTH>)))*(?(DEPTH)(?!))\))$");

            var parsedFormula = ParseFormula(formula);
            var evaluatedFormula = EvaluateCellReferences(sheetData, parsedFormula);
            foreach (var parameter in evaluatedFormula)
            {
                if (double.TryParse(parameter.ToString(), out var doubleValue))
                {
                    parsedParameters.Add(doubleValue);
                }
                else if (nestedFormulas.IsMatch(parameter.ToString()))
                {
                    parsedParameters.Add(EvaluateFormula(sheetData, parameter.ToString()));
                }
                else
                {
                    parsedParameters.Add(parameter);
                }
            }

            return parsedParameters;
        }
        private static List<object> ParseFormula(string formula)
        {
            var parameters = new List<object>();

            var startIndex = 0;
            var parenthesisCount = 0;
            for (var i = 0; i < formula.Length; i++)
            {
                var character = formula[i];
                if (character == ',' && parenthesisCount == 0)
                {
                    parameters.Add(formula.Substring(startIndex, i - startIndex));
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
            parameters.Add(formula.Substring(startIndex));

            return parameters; 
        }
        private static List<object> EvaluateCellReferences(object[][] sheetData, List<object> formula)
        {
            var evaluatedCells = new List<object>();
            foreach (var value in formula)
            {
                Match cellReference = Regex.Match(value.ToString().Trim(), @"^([A-Z])(\d+)$");
                if (cellReference.Success)
                {
                    int col = cellReference.Groups[1].Value[0] - 'A';
                    int row = int.Parse(cellReference.Groups[2].Value) - 1;
                    var cellValue = sheetData[row][col];

                    evaluatedCells.Add(cellValue);
                }
                if (cellReference.Success is false)
                {
                    evaluatedCells.Add(value);
                }
            }

            return evaluatedCells;
        }
        private static string EvaluateConcat(string formula)
        {
            var concatReference = new Regex(@"CONCAT\((.*?)\)");
            var matchedFormula = concatReference.Match(formula);

            while (matchedFormula.Success)
            {
                string concatValue = matchedFormula.Groups[1].Value;
                string evaluatedConcat = EvaluateConcat(concatValue);
                formula = formula.Replace(matchedFormula.Value, evaluatedConcat);
                matchedFormula = concatReference.Match(formula);
            }

            return formula;
        }

        private static List<object> ParseConcat(string formula)
        {
            var parsedStrings = Regex.Matches(formula, "\"(?:\\\\\"|[^\"])+\"|[^,\\s]+")
                                        .Cast<Match>()
                                        .Select(match => match.Value.Trim('"'))
                                        .ToList().ConvertAll(stringValue => (object)stringValue);

            return parsedStrings;
        }

        

        private static object Function(string functionName, List<object> evaluatedParameters)
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
                    return "##Error, invalid function name.";
            }
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

            if (parameters[0] is double)
            {
                x = (double)parameters[0];
            }
            else if (parameters[0] is string stringValue && double.TryParse(stringValue, out double doubleValue))
            {
                x = doubleValue;
            }
            else
            {
                throw new ArgumentException("Invalid parameter.");
            }

            if (parameters[1] is double)
            {
                y = (double)parameters[1];
            }
            else if (parameters[1] is string stringValue && double.TryParse(stringValue, out double doubleValue))
            {
                y = doubleValue;
            }
            else
            {
                throw new ArgumentException("Invalid parameter.");
            }

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

            if (left is double leftDouble && right is double rightDouble)
            {
                return leftDouble == rightDouble;
            }
            if (left is string leftString && right is string rightString)
            {
                return leftString.Equals(rightString);
            }

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