using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UberSystem.Domain.Interfaces;
using UberSystem.Domain.Repository;
using UberSystem.Infrastructure;
using UberSystem.Infrastructure.Repository;
using UberSystem.Service;
 
namespace UberSystem.Api.Customer.Extensions
{
   	public static class ServiceCollectionExtensions
	{
        /// <summary>
        /// Add needed instances for database
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddDatabase(this IServiceCollection services, ConfigurationManager configuration)
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

            //        services.AddIdentity<User, IdentityRole>()
            //.AddEntityFrameworkStores<UberSystemDbContext>()
            //.AddDefaultTokenProviders();
            // Configure JWT authentication
            //var jwtSettings = configuration.GetSection("JwtSettings");
            //var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);

            //services.AddAuthentication(options =>
            //{
            //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //})
            //.AddJwtBearer(options =>
            //{
            //    options.RequireHttpsMetadata = false;
            //    options.SaveToken = true;
            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidateIssuerSigningKey = true,
            //        IssuerSigningKey = new SymmetricSecurityKey(key),
            //        ValidateIssuer = false,
            //        ValidateAudience = false
            //    };
            //});
            return services;
        }

        /// <summary>
        /// Add instances of in-use services
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddServices(this IServiceCollection services)
    	{
            
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ICabRepsitory, CabRepository>();
            services.AddScoped<IDriverRepository, DriverRepository>();
            services.AddScoped<ITripRepository, TripRepository>();
            services.AddScoped<IGSPRepository, GSPRepository>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IRatingRepository, RatingRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            return services;
    	}
	}
}