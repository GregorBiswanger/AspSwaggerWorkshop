using AspRestApiWorkshop.Authentication;
using AspRestApiWorkshop.OperationFilters;
using CoreCodeCamp.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]
namespace AspRestApiWorkshop
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
            services.AddDbContext<CampContext>();
            services.AddScoped<ICampRepository, CampRepository>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddControllers(configuration => 
            {
                var jsonFormatter = configuration.OutputFormatters.OfType<SystemTextJsonOutputFormatter>().FirstOrDefault();
                jsonFormatter.SupportedMediaTypes.Add("application/vnd.marvin.hateoas+json");

            }).AddXmlDataContractSerializerFormatters();

            services.AddAuthentication("Basic")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", null);

            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VV";
                options.SubstituteApiVersionInUrl = true;
            });

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

            services.AddSwaggerGen(setupAction =>
            {
                var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

                foreach (var description in provider.ApiVersionDescriptions)
                {
                    setupAction.SwaggerDoc(description.GroupName, new OpenApiInfo
                    {
                        Title = "Camps API",
                        Version = description.ApiVersion.ToString(),
                        Description = "Das ist unsere super coole API vom Workshop",
                        Contact = new OpenApiContact
                        {
                            Name = "Gregor Biswanger",
                            Email = "gregor.biswanger@web-enliven.de",
                            Url = new Uri("https://twitter.com/BFreakout")
                        },
                        License = new OpenApiLicense
                        {
                            Name = "MIT License",
                            Url = new Uri("https://opensource.org/licenses/MIT")
                        }
                    });
                }

                setupAction.AddSecurityDefinition("basicAuth", new OpenApiSecurityScheme()
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "basic",
                    Description = "Input your username and password to access this API"
                });

                setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "basicAuth" }
                        }, new List<string>() }
                });

                setupAction.OperationFilter<GetCampOperationFilter>();
                setupAction.OperationFilter<CreateCampOperationFilter>();

                var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlCommentsPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);
                setupAction.IncludeXmlComments(xmlCommentsPath);
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();

            app.UseSwaggerUI(setupAction => 
            {
                setupAction.InjectStylesheet("/assets/custom-ui.css");
                //setupAction.IndexStream = ()
                //        => GetType().Assembly
                //        .GetManifestResourceStream("AspRestApiWorkshop.EmbeddedAssets.index.html");

                foreach (var description in provider.ApiVersionDescriptions)
                {
                    setupAction.SwaggerEndpoint($"../swagger/{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant());
                }

                setupAction.RoutePrefix = "";

                setupAction.DefaultModelExpandDepth(2);
                setupAction.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
                setupAction.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                setupAction.EnableDeepLinking();
                setupAction.DisplayOperationId();
            });

            app.UseStaticFiles();

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














