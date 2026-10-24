using System.Text.Json.Serialization;

namespace IPC2_Proxy03_202602_202405840.Models;

public class RtuResponse
{
    [JsonPropertyName("CONTRIBUYENTES_NUEVOS")]       public int ContribuyentesNuevos { get; set; }
    [JsonPropertyName("CONTRIBUYENTES_ACTUALIZADOS")] public int ContribuyentesActualizados { get; set; }
    [JsonPropertyName("NITS_INVALIDOS")]              public int NitsInvalidos { get; set; }
}

public class AutorizacionResponse
{
    [JsonPropertyName("resultadoAutorizaciones")]
    public ResultadoAutorizaciones Resultado { get; set; } = new();
}

public class ResultadoAutorizaciones
{
    // Los 4 campos
    [JsonPropertyName("cantidadAutorizacionesAprobadas")]                              public int CantidadAprobadas { get; set; }
    [JsonPropertyName("cantidadAutorizacionesRechazadasPorNitEmisorInexistente")]      public int RechazadasNitEmisor { get; set; }
    [JsonPropertyName("cantidadAutorizacionesRechazadasPorNitReceptorInexistente")]    public int RechazadasNitReceptor { get; set; }
    [JsonPropertyName("detalleAprobaciones")]                                          public List<DetalleAprobacion> Detalle { get; set; } = new();

    // Campos adicionales esperados
    [JsonPropertyName("cantidadAutorizacionesRechazadasPorNitEmisorInvalido")]         public int RechazadasNitEmisorInvalido { get; set; }
    [JsonPropertyName("cantidadAutorizacionesRechazadasPorNitReceptorInvalido")]       public int RechazadasNitReceptorInvalido { get; set; }
    [JsonPropertyName("cantidadAutorizacionesRechazadasPorIvaMalCalculado")]           public int RechazadasIva { get; set; }
    [JsonPropertyName("cantidadAutorizacionesRechazadasPorTotalMalCalculado")]         public int RechazadasTotal { get; set; }
    [JsonPropertyName("cantidadAutorizacionesRechazadasPorReferenciaDuplicada")]       public int RechazadasReferencia { get; set; }
    [JsonPropertyName("cantidadAutorizacionesRechazadas")]                             public int CantidadRechazadas { get; set; }
    [JsonPropertyName("cantidadDescartadasPorFormato")]                                public int Descartadas { get; set; }
}

public class DetalleAprobacion
{
    [JsonPropertyName("referencia")]       public string Referencia { get; set; } = "";
    [JsonPropertyName("codigoAprobacion")] public long CodigoAprobacion { get; set; }
}