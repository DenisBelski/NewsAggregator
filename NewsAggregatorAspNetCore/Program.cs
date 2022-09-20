using Microsoft.EntityFrameworkCore;
using NewsAggregator.Business.ServicesImplementations;
using NewsAggregator.Core;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.DataBase;
using Serilog;
using Serilog.Events;

namespace NewsAggregatorAspNetCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //builder.Host.UseSerilog((ctx, lc) =>
            //    lc.WriteTo.File(@"D:\IT\GitHub_Projects\NewsAggregator\data.log",
            //    LogEventLevel.Information));

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var connectionString = builder.Configuration.GetConnectionString("Default");
            //var connectionString = "Server=DESKTOP-LNVP1TV;Database=NewsAggregatorDataBase;Trusted_Connection=True;";

            builder.Services.AddDbContext<NewsAggregatorContext>(optionsBuilder => optionsBuilder.UseSqlServer(connectionString));
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            builder.Services.AddScoped<IArticleService, ArticleService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}