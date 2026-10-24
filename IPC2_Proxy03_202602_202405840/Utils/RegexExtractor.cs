using System.Globalization;
using System.Text.RegularExpressions;

namespace IPC2_Proxy03_202602_202405840.Utils;

public static class RegexExtractor
{
    // Acepta 1 o 2 dígitos en día, mes, hora
    private static readonly Regex PatronFecha = new(
        @"(\d{1,2}/\d{1,2}/\d{4}\s+\d{1,2}:\d{2})",
        RegexOptions.Compiled);

    private static readonly string[] Formatos =
    {
        "d/M/yyyy H:mm",   "dd/MM/yyyy HH:mm",
        "d/MM/yyyy H:mm",  "dd/M/yyyy HH:mm",
        "d/M/yyyy HH:mm",  "dd/MM/yyyy H:mm",
    };

    public static DateTime ExtraerFecha(string tiempo)
    {
        var m = PatronFecha.Match(tiempo ?? "");
        if (!m.Success) throw new FormatException("Fecha no encontrada en TIEMPO.");

        if (!DateTime.TryParseExact(m.Value, Formatos, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var fecha))
            throw new FormatException($"Fecha inválida: {m.Value}");

        return fecha;
    }
}