using Hangfire;
using Hangfire.SqlServer;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NewsAggregator.Business.ServicesImplementations;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Data.Abstractions;
using NewsAggregator.Data.Abstractions.Repositories;
using NewsAggregator.Data.CQS.Commands;
using NewsAggregator.Data.Repositories;
using NewsAggregator.Data.Repositories.Implementations;
using NewsAggregator.DataBase;
using NewsAggregator.DataBase.Entities;
using NewsAggregator.WebAPI.Utils;
using Serilog;
using Serilog.Events;
using System.Text;

namespace NewsAggregator.WebAPI
{
    /// <summary>
    /// The class to start the application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The Main method is the entry point of an application. 
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((ctx, lc) =>
                lc.WriteTo.File(builder.Configuration["Serilog:LogFilePath"],
                LogEventLevel.Information)
                .WriteTo.Console(LogEventLevel.Verbose));

            builder.Services.AddControllers();

            var connectionString = builder.Configuration.GetConnectionString("Default");
            builder.Services.AddDbContext<NewsAggregatorContext>(
                optionsBuilder => optionsBuilder.UseSqlServer(connectionString));

            //!!! Read documentation. Adding Dashboard UI. Dashboard authorization must be configured in order to allow remote access.
            builder.Services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString,
                    new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.Zero,
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true,
                    }));

            builder.Services.AddHangfireServer();
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IRssService, RssService>();
            builder.Services.AddScoped<ISourceService, SourceService>();
            builder.Services.AddScoped<IArticleService, ArticleService>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IRepository<Role>, Repository<Role>>();
            builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
            builder.Services.AddScoped<ISourceRepository, SourceRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IJwtUtil, JwtUtilSha256>();

            builder.Services.AddMediatR(typeof(AddArticleDataFromRssFeedCommand).Assembly);
            builder.Services.AddMediatR(typeof(UpdateArticleTextCommand).Assembly);
            //builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
            //builder.Services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
                {
                    options.IncludeXmlComments(builder.Configuration["XmlDoc"]);
                });

            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(opt =>
                {
                    opt.RequireHttpsMetadata = false;
                    opt.SaveToken = true;
                    opt.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidIssuer = builder.Configuration["Token:Issuer"],
                        ValidAudience = builder.Configuration["Token:Issuer"],
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Token:JwtSecret"])),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            var app = builder.Build();

            app.UseStaticFiles();
            app.UseHangfireDashboard();

            app.UseRouting();
            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHangfireDashboard();
            app.MapControllers();

            app.Run();
        }
    }
}