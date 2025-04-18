using Microsoft.EntityFrameworkCore;
using WebApplication3;
using WebApplication3.Repositories;
using WebApplication3.Token;
using WebApplication3.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApplication3.Identities;
using WebApplication3.Users;
using System.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.UI.Services;
using WebApplication3.Models;
using WebApplication3.EmailSender;
using IEmailSender = WebApplication3.EmailSender.IEmailSender;

var builder = WebApplication.CreateBuilder(args); 
// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins("http://localhost:5173", "https://localhost:5173", "http://localhost:5176", "http://localhost:5174", "http://localhost:5175", "http://127.0.0.1:5500")
               .AllowAnyMethod()
               .AllowCredentials()
               .AllowAnyHeader();
    });
});

builder.Services.Configure<CustomTokenOptions>(builder.Configuration.GetSection("TokenOptions"));
builder.Services.Configure<Clients>(builder.Configuration.GetSection("Clients"));
// EmailSettings yapılandırmasını ayarlayın
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// EmailSender servisini kaydedin

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<IPointService, ApiService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
//sikinti
//builder.Services.AddScoped<IUserService2,UserService2>();
builder.Services.AddScoped<UserService2>();

builder.Services.AddScoped(typeof(IWriteRepository<>), typeof(WriteRepository<>));
builder.Services.AddScoped(typeof(IReadRepository<>), typeof(ReadRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


//Sikinti
//Add Identity
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
}).AddEntityFrameworkStores<CbsDbContext>()
.AddDefaultTokenProviders();



builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(3); // Token ömrü
});
builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    var tokenOptions = builder.Configuration.GetSection("TokenOptions").Get<CustomTokenOptions>();
    if (tokenOptions == null)
    {
        throw new InvalidOperationException("TokenOptions configuration section is missing or invalid.");
    }

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = tokenOptions.Issuer,
        ValidateIssuer = true,
        ValidAudiences = tokenOptions.Audience,
        ValidateAudience = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.Signature)),
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        //RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

    };
});
builder.Services.AddAuthorization(x =>
{
    x.AddPolicy("UpdatePolicy", y =>
    {y.RequireClaim("update", "true");});
    x.AddPolicy("DeletePolicy", y =>
    { y.RequireClaim("delete", "true"); });
    x.AddPolicy("CreatePolicy", y =>
    { y.RequireClaim("create", "true"); });

});

// Configure DbContext
builder.Services.AddDbContext<CbsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.UseNetTopologySuite()));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
    });


// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Use CORS
app.UseCors("AllowSpecificOrigin");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();



app.Run();