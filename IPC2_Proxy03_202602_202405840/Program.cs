using IPC2_Proxy03_202602_202405840.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5000");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddScoped<RtuService>();
builder.Services.AddScoped<DteService>();
builder.Services.AddScoped<ReportService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.MapControllers();
app.Run();