using Business.Abstract;
using Business.Concrete;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WebAPI.Middleware;
using Core.Utilities.Results;
using System.Text.Json;

namespace WebAPI
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
            services.AddControllers();

            var secret = "eb86155ce6eec9a0d24e23265fc75682cd0e4977a332246d3b000a5671a96e1f";

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = async context =>
                    {
                        if (!context.Response.HasStarted)
                        {
                            context.Response.StatusCode = 401;

                            context.Response.ContentType = "application/json";

                            var response = new ErrorResult("Unauthorized: Invalid or expired token");
                            var jsonResponse = JsonSerializer.Serialize(response);

                            await context.Response.WriteAsync(jsonResponse);
                        }

                        context.Response.CompleteAsync();
                    },
                    OnTokenValidated = context =>
                    {
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = async context =>
                    {
                        if (string.IsNullOrEmpty(context.Request.Headers.Authorization))
                        {
                            if (!context.Response.HasStarted)
                            {
                                context.Response.ContentType = "application/json";
                                context.Response.StatusCode = 401;

                                var response = new ErrorResult("Unauthorized: Token not provided");
                                var jsonResponse = JsonSerializer.Serialize(response);

                                await context.Response.WriteAsync(jsonResponse);
                            }

                            context.Response.CompleteAsync();
                        }
                    },
                };
            });

            services.AddSingleton<INotificationService, NotificationManager>();
            services.AddSingleton<INotificationDal, EfUserNotificationPreferenceDal>();

            //services.AddDependencyResolvers(new ICoreModule[] {
            //   new CoreModule()
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseMiddleware<CustomAuthenticationFailedMiddleware>();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}