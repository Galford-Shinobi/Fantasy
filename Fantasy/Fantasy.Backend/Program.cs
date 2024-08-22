using Fantasy.Backend.Data;
using Fantasy.Backend.Helpers.Interfaces;
using Fantasy.Backend.Helpers.Repositories;
using Fantasy.Backend.Repositories.Implementations;
using Fantasy.Backend.Repositories.Interfaces;
using Fantasy.Backend.UnitsOfWork.Implementations;
using Fantasy.Backend.UnitsOfWork.Interfaces;
using Fantasy.Shared.Entites;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<DataContext>(x => x.UseSqlServer("name=LocalConnection"));
builder.Services.AddTransient<SeedDb>();
builder.Services.AddScoped<IFireBaseService, FireBaseService>();
builder.Services.AddScoped<IMailHelper, MailHelper>();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fantasy Backend", Version = "v1", Description = "This is a web api to work with the Fantasy. (Draco Master Orochi(ラコマスターオロチ))", });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. <br /> <br />
                      Enter 'Bearer' [space] and then your token in the text input below.<br /> <br />
                      Example: 'Bearer 12345abcdef'<br /> <br />",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
      {
        {
          new OpenApiSecurityScheme
          {
            Reference = new OpenApiReference
              {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
              },
              Scheme = "oauth2",
              Name = "Bearer",
              In = ParameterLocation.Header,
            },
            new List<string>()
          }
        });
});

builder.Services.AddScoped(typeof(IGenericUnitOfWork<>), typeof(GenericUnitOfWork<>));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddScoped<ICountriesUnitOfWork, CountriesUnitOfWork>();
builder.Services.AddScoped<ITeamsRepository, TeamsRepository>();
builder.Services.AddScoped<ITeamsUnitOfWork, TeamsUnitOfWork>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IUsersUnitOfWork, UsersUnitOfWork>();

//builder.Services.AddSwaggerGen(c =>
//{
//    //c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sales API", Version = "v1" });
//    c.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "Sales API",
//        Version = "v1",
//        Description = "This is a web api to work with the Fantasy.",
//        Contact = new OpenApiContact
//        {
//            Email = "dracomasterorochi@outlook.com",
//            Name = "Draco Master Orochi(ラコマスターオロチ)",
//            Url = new Uri("https://gavilan.blog")
//        },
//        License = new OpenApiLicense
//        {
//            Name = "MIT"
//        }
//    });
//    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Description = @"JWT Authorization header using the Bearer scheme. <br /> <br />
//                      Enter 'Bearer' [space] and then your token in the text input below.<br /> <br />
//                      Example: 'Bearer 12345abcdef'<br /> <br />",
//        Name = "Authorization",
//        In = ParameterLocation.Header,
//        Type = SecuritySchemeType.ApiKey,
//        Scheme = "Bearer"
//    });
//    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
//      {
//        {
//          new OpenApiSecurityScheme
//          {
//            Reference = new OpenApiReference
//              {
//                Type = ReferenceType.SecurityScheme,
//                Id = "Bearer"
//              },
//              Scheme = "oauth2",
//              Name = "Bearer",
//              In = ParameterLocation.Header,
//            },
//            new List<string>()
//          }
//        });
//});
builder.Services.AddIdentity<User, IdentityRole>(x =>
{
    x.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
    x.SignIn.RequireConfirmedEmail = true;
    x.User.RequireUniqueEmail = true;
    x.Password.RequireDigit = false;
    x.Password.RequiredUniqueChars = 0;
    x.Password.RequireLowercase = false;
    x.Password.RequireNonAlphanumeric = false;
    x.Password.RequireUppercase = false;
    x.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    x.Lockout.MaxFailedAccessAttempts = 3;
    x.Lockout.AllowedForNewUsers = true;
})
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(x => x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Tokens:Issuer"],
        ValidAudience = builder.Configuration["Tokens:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Tokens:jwtKey"]!)),
        ClockSkew = TimeSpan.Zero
    });
var app = builder.Build();
SeedData(app);

void SeedData(WebApplication app)
{
    var scopedFactory = app.Services.GetService<IServiceScopeFactory>();
    using var scope = scopedFactory!.CreateScope();
    var service = scope.ServiceProvider.GetService<SeedDb>();
    service!.SeedAsync().Wait();
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials());

app.Run();