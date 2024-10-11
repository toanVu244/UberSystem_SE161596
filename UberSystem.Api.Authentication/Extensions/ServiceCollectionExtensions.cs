using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UberSystem.Domain.Entities;
using UberSystem.Domain.Interfaces;
using UberSystem.Domain.Interfaces.Services;
using UberSystem.Domain.Models;
using UberSystem.Infrastructure;
using UberSystem.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UberSystem.Domain.Repository;
using UberSystem.Infrastructure.Repository;
using UberSystem.Api.Authentication.Controllers;
namespace UberSystem.Api.Authentication.Extensions
{
   	public static class ServiceCollectionExtensions
	{
    	/// <summary>
    	/// Add needed instances for database
    	/// </summary>
    	/// <param name="services"></param>
    	/// <param name="configuration"></param>
    	/// <returns></returns>
    	public static IServiceCollection AddDatabase(this IServiceCollection services,ConfigurationManager configuration)
    	{
        	// Configure DbContext with Scoped lifetime  
            services.AddDbContext<UberSystemDbContext>(options =>
            	{
                    options.UseSqlServer(configuration.GetConnectionString("Default"),
                    	sqlOptions => sqlOptions.CommandTimeout(120));
                    // options.UseLazyLoadingProxies();
            	}
        	);
 
        	services.AddScoped<Func<UberSystemDbContext>>((provider) => () => provider.GetService<UberSystemDbContext>());
            services.AddScoped<DbFactory>();
        	services.AddScoped<IUnitOfWork, UnitOfWork>();

            //services.AddIdentity<User, IdentityRole>()
            //.AddEntityFrameworkStores<UberSystemDbContext>()
            //.AddDefaultTokenProviders();
            // Configure JWT authentication
            var jwtSettings = configuration.GetSection("JwtSettings");
			var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);
            services.Configure<AppSettings>(configuration.GetSection("JwtSettings"));

            services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
			.AddJwtBearer(options =>
			{
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            

            return services;
    	}
 
    	/// <summary>
    	/// Add instances of in-use services
    	/// </summary>
    	/// <param name="services"></param>
    	/// <returns></returns>
    	public static IServiceCollection AddServices(this IServiceCollection services)
    	{
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IDriverService, DriverService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ICabRepsitory, CabRepository>();
            services.AddScoped<IDriverRepository, DriverRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IGSPRepository, GSPRepository>();
            services.AddScoped<IGSPService, GSPService>();
            services.AddScoped<ITripRepository, TripRepository>();
            services.AddSingleton<DriverDataSenderService>();
            services.AddHostedService<DriverDataSenderService>();
            
            return services;
        }
	}
}