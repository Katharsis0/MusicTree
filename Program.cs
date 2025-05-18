using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MusicTree.Repositories;
using MusicTree.Services;
using MusicTree.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

//Add services to the container
builder.Services.AddControllers();

//Esto es para OpenAPI aka swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "MusicTree API", 
        Version = "v1" 
    });
});


//PostgreSQL DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//Register repositories and services
builder.Services.AddScoped<ClusterRepository>();
builder.Services.AddScoped<GenreRepository>();
builder.Services.AddScoped<IClusterService, ClusterService>();
builder.Services.AddScoped<IGenreService, GenreService>();

var app = builder.Build();

//Migración automática (esto hay q corregirlo mas adelante) 
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

//Esto tmbn es para swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();



app.Run();