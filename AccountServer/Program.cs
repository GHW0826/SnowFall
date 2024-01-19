using AccountServer.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SharedDB;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<SharedDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SharedConnection")));


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
