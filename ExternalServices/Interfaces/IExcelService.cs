

using ClosedXML.Excel;

namespace ExternalServices.Interfaces
{
    public interface IExcelService
    {
        void SetColumnWidths(IXLWorksheet worksheet, int[] columnWidths);
        void StyleWorkSheet(IXLWorksheet worksheet, XLColor color = null, XLBorderStyleValues borderStyleValues = XLBorderStyleValues.None,
       XLTableTheme xLTableTheme = null);
    }
}
