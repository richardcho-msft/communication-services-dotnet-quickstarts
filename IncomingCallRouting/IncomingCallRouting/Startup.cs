using IncomingCallRouting.Controllers;
using IncomingCallRouting.Models;
using IncomingCallRouting.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace IncomingCallRouting
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
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "IncomingCallRouting", Version = "v1" });
            });
            services.AddGrpc();
            services.AddSingleton<IIncomingCallEventService, IncomingCallEventService>();
            var connectionManagerOptions = new ConnectionManagerOptions();
            Configuration.Bind("ConnectionManager", connectionManagerOptions);
            services.AddSingleton(connectionManagerOptions);
            services.AddSingleton<IConnectionManager, ConnectionManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "IncomingCallRouting v1"));
            }

            // app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGrpcService<IncomingCallRpcController>();
                endpoints.MapGrpcService<GreeterService>();
            });
        }
    }
}
