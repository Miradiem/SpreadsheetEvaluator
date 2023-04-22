using SpreadsheetEvaluator.Api;
using System.Text.RegularExpressions;

namespace SpreadsheetEvaluator.App
{
    public class Evaluation
    {
        public object[][] EvaluateSpreadsheet(SheetData data)
        {
            object[][] sheetData = data.Data;

            for (int row = 0; row < sheetData.Length; row++)
            {
                for (int column = 0; column < sheetData[row].Length; column++)
                {
                    object cell = sheetData[row][column];
                    if (cell is string formula
                        && formula.StartsWith('='))
                    {
                        cell = EvaluateFormula(sheetData, formula);

                        sheetData[row][column] = cell;
                    }
                }
            }

            return sheetData;
        }

        private object EvaluateFormula(object[][] sheetData, string formula)
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

            return SpreadSheetFunctions.EvaluateFunction(formulaName, evaluatedParameters);
        }

        private Match MatchFormula(string formula, Regex formulaReference, Regex directReference)
        {
            var result = Match.Empty;

            if (formulaReference.Match(formula).Success)
            {
                result = formulaReference.Match(formula);
            }
            if (directReference.Match(formula).Success)
            {
                result = directReference.Match(formula);

            }

            return result;
        }

        private (string formulaName, List<object> formulaParameters) GetParsedFormula(object[][] sheetData, Match matchedFormula)
        {
            var nameResult = "";
            var parametersResult = new List<object>();

            if (matchedFormula.Groups.Count == 1)
            {
                nameResult = matchedFormula.Groups[0].Value;
                parametersResult = ParseFormulaParameters(sheetData, nameResult);
            }
            if (matchedFormula.Groups.Count > 2)
            {
                var formulaValue = matchedFormula.Groups[2].Value;
                nameResult = matchedFormula.Groups[1].Value;
                parametersResult = ParseFormulaParameters(sheetData, formulaValue);
            }
            if (matchedFormula.Groups.Count > 2
                && matchedFormula.Groups[1].Value == "CONCAT")
            {
                var formulaValue = matchedFormula.Groups[2].Value;
                var evaluatedConcat = EvaluateConcat(formulaValue);
                nameResult = matchedFormula.Groups[1].Value;
                parametersResult = ParseConcat(evaluatedConcat);
            }

            return (nameResult, parametersResult);
        }

        private List<object> ParseFormulaParameters(object[][] sheetData, string formula)
        {
            var result = new List<object>();
            var nestedFormulas = new Regex(@"^[A-Z]+\(([^\(\)]*|(\((?<DEPTH>)|(\)(?<-DEPTH>)))*(?(DEPTH)(?!))\))$");

            var parsedFormula = ParseFormula(formula);
            var evaluatedFormula = EvaluateCellReferences(sheetData, parsedFormula);
            foreach (var parameter in evaluatedFormula)
            {
                if (double.TryParse(parameter.ToString(), out var doubleValue))
                {
                    result.Add(doubleValue);
                }
                else if (nestedFormulas.IsMatch(parameter.ToString()))
                {
                    result.Add(EvaluateFormula(sheetData, parameter.ToString()));
                }
                else
                {
                    result.Add(parameter);
                }
            }

            return result;
        }

        private List<object> ParseFormula(string formula)
        {
            var result = new List<object>();

            var startIndex = 0;
            var parenthesisCount = 0;
            for (var i = 0; i < formula.Length; i++)
            {
                var character = formula[i];
                if (character == ',' && parenthesisCount == 0)
                {
                    result.Add(formula.Substring(startIndex, i - startIndex));
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
            result.Add(formula.Substring(startIndex));

            return result;
        }

        private List<object> EvaluateCellReferences(object[][] sheetData, List<object> formula)
        {
            var result = new List<object>();
            foreach (var value in formula)
            {
                Match cellReference = Regex.Match(value.ToString().Trim(), @"^([A-Z])(\d+)$");
                if (cellReference.Success)
                {
                    var collumn = cellReference.Groups[1].Value[0] - 'A';
                    var row = int.Parse(cellReference.Groups[2].Value) - 1;

                    if (collumn >= sheetData[0].Length || row >= sheetData.Length)
                    {
                        result.Add($"#ERROR: {value} cell does not exist.");
                        continue;
                    }

                    var cellValue = sheetData[row][collumn];
                    result.Add(cellValue);
                }
                if (cellReference.Success is false)
                {
                    result.Add(value);
                }
            }

            return result;
        }

        private string EvaluateConcat(string formula)
        {
            var concatReference = new Regex(@"CONCAT\((.*?)\)");
            var matchedFormula = concatReference.Match(formula);

            while (matchedFormula.Success)
            {
                var concatValue = matchedFormula.Groups[1].Value;
                var evaluatedConcat = EvaluateConcat(concatValue);
                formula = formula.Replace(matchedFormula.Value, evaluatedConcat);
                matchedFormula = concatReference.Match(formula);
            }

            return formula;
        }

        private List<object> ParseConcat(string formula) =>
            Regex.Matches(formula, "\"(?:\\\\\"|[^\"])+\"|[^,\\s]+")
            .Cast<Match>()
            .Select(match => match.Value.Trim('"'))
            .ToList()
            .ConvertAll(stringValue => (object)stringValue);
    }
    
}