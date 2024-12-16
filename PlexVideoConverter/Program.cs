using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.OpenApi.Models;
using NLog.Extensions.Logging;
using PlexVideoConverter.Models;
using PlexVideoConverter.Services;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "PVC API",
        Description = "An ASP.NET Core Web API for managing the PVC application",
        Contact = new OpenApiContact
        {
            Name = "Example Contact",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }
    });
});
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "ClientApp/pvc-app/dist";
});
builder.Logging.ClearProviders();
builder.Host.UseNLog();


var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

NLog.LogManager.Configuration = new NLogLoggingConfiguration(config.GetSection("NLog"));

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

app.UseSpa(spa =>
{
    spa.Options.SourcePath = "ClientApp/pvc-app";
    if (app.Environment.IsDevelopment()) spa.UseAngularCliServer(npmScript: "start");
});

app.UseRouting();

app.UseAuthorization();
app.UseAuthorization();
app.MapControllers();

//app.MapSwagger();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
//Todo test if this is needed
app.MapFallbackToFile("ClientApp/pvc-app/dist/pvc-app/browser/index.html");

SettingsService.Instance.FfmpegSettings = config.GetSection("FfmpegSettings").Get<FfmpegSettings>();

app.Run();