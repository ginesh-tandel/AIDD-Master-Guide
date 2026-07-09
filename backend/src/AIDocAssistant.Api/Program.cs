using AIDocAssistant.Application.Common.Interfaces;
using AIDocAssistant.Infrastructure.Completions;
using AIDocAssistant.Infrastructure.Configuration;
using AIDocAssistant.Infrastructure.Embeddings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Config is read from appsettings.json (structure/defaults) + User Secrets in
// Development (dotnet user-secrets) + environment variables in Production.
// Never put real secrets in appsettings.json or appsettings.*.json — see AGENTS.md.
builder.Services.Configure<GeminiOptions>(
    builder.Configuration.GetSection(GeminiOptions.SectionName));

// Embedding/completion provider — Gemini, chosen for its free tier during
// prototyping. Swap the registration below to change providers; nothing
// elsewhere in Application/Api needs to change since both are interfaces.
builder.Services.AddHttpClient<IEmbeddingService, GeminiEmbeddingService>();
builder.Services.AddHttpClient<ICompletionService, GeminiCompletionService>();

// TODO: register IDocumentRepository once EF Core is added
// builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
