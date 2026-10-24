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

        var cola = new LinkedQueue<FacturaRequest>();
        foreach (var r in requests) cola.Enqueue(r);

        while (!cola.IsEmpty)
        {
            var req = cola.Dequeue();
            var f = new Factura
            {
                Referencia  = req.Referencia ?? "",
                NitEmisor   = (req.NitEmisor ?? "").Trim().ToUpper(),
                NitReceptor = (req.NitReceptor ?? "").Trim().ToUpper(),
                Valor       = req.Valor,
                Iva         = req.Iva,
                Total       = req.Total
            };

            // Fecha (Regex)
            try { f.Fecha = RegexExtractor.ExtraerFecha(req.Tiempo); }
            catch
            {
                f.DescartadaPorFormato = true;
                f.Errores.Add("Fecha inválida");
                resp.Resultado.Descartadas++;
                facturas.Add(f);
                continue;   // No procesar más: sin fecha no hay nada que validar
            }

            if (string.IsNullOrWhiteSpace(f.Referencia) || f.Referencia.Length > 40)
                f.Errores.Add("REFERENCIA inválida");

            if (f.Valor <= 0) f.Errores.Add("VALOR inválido");

            // NIT emisor
            bool emisorFormato = NitValidator.EsNitValido(f.NitEmisor);
            bool emisorExiste  = emisorFormato && rtuNits.Contains(f.NitEmisor);
            if (!emisorFormato)
            {
                f.Errores.Add("NIT emisor inválido");
                resp.Resultado.RechazadasNitEmisorInvalido++;
            }
            else if (!emisorExiste)
            {
                f.Errores.Add("NIT emisor inexistente");
                resp.Resultado.RechazadasNitEmisor++;
            }

            // NIT receptor
            bool receptorFormato = NitValidator.EsNitValido(f.NitReceptor);
            bool receptorExiste  = receptorFormato && rtuNits.Contains(f.NitReceptor);
            if (!receptorFormato)
            {
                f.Errores.Add("NIT receptor inválido");
                resp.Resultado.RechazadasNitReceptorInvalido++;
            }
            else if (!receptorExiste)
            {
                f.Errores.Add("NIT receptor inexistente");
                resp.Resultado.RechazadasNitReceptor++;
            }

            // IVA
            decimal ivaEsperado = Math.Round(f.Valor * 0.12m, 2, MidpointRounding.AwayFromZero);
            if (f.Iva != ivaEsperado)
            {
                f.Errores.Add("IVA mal calculado");
                resp.Resultado.RechazadasIva++;
            }

            // TOTAL
            if (f.Total != f.Valor + f.Iva)
            {
                f.Errores.Add("TOTAL mal calculado");
                resp.Resultado.RechazadasTotal++;
            }

            // REFERENCIA duplicada (solo contra aprobadas en BD o procesadas antes en este mensaje)
            if (refsAprobadas.Contains(f.Referencia))
            {
                f.Errores.Add("REFERENCIA duplicada");
                resp.Resultado.RechazadasReferencia++;
            }

            // Autorizar
            if (f.Errores.Count == 0)
            {
                f.EsValida = true;
                // Importante: código de aprobación usa la fecha de HOY (del sistema)
                var ahora = DateTime.Now;
                long correl = _db.ObtenerSiguienteCorrelativo(ahora);
                f.CodigoAprobacion = $"{ahora:yyyyMMdd}{correl:D8}";

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
                resp.Resultado.CantidadRechazadas++;
            }

            facturas.Add(f);
        }

        _db.GuardarFacturas(facturas);
        return resp;
    }
}