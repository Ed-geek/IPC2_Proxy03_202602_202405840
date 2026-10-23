using IPC2_Proxy03_202602_202405840.Models;
using IPC2_Proxy03_202602_202405840.Utils;

namespace IPC2_Proxy03_202602_202405840.Services;

public class DteService
{
    private readonly DatabaseService _db;
    public DteService(DatabaseService db) => _db = db;

    public AutorizacionResponse Procesar(List<FacturaRequest> requests)
    {
        var rtuNits = _db.LeerRtu().Select(c => c.Nit).ToHashSet();
        var facturas = _db.LeerFacturas();
        var refsAprobadas = facturas.Where(f => f.EsValida).Select(f => f.Referencia).ToHashSet();
        var resp = new AutorizacionResponse();

        // Uso del TDA propio: procesamos en orden FIFO
        var cola = new LinkedQueue<FacturaRequest>();
        foreach (var r in requests) cola.Enqueue(r);

        while (!cola.IsEmpty)
        {
            var req = cola.Dequeue();
            var f = new Factura
            {
                Referencia  = req.Referencia ?? "",
                NitEmisor   = (req.NitEmisor   ?? "").Trim().ToUpper(),
                NitReceptor = (req.NitReceptor ?? "").Trim().ToUpper(),
                Valor       = req.Valor,
                Iva         = req.Iva,
                Total       = req.Total
            };

            // 1) Fecha con Regex (ignora "Guatemala, ... hrs.")
            try { f.Fecha = RegexExtractor.ExtraerFecha(req.Tiempo); }
            catch { f.Errores.Add("Fecha inválida"); }

            // 2) REFERENCIA válida (máx 40 posiciones)
            if (string.IsNullOrWhiteSpace(f.Referencia) || f.Referencia.Length > 40)
                f.Errores.Add("REFERENCIA inválida");

            // 3) VALOR real positivo
            if (f.Valor <= 0) f.Errores.Add("VALOR inválido");

            // 4) NIT emisor
            bool emisorFormato = NitValidator.EsNitValido(f.NitEmisor);
            bool emisorExiste  = emisorFormato && rtuNits.Contains(f.NitEmisor);
            bool recEmisorInexistente = false;
            if (!emisorFormato) f.Errores.Add("NIT emisor inválido");
            else if (!emisorExiste) { f.Errores.Add("NIT emisor inexistente"); recEmisorInexistente = true; }

            // 5) NIT receptor
            bool receptorFormato = NitValidator.EsNitValido(f.NitReceptor);
            bool receptorExiste  = receptorFormato && rtuNits.Contains(f.NitReceptor);
            bool recReceptorInexistente = false;
            if (!receptorFormato) f.Errores.Add("NIT receptor inválido");
            else if (!receptorExiste) { f.Errores.Add("NIT receptor inexistente"); recReceptorInexistente = true; }

            // 6) IVA
            decimal ivaEsperado = Math.Round(f.Valor * 0.12m, 2, MidpointRounding.AwayFromZero);
            if (f.Iva != ivaEsperado) f.Errores.Add("IVA mal calculado");

            // 7) TOTAL
            if (f.Total != f.Valor + f.Iva) f.Errores.Add("TOTAL mal calculado");

            // 8) REFERENCIA duplicada (solo contra aprobadas)
            if (f.Errores.Count == 0 && refsAprobadas.Contains(f.Referencia))
                f.Errores.Add("REFERENCIA duplicada");

            // 9) Autorizar si no hay errores
            if (f.Errores.Count == 0)
            {
                f.EsValida = true;
                long correl = _db.ObtenerSiguienteCorrelativo(f.Fecha);

                // El PDF muestra un código de 16 dígitos (ej. 2026092100000001).
                // Usamos D8 para reproducir el ejemplo exacto del enunciado.
                f.CodigoAprobacion = $"{f.Fecha:yyyyMMdd}{correl:D8}";

                refsAprobadas.Add(f.Referencia);
                resp.Resultado.CantidadAprobadas++;
                resp.Resultado.Detalle.Add(new DetalleAprobacion
                {
                    Referencia = f.Referencia,
                    CodigoAprobacion = long.Parse(f.CodigoAprobacion)
                });
            }
            else
            {
                if (recEmisorInexistente)   resp.Resultado.RechazadasNitEmisor++;
                if (recReceptorInexistente) resp.Resultado.RechazadasNitReceptor++;
            }

            facturas.Add(f); // Guardamos TODAS (válidas e inválidas) para el reporte
        }

        _db.GuardarFacturas(facturas);
        return resp;
    }
}