using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using NewsAggregator.Business.ServicesImplementations;
using NewsAggregator.Core;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Data.Abstractions;
using NewsAggregator.Data.Abstractions.Repositories;
using NewsAggregator.Data.Repositories;
using NewsAggregator.DataBase;
using NewsAggregator.DataBase.Entities;
using Serilog;
using Serilog.Events;

namespace NewsAggregatorAspNetCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((ctx, lc) =>
                lc.WriteTo.File(@"D:\IT\GitHub_Projects\NewsAggregator\dataMvc.log",
                LogEventLevel.Information));

            // Add services to the container.
            builder.Services.AddControllersWithViews();



            // !!! change pathString
            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);
                    options.LoginPath = new PathString(@"/Account/Login");
                    options.LogoutPath = new PathString(@"/Account/Logout");
                    options.AccessDeniedPath = new PathString(@"/Account/Login");
                });

            var connectionString = builder.Configuration.GetConnectionString("Default");
            builder.Services.AddDbContext<NewsAggregatorContext>(
                optionsBuilder => optionsBuilder.UseSqlServer(connectionString));




            //builder.Services
            //    .AddIdentity<ApplicationUser, IdentityRole<Guid>>()
            //    .AddEntityFrameworkStores<AppDbContext>()
            //    .AddDefaultTokenProviders();




            //builder.Services.AddHangfire(configuration => configuration
            //    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            //    .UseSimpleAssemblyNameTypeSerializer()
            //    .UseRecommendedSerializerSettings()
            //    .UseSqlServerStorage(connectionString,
            //        new SqlServerStorageOptions
            //        {
            //            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            //            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            //            QueuePollInterval = TimeSpan.Zero,
            //            UseRecommendedIsolationLevel = true,
            //            DisableGlobalLocks = true,
            //        }));

            //// Add the processing server as IHostedService
            //builder.Services.AddHangfireServer();



            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            

            
            builder.Services.AddScoped<IArticleService, ArticleService>();
            builder.Services.AddScoped<ISourceService, SourceService>();
            builder.Services.AddScoped<IRssService, RssService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IAdditionalArticleRepository, ArticleGenericRepository>();
            builder.Services.AddScoped<IRepository<Source>, Repository<Source>>();
            builder.Services.AddScoped<IRepository<User>, Repository<User>>();
            builder.Services.AddScoped<IRepository<Role>, Repository<Role>>();
            builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
            builder.Services.AddScoped<ISourceRepository, SourceRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            //builder.Services.AddScoped<ArticleCheckerActionFilter>();

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

            //app.UseHangfireDashboard();

            app.UseRouting();

            //app.MapHangfireDashboard(); // check the place to connect this middleware, as an option: plase before app.Run()

            app.UseAuthentication(); // Set HttpContext.User, add to context automatically 
            app.UseAuthorization(); // check access to resource for user

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}