using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UdemyRealWorldUnitTest.Web.Models;
using UdemyRealWorldUnitTest.Web.Repository;

namespace UdemyRealWorldUnitTest.Web
{

    //Database First : Model ve DbContext Oluþturma Kod Örneði 

    /*Data Source=DESKTOP-SM0VBLO;Initial Catalog=UdemyUnitTest;Integrated Security=True;
    Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;
    ApplicationIntent=ReadWrite;MultiSubnetFailover=False */

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
            //IRepo görürse Repo'dan nesne örneði alacaktýr 
            //AddScoped : Birden fazla requestte ilk nesneyi instance'i kullanýr. Performanslýdýr
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));


            services.AddDbContext<UdemyUnitTestContext>(opts =>
            {
                //Configuration'a appsettingsjson ' daki key deðerini veriyoruz.
                opts.UseSqlServer(Configuration["SqlConString"]);
            });



            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
