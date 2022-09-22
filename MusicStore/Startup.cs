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

            //AddDefaultTokenProviders=> token için tanýmlandý
            services.AddIdentity<IdentityUser,IdentityRole>()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            //unitOfWork tanýmladýk.AddScoped sadece tek bir defa oluþturmaya yarar
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            //email ayarý için tanýmlandý
            services.AddSingleton<IEmailSender, EmailSender>();

            //senggrid ayarlarý
            services.Configure<EmailOptions>(Configuration);

            //stripe ödeme sistemi ayarý
            services.Configure<StripeSettings>(Configuration.GetSection("Stripe"));

            //arka tarafta yapýlan deðiþiklikleri web tarafýna anýnda yansýdýr
            services.AddControllersWithViews().AddRazorRuntimeCompilation();

            services.AddMvc();

            //yetkisi olmayan veya bulunamayan sayfalar için yönlendirilecek sayfalarýn yolu tanýmlandý
            
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
            });
            
            //facebook ile giriþ servisinin açýlmasý
            services.AddAuthentication().AddFacebook(options =>
            {
                //facebook api id
                options.AppId = "637173471447148";
                //facebook api key
                options.AppSecret = "16ddadac11ad05d7caea3607e4990c94";
            });

            //google ile giriþ ayarý
            services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = "15748677037-kbrmtftncrmvkn720jkrqfgab0pnl9h2.apps.googleusercontent.com";
                options.ClientSecret = "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";
            });

            //session'larýn çalýþmasý için tanýmlandý
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); //30 dakika aktif kalýnýr
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

            //Stripe ödeme ayarý
            StripeConfiguration.ApiKey = Configuration.GetSection("Stripe")["StripeKey"];

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession(); //session'un kullanýlabilmesi için tanýmlandý    
            //yetkilendirme iþlemleri için tanýmlandý(Microsoftun Identity özelliðinin kullanýlabilmesi için)
            
            app.UseAuthentication();
            app.UseAuthorization();

            //yetkilendirme iþlemleri için tanýmlandý bitti

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
