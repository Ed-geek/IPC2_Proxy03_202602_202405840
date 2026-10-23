using IPC2_Proxy03_202602_202405840.Models;
using IPC2_Proxy03_202602_202405840.Utils;

namespace IPC2_Proxy03_202602_202405840.Services;

public class RtuService
{
    private readonly DatabaseService _db;
    public RtuService(DatabaseService db) => _db = db;

    public RtuResponse Procesar(List<RtuRequest> requests)
    {
        var rtu = _db.LeerRtu();
        var resp = new RtuResponse();

        foreach (var r in requests)
        {
            if (!NitValidator.EsNitValido(r.Nit)) { resp.NitsInvalidos++; continue; }

            var existente = rtu.FirstOrDefault(c => c.Nit == r.Nit);
            if (existente is null)
            {
                rtu.Add(new Contribuyente { Nit = r.Nit, Nombre = r.Nombre });
                resp.ContribuyentesNuevos++;
            }
            else
            {
                existente.Nombre = r.Nombre;
                resp.ContribuyentesActualizados++;
            }
        }

        _db.GuardarRtu(rtu);
        return resp;
    }
}