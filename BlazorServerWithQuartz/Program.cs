using System.Configuration;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using BlazorServerWithQuartz.Data;
using BlazorServerWithQuartz.Jobs;
using BlazorServerWithQuartz.Listeners;
using BlazorServerWithQuartz.Services;
using Microsoft.Extensions.FileSystemGlobbing;
using Quartz;
using Quartz.Impl.Matchers;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config")]

log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
log.Info("Blazor Server App Starting...");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddHostedService<ScheduledServiceBuilderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");


app.Run();