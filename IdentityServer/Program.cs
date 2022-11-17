using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using IdentityServer.Data;
using IdentityServer.Factories;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDBContext>((serviceProvider, options) =>
{
    options.UseNpgsql(serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("Identity"),
        NpgsqlOptionsAction);
});
IdentityModelEventSource.ShowPII = true;
builder.Services.AddIdentity<TrimarkUser,IdentityRole>()
    .AddClaimsPrincipalFactory<TrimarkUserClaimsPrincipleFactory>()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDBContext>();

builder.Services.AddIdentityServer()
.AddAspNetIdentity<TrimarkUser>()
.AddConfigurationStore(configurationStoreOptions =>
{
    configurationStoreOptions.ResolveDbContextOptions = ResolveDbContextOptions;
})
.AddOperationalStore(operationalStoreOptions =>
{
    operationalStoreOptions.ResolveDbContextOptions = ResolveDbContextOptions;
});

builder.Services.AddRazorPages();
var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseIdentityServer();
app.UseAuthorization();
app.MapRazorPages();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    await scope.ServiceProvider.GetRequiredService<ApplicationDBContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.MigrateAsync();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<TrimarkUser>>();

    if (await userManager.FindByNameAsync("pjain") == null)
    {
        var xyz= await userManager.CreateAsync(
            new TrimarkUser
            {
                UserName = "pjain",
                Email = "pankaj.jain@trimarkassoc.com"
            }, "Password123!");
    }

    var configurationDbContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

    if (!await configurationDbContext.ApiResources.AnyAsync())
    {
        await configurationDbContext.ApiResources.AddAsync(new ApiResource
        {
            Name = "9fc33c2e-dbc1-4d0a-b212-68b9e07b3ba0",
            DisplayName = "API",
            Scopes = new List<string> { "https://www.trimark.com/api" }
        }.ToEntity());


        await configurationDbContext.SaveChangesAsync();
    }

    if (!await configurationDbContext.ApiScopes.AnyAsync())
    {
        await configurationDbContext.ApiScopes.AddAsync(new ApiScope
        {
            Name = "https://www.trimark.com/api",
            DisplayName = "API"
        }.ToEntity());

        await configurationDbContext.SaveChangesAsync();
    }

    if (!await configurationDbContext.Clients.AnyAsync())
    {
        await configurationDbContext.Clients.AddRangeAsync(
            new Client
            {
                ClientId = "b4e758d2-f13d-4a1e-bf38-cc88f4e290e1",
                ClientSecrets = new List<Secret> { new("secret".Sha512()) },
                ClientName = "Console Application",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = new List<string> { "https://www.trimark.com/api" },
                AllowedCorsOrigins = new List<string> { "https://api:7001" }
            }.ToEntity(),     
        new Client
        {
            ClientId = "7e98ad57-540a-4191-b477-03d88b8187e1",
            RequireClientSecret = false,
            ClientName = "BlazorUI",
            AllowedGrantTypes = GrantTypes.Code,
            AllowedScopes = new List<string> { "openid", "profile", "email", "https://www.trimark.com/api" },
            AllowedCorsOrigins = new List<string> { "https://localhost:7094" },
            RedirectUris =
                new List<string> { "https://localhost:7094/authentication/login-callback" },
            PostLogoutRedirectUris = new List<string>
                {
                    "https://localhost:7094/authentication/logout-callback"
                }
        }.ToEntity());

        await configurationDbContext.SaveChangesAsync();
    }

    if (!await configurationDbContext.IdentityResources.AnyAsync())
    {
        await configurationDbContext.IdentityResources.AddRangeAsync(
            new IdentityResources.OpenId().ToEntity(),
            new IdentityResources.Profile().ToEntity(),
            new IdentityResources.Email().ToEntity());

        await configurationDbContext.SaveChangesAsync();
    }
}

app.Run();

void NpgsqlOptionsAction(NpgsqlDbContextOptionsBuilder npgsqlDbContextOptionsBuilder)
{
    npgsqlDbContextOptionsBuilder.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name);
}

void ResolveDbContextOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder dbContextOptionsBuilder)
{
    dbContextOptionsBuilder.UseNpgsql(
        serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("IdentityServer"),
        NpgsqlOptionsAction);
}