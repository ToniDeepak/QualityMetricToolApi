using ClosedXML.Excel;
using ExternalServices.Interfaces;
using System.Linq;

namespace ExternalServices.Implementations
{
    public class ExcelService: IExcelService
    {
        public void StyleWorkSheet(IXLWorksheet worksheet, XLColor color = null, XLBorderStyleValues borderStyleValues = XLBorderStyleValues.None,
       XLTableTheme xLTableTheme = null)
        {

            worksheet.Cells().Style.Border.BottomBorderColor = color ?? XLColor.NoColor;
            worksheet.Cells().Style.Border.TopBorderColor = color ?? XLColor.NoColor;
            worksheet.Cells().Style.Border.RightBorderColor = color ?? XLColor.NoColor;
            worksheet.Cells().Style.Border.LeftBorderColor = color ?? XLColor.NoColor;
            worksheet.Cells().Style.Border.TopBorder = borderStyleValues;
            worksheet.Cells().Style.Border.BottomBorder = borderStyleValues;
            worksheet.Cells().Style.Border.LeftBorder = borderStyleValues;
            worksheet.Cells().Style.Border.RightBorder = borderStyleValues;
            worksheet.Row(1).Style.Font.SetBold(true);
            worksheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            worksheet.Cells().Style.Alignment.WrapText = true;
            worksheet.Tables.First().Theme = xLTableTheme ?? XLTableTheme.None;
            worksheet.Tables.First().ShowAutoFilter = false;
        }

        public  void SetColumnWidths(IXLWorksheet worksheet, int[] columnWidths)
        {
            for (int i = 1; i < columnWidths.Length; i++)
            {
                worksheet.Columns(i.ToString()).Width = columnWidths[i];
            }
        }

    }
}
