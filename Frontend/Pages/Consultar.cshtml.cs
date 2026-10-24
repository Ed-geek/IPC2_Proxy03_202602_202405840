using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class ConsultarModel : PageModel
{
    [BindProperty] public string Nit   { get; set; } = "";
    [BindProperty] public string Tipo  { get; set; } = "emitidas";
    [BindProperty] public DateTime Inicio { get; set; } = DateTime.Today.AddMonths(-1);
    [BindProperty] public DateTime Fin    { get; set; } = DateTime.Today;
    public string Resultado { get; set; } = "";
    private readonly IHttpClientFactory _http;
    public ConsultarModel(IHttpClientFactory http) => _http = http;

    public async Task OnPostAsync()
    {
        var client = _http.CreateClient("api");
        var url = $"ConsultaDatos?nit={Nit}&tipo={Tipo}&inicio={Inicio:yyyy-MM-dd}&fin={Fin:yyyy-MM-dd}";
        var json = await client.GetStringAsync(url);
        using var doc = JsonDocument.Parse(json);
        Resultado = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
    }
}