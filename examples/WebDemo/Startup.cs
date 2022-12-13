using LineNotifyHelper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebDemo.Models;
using WebDemo.Repository;

namespace WebDemo
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
            services.AddDbContext<UserDBContext>(options => options.UseInMemoryDatabase(databaseName: "Users"));

            services.AddScoped<UserRepository, UserRepository>();

            services.AddDistributedMemoryCache();
            services.AddSession(s => 
            {
                /// chrome won't send session cookie with SameSite=Strict at post callback from line auth
                s.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
                s.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                s.IdleTimeout = TimeSpan.FromMinutes(30);
            });

            services.AddHttpContextAccessor();

            /// LineNotifySender need HttpClientFactory
            services.AddHttpClient();
            services.AddOptions<LineNotifyOptions>().Bind(Configuration.GetSection("LineNotifyOptions"));
            services.AddScoped<ILineNotifySender, LineNotifySender>();

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

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
