using Microsoft.EntityFrameworkCore;
using Hospital.DAL.EF;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Додаємо CORS (щоб фронтенд міг стукатися до API)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(options =>
{
  
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ТВІЙ_ДУЖЕ_СЕКРЕТНИЙ_КЛЮЧ_ЯКИЙ_МАЄ_БУТИ_БІЛЬШЕ_16_СИМВОЛІВ"))
    };
});

builder.Services.AddAuthorization(); 

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Підключення БД (SQLite)
builder.Services.AddDbContext<HospitalDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<Hospital.BLL.Interfaces.IAuthService, Hospital.BLL.Services.AuthService>();
builder.Services.AddScoped<Hospital.BLL.Interfaces.IPatientService, Hospital.BLL.Services.PatientService>();
builder.Services.AddScoped<Hospital.BLL.Interfaces.IDoctorService, Hospital.BLL.Services.DoctorService>();
builder.Services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<Hospital.DAL.Interfaces.IUnitOfWork, Hospital.DAL.Repositories.UnitOfWork>();
var app = builder.Build();

// 2. Активація Swagger (тільки для розробки)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 3. Активація CORS (має стояти ПЕРЕД UseAuthorization)
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();