using System.Diagnostics;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using PlexVideoConverter.Models;
using PlexVideoConverter.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "ClientApp/pvc-app/dist";
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "PVC API",
        Description = "An ASP.NET Core Web API for managing the PVC application"
    });
});
builder.Services.AddMvc(option => option.EnableEndpointRouting = false);
builder.Services.AddSingleton<FileListenerService>();
builder.Services.AddSingleton<FfmpegCoreService>();
builder.Logging.ClearProviders();
builder.Host.UseNLog();


var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

LogManager.Configuration = new NLogLoggingConfiguration(config.GetSection("NLog"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(options => 
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = "swagger";
    });
}


app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();
if (!app.Environment.IsDevelopment()) app.UseSpaStaticFiles();

app.UseCors(corsBuilder =>
{ 
    corsBuilder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); // Configures CORS to allow any origin, header, and method.
});

app.UseRouting();

app.MapControllers();

app.UseMvc(routes =>
{
    routes.MapRoute(
        name: "default",
        template: "{controller=Home}/{action=Index}");
});

var excludedPaths = new PathString[] { "/api" };

app.UseWhen((ctx) =>
{
    var path = ctx.Request.Path;
    return !Array.Exists(excludedPaths,
        excluded => path.StartsWithSegments(excluded, StringComparison.OrdinalIgnoreCase));
}, then =>
{
    if (builder.Environment.IsProduction())
    {
        then.UseSpaStaticFiles();
    }

    then.UseSpa(spa =>
    {
        spa.Options.SourcePath = "ClientApp/pvc-app";
        if (app.Environment.IsDevelopment())
        {
            //The below should work but isn't
            //spa.UseAngularCliServer(npmScript: "start");
            string angularProjectPath = spa.Options.SourcePath;

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "npm",
                Arguments = "start",
                WorkingDirectory = angularProjectPath,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Minimized
            };

            Process? npmProcess = Process.Start(psi);
            SettingsService.Instance.npmProcess = npmProcess;

            spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
        }
    });
});

//Todo test if this is needed
// app.MapFallbackToFile("ClientApp/pvc-app/dist/pvc-app/browser/index.html");

SettingsService.Instance.FfmpegSettings = config.GetSection("FfmpegSettings").Get<FfmpegSettings>();
SettingsService.Instance.PopulateGlobalSettings();

await app.StartAsync();

app.WaitForShutdown();