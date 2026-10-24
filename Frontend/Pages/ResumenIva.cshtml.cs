using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class ResumenIvaModel : PageModel
{
    [BindProperty] public DateTime Fecha { get; set; } = DateTime.Today;
    public string Resultado { get; set; } = "";
    public string GraphJson { get; set; } = "[]";
    private readonly IHttpClientFactory _http;
    public ResumenIvaModel(IHttpClientFactory http) => _http = http;

    public async Task OnPostAsync()
    {
        var client = _http.CreateClient("api");
        var json = await client.GetStringAsync($"ResumenIva?fecha={Fecha:yyyy-MM-dd}");
        using var doc = JsonDocument.Parse(json);
        Resultado = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
        GraphJson = doc.RootElement.GetProperty("porNit").GetRawText();
    }
}