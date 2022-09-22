using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MusicStore.Data;
using MusicStore.DataAccess.IMainRepository;
using MusicStore.DataAccess.MainRepository;
using MusicStore.Utility;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicStore
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDatabaseDeveloperPageExceptionFilter();

            //AddDefaultTokenProviders=> token i�in tan�mland�
            services.AddIdentity<IdentityUser,IdentityRole>()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            //unitOfWork tan�mlad�k.AddScoped sadece tek bir defa olu�turmaya yarar
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            //email ayar� i�in tan�mland�
            services.AddSingleton<IEmailSender, EmailSender>();

            //senggrid ayarlar�
            services.Configure<EmailOptions>(Configuration);

            //stripe �deme sistemi ayar�
            services.Configure<StripeSettings>(Configuration.GetSection("Stripe"));

            //arka tarafta yap�lan de�i�iklikleri web taraf�na an�nda yans�d�r
            services.AddControllersWithViews().AddRazorRuntimeCompilation();

            services.AddMvc();

            //yetkisi olmayan veya bulunamayan sayfalar i�in y�nlendirilecek sayfalar�n yolu tan�mland�
            
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
            });
            
            //facebook ile giri� servisinin a��lmas�
            services.AddAuthentication().AddFacebook(options =>
            {
                //facebook api id
                options.AppId = "637173471447148";
                //facebook api key
                options.AppSecret = "16ddadac11ad05d7caea3607e4990c94";
            });

            //google ile giri� ayar�
            services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = "15748677037-kbrmtftncrmvkn720jkrqfgab0pnl9h2.apps.googleusercontent.com";
                options.ClientSecret = "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";
            });

            //session'lar�n �al��mas� i�in tan�mland�
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); //30 dakika aktif kal�n�r
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //Stripe �deme ayar�
            StripeConfiguration.ApiKey = Configuration.GetSection("Stripe")["StripeKey"];

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession(); //session'un kullan�labilmesi i�in tan�mland�    
            //yetkilendirme i�lemleri i�in tan�mland�(Microsoftun Identity �zelli�inin kullan�labilmesi i�in)
            
            app.UseAuthentication();
            app.UseAuthorization();

            //yetkilendirme i�lemleri i�in tan�mland� bitti

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{area:exists}/{controller=Default}/{action=Index}/{id?}");
                    //pattern: "{area:Customer}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
