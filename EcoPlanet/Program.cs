using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EcoPlanet.Data;
using EcoPlanet.Areas.Identity.Data;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Amazon.XRay.Recorder.Handlers.AspNetCore;


var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("EcoPlanetContextConnection") ?? throw new InvalidOperationException("Connection string 'EcoPlanetContextConnection' not found.");

AWSSDKHandler.RegisterXRayForAllServices();

builder.Services.AddDbContext<EcoPlanetContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<EcoPlanetUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<EcoPlanetContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseXRay("EcoPlanet Server");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
