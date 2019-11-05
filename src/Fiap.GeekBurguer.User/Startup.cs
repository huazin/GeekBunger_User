using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fiap.GeekBurguer.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace Fiap.GeekBurguer.Users
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<MyDbContext>(options =>
                 options.UseSqlServer(
                     Configuration.GetConnectionString("MyBase")));
            
            services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "GeekBurguer Users API", Version = "v1" });
            });

            var mvcCoreBuilder = services.AddMvcCore();
                        
            mvcCoreBuilder
            .AddFormatterMappings()
            .AddJsonFormatters()
            .AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "GeekBurguer Users API v1");
                c.RoutePrefix = string.Empty;
            });

            app.UseMvc();

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
