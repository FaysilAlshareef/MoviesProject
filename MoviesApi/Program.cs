using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoviesApi.Extensions;
using MoviesApi.Helpers;
using MoviesApi.Models;
using MoviesApi.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//JWT
builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT"));


// Add services to the container.



builder.Services.AddDbContext<ApplicationDbContext>(options=>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


//Inject Application Services
builder.Services.AddApplicationServices();

builder.Services.AddAuthServices(builder.Configuration);




builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(C=> C.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
