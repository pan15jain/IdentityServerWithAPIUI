using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.Audience= builder.Configuration["Authentication:Audience"];

        //options.SaveToken = true;
        //options.TokenValidationParameters.ValidateAudience = true;
        //options.TokenValidationParameters.ValidateIssuer = true;
        //options.TokenValidationParameters.ValidateIssuerSigningKey = true;
        options.Configuration = new OpenIdConnectConfiguration();
        options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Authentication:Audience"]))
        };

    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policyBuilder =>
    {
        policyBuilder.RequireAuthenticatedUser().RequireClaim("scope", "https://www.trimark.com/api");
    });
});

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });

    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            ClientCredentials = new OpenApiOAuthFlow
            {
                TokenUrl = new Uri($"{builder.Configuration["Authentication:Authority"]}/connect/token"),
                Scopes = {{"https://www.trimark.com/api","API" }}
            }
        }
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference=new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="oauth2"
                }
            },
            new List<string>
            {
                "https://www.trimark.com/api"
            }
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
