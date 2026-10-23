using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using IPC2_Proxy03_202602_202405840.Models;
using IPC2_Proxy03_202602_202405840.Services;

namespace IPC2_Proxy03_202602_202405840.Controllers;

[ApiController]
[Route("api")]
public class ApiController : ControllerBase
{
    private readonly DatabaseService _db;
    private readonly RtuService _rtu;
    private readonly DteService _dte;
    private readonly ReportService _report;

    public ApiController(DatabaseService db, RtuService rtu, DteService dte, ReportService report)
    { _db = db; _rtu = rtu; _dte = dte; _report = report; }

    // ─── Procesamiento (RTU o DTE, autodetecta) ──────────────────
    [HttpPost("procesar")]
    public IActionResult Procesar([FromBody] JsonElement body)
    {
        if (body.ValueKind != JsonValueKind.Array || body.GetArrayLength() == 0)
            return BadRequest(new { error = "Se esperaba un array JSON no vacío." });

        var first = body[0];
        if (first.ValueKind != JsonValueKind.Object)
            return BadRequest(new { error = "Cada elemento debe ser un objeto JSON." });

        // Detecta RTU (tiene "NIT" y "nombre")
        if (first.TryGetProperty("NIT", out _) && first.TryGetProperty("nombre", out _))
        {
            var reqs = body.Deserialize<List<RtuRequest>>()!;
            return Ok(_rtu.Procesar(reqs));
        }

        // Detecta Factura DTE (tiene "REFERENCIA")
        if (first.TryGetProperty("REFERENCIA", out _))
        {
            var reqs = body.Deserialize<List<FacturaRequest>>()!;
            return Ok(_dte.Procesar(reqs));
        }

        return BadRequest(new { error = "Formato JSON no reconocido." });
    }

    // ─── Reset ───────────────────────────────────────────────────
    [HttpPost("reset")]
    public IActionResult Reset()
    {
        _db.Reset();
        return Ok(new { mensaje = "Sistema reiniciado correctamente." });
    }

    // ─── Consultas ───────────────────────────────────────────────
    [HttpGet("ConsultaDatos")]
    public IActionResult ConsultaDatos(
        [FromQuery] string nit,
        [FromQuery] string tipo,
        [FromQuery] DateTime inicio,
        [FromQuery] DateTime fin)
        => Ok(_report.ConsultaDatos(nit, tipo, inicio, fin));

    [HttpGet("ResumenIva")]
    public IActionResult ResumenIva([FromQuery] DateTime fecha)
        => Ok(_report.ResumenIva(fecha));

    [HttpGet("ResumenRango")]
    public IActionResult ResumenRango([FromQuery] DateTime inicio, [FromQuery] DateTime fin)
        => Ok(_report.ResumenRango(inicio, fin));

    [HttpGet("ReporteRango")]
    public IActionResult ReporteRango([FromQuery] DateTime inicio, [FromQuery] DateTime fin)
        => Ok(_report.ReporteRango(inicio, fin));

    // ─── Datos para gráfica ──────────────────────────────────────
    [HttpPost("Grafica")]
    public IActionResult Grafica([FromBody] JsonElement body)
    {
        if (body.ValueKind != JsonValueKind.Object)
            return BadRequest(new { error = "Se esperaba un objeto JSON." });
        if (!body.TryGetProperty("tipo", out var tipoProp))
            return BadRequest(new { error = "Falta el campo 'tipo'." });

        string tipo = tipoProp.GetString()?.ToLower() ?? "";

        if (tipo == "iva" && body.TryGetProperty("fecha", out var fProp))
            return Ok(_report.ResumenIva(DateTime.Parse(fProp.GetString()!)));

        if (tipo == "rango"
            && body.TryGetProperty("inicio", out var iProp)
            && body.TryGetProperty("fin",    out var f2Prop))
            return Ok(_report.ResumenRango(
                DateTime.Parse(iProp.GetString()!),
                DateTime.Parse(f2Prop.GetString()!)));

        return BadRequest(new { error = "Tipo de gráfica no soportado." });
    }
}