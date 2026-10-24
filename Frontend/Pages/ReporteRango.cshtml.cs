using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class ReporteRangoModel : PageModel
{
    [BindProperty] public DateTime Inicio { get; set; } = DateTime.Today.AddMonths(-1);
    [BindProperty] public DateTime Fin    { get; set; } = DateTime.Today;
    public string Resultado { get; set; } = "";
    private readonly IHttpClientFactory _http;
    public ReporteRangoModel(IHttpClientFactory http) => _http = http;

    public async Task OnPostAsync()
    {
        var client = _http.CreateClient("api");
        var json = await client.GetStringAsync($"ReporteRango?inicio={Inicio:yyyy-MM-dd}&fin={Fin:yyyy-MM-dd}");
        using var doc = JsonDocument.Parse(json);
        Resultado = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
    }
}