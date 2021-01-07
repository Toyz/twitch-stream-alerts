using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Website.Models;

namespace Website
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
            services.AddRazorPages();

            services.AddControllers();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddMvc(options => options.EnableEndpointRouting = false)
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddDefaultIdentity<User>(options => { options.SignIn.RequireConfirmedAccount = false; })
              .AddRoles<IdentityRole>()
              .AddEntityFrameworkStores<DBContext>();

            services.AddDbContext<DBContext>(options =>
               options.UseNpgsql(
                   Configuration.GetConnectionString("ConnectionString"), o =>
                   {
                       o.UseTrigrams();
                   }).EnableSensitiveDataLogging());

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie(options =>
            {
                options.Cookie.Name = "NP.Auth";
                options.Cookie.IsEssential = true;
                options.LoginPath = "/Auth/Login";
                options.LogoutPath = "/Auth/Logout";
            })
           .AddTwitch(options =>
           {
               options.ClientId = Configuration.GetValue("Twitch:ClientID", "");
               options.ClientSecret = Configuration.GetValue("Twitch:ClientSecret", "");

               options.SaveTokens = true;
               options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;

               options.Events = new OAuthEvents
               {
                   OnCreatingTicket = ctx =>
                   {
                       ctx.Identity.AddClaim(new Claim("urn:twitch:access_token", ctx.AccessToken));
                       ctx.Identity.AddClaim(new Claim("urn:twitch:refresh_token", ctx.RefreshToken));
                       ctx.Identity.AddClaim(new Claim("urn:twitch:expires",
                           DateTime.UtcNow.Add(ctx.ExpiresIn.Value).ToString()));
                       return Task.CompletedTask;
                   },

                   OnTicketReceived = ctx =>
                   {
                       var username = ctx.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
                       if (string.IsNullOrWhiteSpace(username))
                       {
                           ctx.HandleResponse();
                           ctx.Response.Redirect("/");
                           return Task.CompletedTask;
                       }

                       return Task.CompletedTask;
                   }
               };
           });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            InitializeDatabase(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }

        private static void InitializeDatabase(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();

            var ctx = scope.ServiceProvider.GetRequiredService<DBContext>();
            ctx.Database.Migrate();
        }
    }
}
