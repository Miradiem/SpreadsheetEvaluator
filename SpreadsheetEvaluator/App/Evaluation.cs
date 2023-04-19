using Flurl.Http;
using SpreadsheetEvaluator.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpreadsheetEvaluator.App
{
    public class Evaluation
    {
        public object[][] EvaluateSpreadsheet(SheetData sheetData)
        {
            object[][] sheet = sheetData.Data;

            for (int row = 0; row < sheet.Length; row++)
            {
                for (int column = 0; column < sheet[row].Length; column++)
                {
                    object cell = sheet[row][column];
                    if (cell is string formula
                        && formula.StartsWith('='))
                    {
                        cell = TreeNodes.EvaluateFunction(sheet, formula);

                        sheet[row][column] = cell;
                    }
                }
            }

            return sheet;
        }
        
    }
    
}