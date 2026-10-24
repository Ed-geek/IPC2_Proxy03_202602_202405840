using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class EnviarMensajeModel : PageModel
{
    [BindProperty] public IFormFile? Archivo { get; set; }
    public string Respuesta { get; set; } = "";
    private readonly IHttpClientFactory _http;
    public EnviarMensajeModel(IHttpClientFactory http) => _http = http;

    public async Task OnPostAsync()
    {
        if (Archivo is null) { Respuesta = "Selecciona un archivo."; return; }

        using var reader = new StreamReader(Archivo.OpenReadStream());
        var content = await reader.ReadToEndAsync();

        var client = _http.CreateClient("api");
        using var httpContent = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("procesar", httpContent);

        var raw = await response.Content.ReadAsStringAsync();
        try
        {
            using var doc = JsonDocument.Parse(raw);
            Respuesta = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
        }
        catch { Respuesta = raw; }
    }
}