using FluentValidation.AspNetCore;
using Hqv.Thermostat.Api.Handlers;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;

namespace Hqv.Thermostat.Api
{
    public class Startup
    {
        public static IConfigurationRoot Configuration;

        public Startup(IHostingEnvironment env)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Error()
                .WriteTo.File("log\\global_logs.json")                
                .CreateLogger();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appSettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                // Add MVC
                .AddMvc(action =>
                {
                    // Return 406 if we cannot generate the response in the requested format 
                    // Can output in XML
                    // Can accept XML 
                    action.ReturnHttpNotAcceptable = true;
                    action.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                    action.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());
                })
                // Shaping the return result will make the properties uppercase. This JSON option
                //will make it lower case. Reason is the returned object is an ExpandoObject which uses
                //a dictionary in the background. The default ContractResolver does not correctly work with
                //a dictionary.
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                })
                // Use Fluent Validation for validation instead of the built-in validation. I like to 
                //see validation in one place, rather than in attributes and in code. 
                .AddFluentValidation(x => x.RegisterValidatorsFromAssemblyContaining<Startup>())
                ;
            ;
            
            services.AddMediatR(typeof(GetThermostatHandler)); // Add MediatR

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Web Api Pattern Core", Version = "v1" });
            });

            Ioc.Register(services,Configuration);            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddSerilog();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Exception handling pipeline
                // Unable to get the body at this point as ASP.NET have already parse the body (it can only be read once)
                //unless you set EnableRewind(). But you can still only read the body before the controller reads it.
                app.UseExceptionHandler(builder =>
                {
                    builder.Run(async context =>
                    {
                        // On an unhandled exception, send a 500 response with a generic message 
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again later");
                    });
                });
            }

            app.UseMvc(); // Use MVC            

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
        }
    }
}
