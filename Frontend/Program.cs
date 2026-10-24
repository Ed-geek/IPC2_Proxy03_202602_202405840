var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5001");
builder.Services.AddRazorPages();
builder.Services.AddHttpClient("api", c =>
    c.BaseAddress = new Uri("http://localhost:5000/api/"));

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.MapPost("/Inicializar", async (IHttpClientFactory http) =>
{
    var client = http.CreateClient("api");
    var resp = await client.PostAsync("reset", null);
    return Results.Content(await resp.Content.ReadAsStringAsync(), "application/json");
});

app.MapPost("/Procesar", async (HttpRequest req, IHttpClientFactory http) =>
{
    using var reader = new StreamReader(req.Body);
    var body = await reader.ReadToEndAsync();
    var client = http.CreateClient("api");
    using var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
    var resp = await client.PostAsync("procesar", content);
    return Results.Content(await resp.Content.ReadAsStringAsync(), "application/json",
        statusCode: (int)resp.StatusCode);
});

app.Run();