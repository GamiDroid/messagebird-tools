using MessagebirdTools.WebApp.Components;
using MessagebirdTools.WebApp.Services;
using MudBlazor.Services;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add Radzen services
builder.Services.AddRadzenComponents();

builder.Services.AddSingleton<IExcelService, ExcelService>();
builder.Services.AddScoped<IFilePathService, FilePathService>();
builder.Services.AddScoped<IMessagebirdService, MessagebirdService>();
builder.Services.AddHttpClient<IMessagebirdClient, MessagebirdClient>(client =>
{
    client.BaseAddress = new Uri("https://flows.messagebird.com");
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
