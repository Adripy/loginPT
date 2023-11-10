using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Webuser.Services;
using Microsoft.IdentityModel.Logging;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);
IdentityModelEventSource.ShowPII = true;
// Add services to the container.
builder.Services.AddControllersWithViews();

var keyVaultEndpoint = builder.Configuration["Secrets:KeyVault:BaseUrl"];
var clientId = builder.Configuration["AzureAd:ClientId"];
var clientSecret = builder.Configuration["AzureAd:ClientSecret"];
var tenantId = builder.Configuration["AzureAd:TenantId"];

var secretClient = new SecretClient(new Uri(keyVaultEndpoint), new ClientSecretCredential(tenantId, clientId, clientSecret));

KeyVaultSecret credencialApi = await secretClient.GetSecretAsync("credencialApi");
KeyVaultSecret keyToken = await secretClient.GetSecretAsync("keyToken");

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                     .AddJsonFile($"appsettings.Production.json", optional: true)
                     .AddEnvironmentVariables();

builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
                                            {
                                                { "credencialApi", credencialApi.Value },
                                                { "keyToken", keyToken.Value }
                                            });

builder.Services.AddScoped<IServiceApi, ServiceApi>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/login";
        option.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        option.AccessDeniedPath = "/login";
    });

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

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Web}/{action=Login}/{id?}");

app.Run();
