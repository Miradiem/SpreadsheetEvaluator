using SpreadsheetEvaluator.Api;

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
                        cell = TreeNodes.EvaluateFormula(sheetData, formula);

                        sheetData[row][column] = cell;
                    }
                }
            }

            return sheetData;
        }
        
    }
    
}