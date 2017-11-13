using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using LibraryRESTfulApi.Entities;
using Microsoft.EntityFrameworkCore;
using LibraryRESTfulApi.Services;
using Microsoft.Extensions.Logging;
using LibraryRESTfulApi.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Diagnostics;
using NLog.Extensions.Logging;

namespace LibraryRESTfulApi
{
    public class Startup
    {
        public static IConfiguration Configuration { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(setupAction =>
            {
                setupAction.ReturnHttpNotAcceptable = true;
                setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                setupAction.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());
            });

            var connectionString = Startup.Configuration["connectionsStrings:libraryApiConnectionString"];

            services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));

            services.AddScoped<ILibraryRepository, LibraryRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, 
            LibraryContext libraryContext)
        {
            loggerFactory.AddConsole();

            loggerFactory.AddDebug(LogLevel.Information);

            loggerFactory.AddNLog();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();

                        if (exceptionHandlerFeature != null)
                        {
                            var logger = loggerFactory.CreateLogger("Global exception logger");

                            logger.LogError(500, exceptionHandlerFeature.Error, exceptionHandlerFeature.Error.Message);
                        }

                        context.Response.StatusCode = 500;

                        await context.Response.WriteAsync("An unexpected fault happened. Try again later");
                    });
                });
            }

            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Entities.Author, Models.AuthorDto>().
                ForMember(dest => dest.Name, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}")).
                ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.GetCurrentAge()));

                cfg.CreateMap<Entities.Book, Models.BookDto>();

                cfg.CreateMap<Models.AuthorForCreationDto, Entities.Author>();

                cfg.CreateMap<Models.BookForCreationDto, Entities.Book>();

                cfg.CreateMap<Models.BookForUpdateDto, Entities.Book>();

                cfg.CreateMap<Entities.Book, Models.BookForUpdateDto>();
            });

            libraryContext.EnsureSeedDataForContext();

            app.UseMvc();
        }
    }
}
