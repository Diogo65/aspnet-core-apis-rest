using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alura.ListaLeitura.Api.Formatters;
using Alura.ListaLeitura.Modelos;
using Alura.ListaLeitura.Persistencia;
using Alura.WebAPI.Api.Filtros;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;

namespace Alura.WebAPI.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration config)
        {
            Configuration = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<LeituraContext>(options => {
                options.UseSqlServer(Configuration.GetConnectionString("ListaLeitura"));
            });

            services.AddTransient<IRepository<Livro>, RepositorioBaseEF<Livro>>();

            services.AddMvc(options => {
                options.OutputFormatters.Add(new LivroCsvFormatter());
                options.Filters.Add(typeof(ErrorResponseFilter)); //Filtro para tratar erros de forma generalizada
            }).AddXmlSerializerFormatters();

            //Desabilitar o comportamento automático, de filtro do ModelState que retorna um BadRequest antes de bater na api
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            //Adicionar uma Autentição
            //Isso é feito quando o token chega da requisição
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "JwtBearer"; //Bearer portador do Token
                options.DefaultChallengeScheme = "JwtBearer";
                //Adicionar o scheme
            }).AddJwtBearer("JwtBearer", options => {
                //Configurar como ele irá validar
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true, //validar o emissor
                    ValidateAudience = true, //validar quem pediu
                    ValidateLifetime = true, //validar e expiração
                    ValidateIssuerSigningKey = true,
                    //Chave de assinatura que será utilizada pelo Issuer para validar
                    //utiilizando a senha com chave(que ira gerar mais de 256 bits)
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("alura-webapi-authentication-valid")),
                    ClockSkew = TimeSpan.FromMinutes(5), //duração do token
                    ValidIssuer = "Alura.WebApp", //Arbitrario
                    ValidAudience = "Postman", //Nome arbitrario
                };
            });

            //Adicionar o middleware de versionamento atraves do adds
            //
            //Ler do parâmetro da QueryString e do cabeçalho
            //services.AddApiVersioning(options => {
            //    options.ApiVersionReader = ApiVersionReader.Combine(
            //            new QueryStringApiVersionReader("api-version"),
            //            new HeaderApiVersionReader("api-version")
            //        );
            //});

            //versionamento por Url
            services.AddApiVersioning();

            //adicionar o Swagger
            //instalar Swashbuckle.aspntCore
            //https://github.com/swagger-api/swagger-ui
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Description = "Documentação  da API", Version = "1.0" });
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(); //add ao pipeline

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            });

            app.UseMvc();
        }
    }
}
