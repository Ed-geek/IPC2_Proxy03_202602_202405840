using IPC2_Proxy03_202602_202405840.Models;

namespace IPC2_Proxy03_202602_202405840.Services;

public class ReportService
{
    private readonly DatabaseService _db;
    public ReportService(DatabaseService db) => _db = db;

    // 1) Consulta facturas emitidas/recibidas por NIT en un rango
    public object ConsultaDatos(string nit, string tipo, DateTime inicio, DateTime fin)
    {
        var fs = _db.LeerFacturas()
            .Where(x => x.EsValida && x.Fecha.Date >= inicio.Date && x.Fecha.Date <= fin.Date);

        fs = (tipo ?? "").ToLower() == "recibidas"
            ? fs.Where(x => x.NitReceptor == nit)
            : fs.Where(x => x.NitEmisor   == nit);

        var lista = fs.ToList();
        var porFecha = lista.GroupBy(x => x.Fecha.Date).OrderBy(g => g.Key)
            .Select(g => new {
                fecha      = g.Key.ToString("dd/MM/yyyy"),
                cantidad   = g.Count(),
                montoTotal = g.Sum(x => x.Total)
            });

        return new {
            nit, tipo,
            porFecha,
            totalFacturado = lista.Sum(x => x.Total)
        };
    }

    // 2) Resumen de IVA por NIT en una fecha específica
    public object ResumenIva(DateTime fecha)
    {
        var fs = _db.LeerFacturas()
            .Where(x => x.EsValida && x.Fecha.Date == fecha.Date)
            .ToList();

        var porNit = fs.Select(x => x.NitEmisor).Concat(fs.Select(x => x.NitReceptor))
            .Distinct()
            .Select(nit => new {
                nit,
                ivaEmitido  = fs.Where(x => x.NitEmisor  == nit).Sum(x => x.Iva),
                ivaRecibido = fs.Where(x => x.NitReceptor == nit).Sum(x => x.Iva)
            }).ToList();

        return new { fecha = fecha.ToString("dd/MM/yyyy"), porNit };
    }

    // 3) Resumen por rango: valor autorizado por fecha (con y sin IVA) + IVA por NIT
    public object ResumenRango(DateTime inicio, DateTime fin)
    {
        var fs = _db.LeerFacturas()
            .Where(x => x.EsValida && x.Fecha.Date >= inicio.Date && x.Fecha.Date <= fin.Date)
            .ToList();

        var porFecha = fs.GroupBy(x => x.Fecha.Date).OrderBy(g => g.Key)
            .Select(g => new {
                fecha       = g.Key.ToString("dd/MM/yyyy"),
                cantidad    = g.Count(),
                valorSinIva = g.Sum(x => x.Valor),
                valorConIva = g.Sum(x => x.Total)
            }).ToList();

        var porNit = fs.Select(x => x.NitEmisor).Concat(fs.Select(x => x.NitReceptor))
            .Distinct()
            .Select(nit => new {
                nit,
                ivaEmitido  = fs.Where(x => x.NitEmisor  == nit).Sum(x => x.Iva),
                ivaRecibido = fs.Where(x => x.NitReceptor == nit).Sum(x => x.Iva)
            }).ToList();

        return new {
            inicio = inicio.ToString("dd/MM/yyyy"),
            fin    = fin.ToString("dd/MM/yyyy"),
            valorAutorizadoPorFecha = porFecha,
            ivaPorNit = porNit
        };
    }

    // 4) Reporte completo por rango (rubric: todos los errores)
    public object ReporteRango(DateTime inicio, DateTime fin)
    {
        var fs = _db.LeerFacturas()
            .Where(x => x.Fecha.Date >= inicio.Date && x.Fecha.Date <= fin.Date)
            .ToList();

        object Stats(IEnumerable<Factura> g)
        {
            var l = g.ToList();
            return new {
                totalRecibidas = l.Count,
                totalEnviadasExitosamente = l.Count(x => x.EsValida),
                facturasSinErrores = l.Count(x => x.Errores.Count == 0),
                errores = new {
                    nitEmisorInvalido      = l.Count(x => x.Errores.Contains("NIT emisor inválido")),
                    nitReceptorInvalido    = l.Count(x => x.Errores.Contains("NIT receptor inválido")),
                    nitEmisorInexistente   = l.Count(x => x.Errores.Contains("NIT emisor inexistente")),
                    nitReceptorInexistente = l.Count(x => x.Errores.Contains("NIT receptor inexistente")),
                    ivaMalCalculado        = l.Count(x => x.Errores.Contains("IVA mal calculado")),
                    totalMalCalculado      = l.Count(x => x.Errores.Contains("TOTAL mal calculado")),
                    referenciaDuplicada    = l.Count(x => x.Errores.Contains("REFERENCIA duplicada"))
                },
                emisoresDistintos   = l.Where(x => x.EsValida).Select(x => x.NitEmisor).Distinct().Count(),
                receptoresDistintos = l.Where(x => x.EsValida).Select(x => x.NitReceptor).Distinct().Count()
            };
        }

        var porFecha = fs.GroupBy(x => x.Fecha.Date).OrderBy(g => g.Key)
            .Select(g => new { fecha = g.Key.ToString("dd/MM/yyyy"), stats = Stats(g) })
            .ToList();

        return new {
            inicio = inicio.ToString("dd/MM/yyyy"),
            fin    = fin.ToString("dd/MM/yyyy"),
            porFecha,
            totales = Stats(fs)
        };
    }
}