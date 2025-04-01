using ClosedXML.Excel;
namespace MoviesDataCleaning;

public static class CellExtensions
{
    public static string? GetStr(this IXLCell cell)
    {
        if(cell.Value.IsBlank)
            return null;

        return cell.Value.ToString();
    }


    public static int? GetInt(this IXLCell cell)
    {
        if(cell.Value.IsBlank)
            return null;

        return (int)cell.Value.GetNumber();
    }
}