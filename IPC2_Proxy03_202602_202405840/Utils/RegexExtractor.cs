using System.Globalization;
using System.Text.RegularExpressions;

namespace IPC2_Proxy03_202602_202405840.Utils;

public static class RegexExtractor
{
    private static readonly Regex PatronFecha =
        new(@"(\d{2}/\d{2}/\d{4}\s+\d{2}:\d{2})", RegexOptions.Compiled);

    public static DateTime ExtraerFecha(string tiempo)
    {
        var m = PatronFecha.Match(tiempo ?? "");
        if (!m.Success) throw new FormatException("Fecha no encontrada en TIEMPO.");
        return DateTime.ParseExact(m.Value, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
    }
}