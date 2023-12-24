using Business.Abstract;
using Business.Concrete;
using DataAccess.Abstract;
using DataAccess.Concrete.MongoDB.Context;
using DataAccess.Concrete.MongoDB.Repositories;
using StackExchange.Redis;

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

            services.AddSingleton(sp =>
            {
                var connectionString = Environment.GetEnvironmentVariable("ConnectionString");
                var databaseName = Environment.GetEnvironmentVariable("DatabaseName");

                return new MongoContext(connectionString, databaseName);
            });

            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                var connectionString = Environment.GetEnvironmentVariable("RedisConnectionString");

                var configuration = ConfigurationOptions.Parse(connectionString);
                return ConnectionMultiplexer.Connect(configuration);
            });

            services.AddDistributedMemoryCache();

            services.AddSingleton<IUserService, UserManager>();
            services.AddSingleton<IEmailService, EmailManager>();
            services.AddSingleton<IRedisService, RedisManager>();
            services.AddSingleton<ITokenService, TokenManager>();
            services.AddSingleton<IVerificationService, VerificationManager>();
            services.AddSingleton<IUserDal, MongoUserRepository>();

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

            //app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}