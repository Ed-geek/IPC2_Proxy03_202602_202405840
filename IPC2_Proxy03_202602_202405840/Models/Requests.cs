using System.Text.Json.Serialization;

namespace IPC2_Proxy03_202602_202405840.Models;

public class RtuRequest
{
    [JsonPropertyName("NIT")]    public string Nit { get; set; } = "";
    [JsonPropertyName("nombre")] public string Nombre { get; set; } = "";
}

public class FacturaRequest
{
    [JsonPropertyName("TIEMPO")]       public string Tiempo { get; set; } = "";
    [JsonPropertyName("REFERENCIA")]   public string Referencia { get; set; } = "";
    [JsonPropertyName("NIT EMISOR")]   public string NitEmisor { get; set; } = "";
    [JsonPropertyName("NIT RECEPTOR")] public string NitReceptor { get; set; } = "";
    [JsonPropertyName("VALOR")]        public decimal Valor { get; set; }
    [JsonPropertyName("IVA")]          public decimal Iva { get; set; }
    [JsonPropertyName("TOTAL")]        public decimal Total { get; set; }
}