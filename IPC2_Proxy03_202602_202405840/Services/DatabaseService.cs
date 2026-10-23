using System.Text.Json;
using IPC2_Proxy03_202602_202405840.Models;

namespace IPC2_Proxy03_202602_202405840.Services;

public class DatabaseService
{
    private readonly string _dir;
    private readonly JsonSerializerOptions _opts = new() { WriteIndented = true };
    private readonly object _lock = new();

    public DatabaseService()
    {
        _dir = Path.Combine(Directory.GetCurrentDirectory(), "Data");
        Directory.CreateDirectory(_dir);
    }

    public List<Contribuyente> LeerRtu()           => Leer<List<Contribuyente>>("Rtu.json");
    public void GuardarRtu(List<Contribuyente> r)  => Guardar("Rtu.json", r);

    public List<Factura> LeerFacturas()            => Leer<List<Factura>>("Facturas.json");
    public void GuardarFacturas(List<Factura> f)   => Guardar("Facturas.json", f);

    public long ObtenerSiguienteCorrelativo(DateTime fecha)
    {
        lock (_lock)
        {
            var path = Path.Combine(_dir, "Correlativos.json");
            var dict = File.Exists(path)
                ? JsonSerializer.Deserialize<Dictionary<string, long>>(File.ReadAllText(path)) ?? new()
                : new();

            string key = fecha.ToString("yyyyMMdd");
            long next = (dict.TryGetValue(key, out var last) ? last : 0) + 1;
            dict[key] = next;

            File.WriteAllText(path, JsonSerializer.Serialize(dict, _opts));
            return next;
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            if (!Directory.Exists(_dir)) return;
            foreach (var f in Directory.GetFiles(_dir, "*.json")) File.Delete(f);
        }
    }

    private T Leer<T>(string name) where T : new()
    {
        var p = Path.Combine(_dir, name);
        if (!File.Exists(p)) return new T();
        return JsonSerializer.Deserialize<T>(File.ReadAllText(p)) ?? new T();
    }

    private void Guardar<T>(string name, T data)
    {
        lock (_lock) File.WriteAllText(Path.Combine(_dir, name), JsonSerializer.Serialize(data, _opts));
    }
}