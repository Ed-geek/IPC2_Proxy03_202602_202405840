var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5001");
builder.Services.AddRazorPages();
builder.Services.AddHttpClient("api", c =>
    c.BaseAddress = new Uri("http://localhost:5000/api/"));

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();