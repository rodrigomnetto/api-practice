using ApiPractice.Contexts;
using ApiPractice.Identity.Services;
using ApiPractice.Identity.Services.Interfaces;
using ApiPractice.Maps.Favorite.PropertyResolver;
using ApiPractice.Repositories.Character;
using ApiPractice.Repositories.Favorite;
using ApiPractice.Repositories.Interfaces;
using ApiPractice.Repositories.User;
using ApiPractice.Services.Character;
using ApiPractice.Services.Favorite;
using ApiPractice.Services.Interfaces;
using ApiPractice.Services.User;
using ApiPractice.Settings;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ApiPractice
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            RegisterOptions(services);
            RegisterDatabase(services);
            
            RegisterServices(services);
            RegisterRepositories(services);

            services.AddControllers();
            services.AddHttpContextAccessor();
            RegisterAutoMapper(services);

            var key = Encoding.ASCII.GetBytes(Configuration.GetSection(AuthenticationSettings.Authentication).Get<AuthenticationSettings>().Secret);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
        }

        public void RegisterOptions(IServiceCollection services)
        {
            services.Configure<AuthenticationSettings>(Configuration.GetSection(AuthenticationSettings.Authentication));
        }

        public void RegisterDatabase(IServiceCollection services)
        {
            var databaseOptions = Configuration.GetSection(DatabaseSettings.Database).Get<DatabaseSettings>();
            services.AddDbContext<ApiPracticeDbContext>(options => options.UseNpgsql(databaseOptions.ConnectionString));
        }

        public void RegisterAutoMapper(IServiceCollection services)
        {
            services.AddTransient<UserResolver>();
            services.AddTransient<CharacterResolver>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(typeof(Startup).Assembly);
                cfg.ConstructServicesUsing(services.BuildServiceProvider().GetRequiredService);
            });

            services.AddSingleton(config.CreateMapper());
        }

        public void RegisterServices(IServiceCollection services)
        {
            services.AddTransient<IAuthenticationService, AuthenticationService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<ICharacterService, CharacterService>();
            services.AddTransient<IFavoriteCharacterService, FavoriteCharacterService>();
        }

        public void RegisterRepositories(IServiceCollection services)
        {
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<ICharacterRepository, CharacterRepository>();
            services.AddTransient<IFavoriteCharacterRepository, FavoriteCharacterRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
