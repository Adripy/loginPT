using APIuser;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using APIuser.Datos;
using APIuser.Repositorio;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting.Server;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var keyVaultEndpoint = builder.Configuration["Secrets:KeyVault:BaseUrl"];
var clientId = builder.Configuration["AzureAd:ClientId"];
var clientSecret = builder.Configuration["AzureAd:ClientSecret"];
var tenantId = builder.Configuration["AzureAd:TenantId"];

var secretClient = new SecretClient(new Uri(keyVaultEndpoint), new ClientSecretCredential(tenantId, clientId, clientSecret));

KeyVaultSecret conexion = await secretClient.GetSecretAsync("sqlConexion");
KeyVaultSecret credencialApi = await secretClient.GetSecretAsync("credencialApi");
KeyVaultSecret keyToken = await secretClient.GetSecretAsync("keyToken");

Environment.SetEnvironmentVariable("credencialApi", credencialApi.Value);
Environment.SetEnvironmentVariable("keyToken", keyToken.Value);

builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    option.UseSqlServer(conexion.Value);
});

builder.Services.AddAutoMapper(typeof(MappingConfig));

builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<IAccesoRepositorio, AccesoRepositorio>();

builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
