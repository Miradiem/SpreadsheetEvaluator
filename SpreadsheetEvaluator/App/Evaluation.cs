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


//public SheetData EvaluateSpreadsheet2(SheetData sheetData)
//{
//    object[][] data = sheetData.Data;
//    for (int column = 0; column < data.Length; column++)
//    {
//        object cell = data[data.Length - 1][column];
//        if (cell is string formula && formula.StartsWith('='))
//        {
//            Match cellReference = Regex.Match(formula.Substring(1), @"^[A-Z][0-9]+$");
//            if (cellReference.Success)
//            {
//                int row = int.Parse(cellReference.Groups[0].Value.Substring(1)) - 1;
//                int col = cellReference.Groups[0].Value[0] - 'A';

//                cell = data[row][col];
//            }
//            var trees = new TreeNodes().TestDict;
//            foreach (var function in trees)
//            {
//                if (formula.StartsWith(function.Key))
//                {
//                    cell = function.Value(data, formula.Substring(function.Key.Length));
//                }
//            }

//            data[column][data.Length] = cell;
//        }

//    }

//    return sheetData;
//}

///////
/////
//private object EvaluateFormula(object[][] sheet, string formula)
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
//        return EvaluateCell(sheet, row, col);
//    }
//    var trees = new TreeNodes().TestDict;
//    foreach (var operatorFunc in trees)
//    {
//        if (formula.StartsWith(operatorFunc.Key))
//        {
//            return operatorFunc.Value(sheet, formula.Substring(operatorFunc.Key.Length));
//        }
//    }

//    return "#ERROR: Unknown formula";
//}

//private object EvaluateCell(object[][] sheet, int row, int col)
//{
//    object cell = sheet[row][col];
//    if (cell is string str && str.StartsWith("="))
//    {
//        cell = EvaluateFormula(sheet, str);
//        sheet[row][col] = cell;
//    }
//    return cell;
//}

//public SheetData EvaluateSpreadsheet3(SheetData sheetData)
//{
//    object[][] sheet = sheetData.Data;
//    for (int row = 0; row < sheet.Length; row++)
//    {
//        for (int col = 0; col < sheet[row].Length; col++)
//        {
//            EvaluateCell(sheet, row, col);
//        }
//    }


//    return sheetData;
//}