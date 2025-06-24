using Microsoft.EntityFrameworkCore;
using MusicTree.Repositories;
using MusicTree.Services;
using MusicTree.Services.Interfaces;

namespace MusicTree
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:5173")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            // Add services to the container
            builder.Services.AddControllers();

            // Add DbContext with PostgreSQL
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add repositories
            builder.Services.AddScoped<ClusterRepository>();
            builder.Services.AddScoped<GenreRepository>();
            builder.Services.AddScoped<ArtistRepository>();


            // Add services
            builder.Services.AddScoped<IClusterService, ClusterService>();
            builder.Services.AddScoped<IGenreService, GenreService>();
            builder.Services.AddScoped<IArtistService, ArtistService>();
            builder.Services.AddScoped<GenreImportService>();

            // Add API explorer for development
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowFrontend");

            app.UseAuthorization();
            app.MapControllers();
            
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            }

            app.Run();
            
           
        }
    }
}