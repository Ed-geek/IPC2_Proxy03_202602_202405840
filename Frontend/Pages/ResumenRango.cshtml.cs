using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class ResumenRangoModel : PageModel
{
    [BindProperty] public DateTime Inicio { get; set; } = DateTime.Today.AddMonths(-1);
    [BindProperty] public DateTime Fin    { get; set; } = DateTime.Today;
    public string Resultado { get; set; } = "";
    public string GraphJson { get; set; } = "[]";
    private readonly IHttpClientFactory _http;
    public ResumenRangoModel(IHttpClientFactory http) => _http = http;

    public async Task OnPostAsync()
    {
        var client = _http.CreateClient("api");
        var json = await client.GetStringAsync($"ResumenRango?inicio={Inicio:yyyy-MM-dd}&fin={Fin:yyyy-MM-dd}");
        using var doc = JsonDocument.Parse(json);
        Resultado = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
        GraphJson = doc.RootElement.GetProperty("ivaPorNit").GetRawText();
    }
}