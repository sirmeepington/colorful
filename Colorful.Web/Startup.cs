using Colorful.Web.Models.Webhook;
using Colorful.Web.Services;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Colorful.Web
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

            services.AddMassTransit(opt =>
            {
                opt.UsingRabbitMq((context, config) => InitRabbit(context, config));
            });

            services.AddMassTransitHostedService();

            services.AddAuthorization();

            services.AddSingleton<IDiscordService,DiscordService>();

            services.AddSingleton<Webhook>(x => 
                    new Webhook(
                        ulong.Parse(Environment.GetEnvironmentVariable("WEBHOOK_ID")),
                        Environment.GetEnvironmentVariable("WEBHOOK_TOKEN")
                        ));

            services.AddScoped<IUpdaterService,UpdaterService>();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
                .AddCookie(cookieOpt =>
                {
                    cookieOpt.ExpireTimeSpan = TimeSpan.FromDays(7);
                    cookieOpt.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                    cookieOpt.Cookie.Name = "Colorful_Discord_Token";
                    cookieOpt.LoginPath = "/signin";
                    cookieOpt.LogoutPath = "/signout";
                })
                .AddDiscord(opt =>
                {
                    opt.ClientId = Environment.GetEnvironmentVariable("DISCORD_APP_CLIENT_ID");
                    opt.ClientSecret = Environment.GetEnvironmentVariable("DISCORD_APP_CLIENT_SECRET");
                    opt.Scope.Add("guilds");
                    opt.CorrelationCookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                    opt.AccessDeniedPath = "/";
                });

            services.AddCors();
        }

        private void InitRabbit(IBusRegistrationContext context, IRabbitMqBusFactoryConfigurator config)
        {
            config.Host(Environment.GetEnvironmentVariable("RABBIT_HOST"), "/", settings =>
            {
                settings.Username(Environment.GetEnvironmentVariable("RABBIT_USER"));
                settings.Password(Environment.GetEnvironmentVariable("RABBIT_PASS"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors();

            app.UseAuthentication();
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
