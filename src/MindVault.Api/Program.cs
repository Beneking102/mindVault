var builder = WebApplication.CreateBuilder(args);

// (Hier später: AddServices, DbContexts, Identity, CORS, etc.)

var app = builder.Build();

app.MapGet("/", () => Results.Ok("MindVault API running"));
app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow }));

app.Run();
