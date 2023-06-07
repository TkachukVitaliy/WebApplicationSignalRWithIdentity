using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplicationWithIdentity.DAL;
using WebApplicationWithIdentity.Models;

namespace WebApplicationWithIdentity
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
            services.Configure<IdentityOptions>(options =>
            {
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZабвгґдеєжзиіїйклмнопрстуфхцчшщьюяАБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЮЯ0123456789-._@+";
            });
            services.AddSession();
            services.AddDbContext<ApplicationContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("UserDbConnection")));

            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 5;   // минимальная длина
                options.Password.RequireNonAlphanumeric = false;   // требуются ли не алфавитно-цифровые символы
                options.Password.RequireLowercase = false; // требуются ли символы в нижнем регистре
                options.Password.RequireUppercase = false; // требуются ли символы в верхнем регистре
                options.Password.RequireDigit = false; // требуются ли цифры
            })
                .AddEntityFrameworkStores<ApplicationContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<UserManager<User>>();
            services.AddScoped<SignInManager<User>>();
            services.AddSingleton<IUserIdProvider, UserIdProvider>();

            services.AddDbContext<ChatContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("ChatDbConnection")));

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })
                .AddCookie()
                    //options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");) 
                .AddGoogle(options =>
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.ClientId = Configuration.GetValue<string>("OpenIdConnect:ClientId");
                    options.ClientSecret = Configuration.GetValue<string>("OpenIdConnect:ClientSecret");
                    options.CallbackPath = "/signin-google";
                    options.SaveTokens = true;

                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = (async context =>
                        {
                            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                            var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationContext>();
                            var signInManager = context.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();

                            var email = context.Principal.FindFirstValue(ClaimTypes.Email);
                            var name = context.Principal.FindFirstValue(ClaimTypes.Name);
                            var userName = name.Replace(" ", "");

                            var user = await userManager.FindByEmailAsync(email);
                            if (user == null)
                            {
                                user = new User { Email = email, UserName = userName };
                                await userManager.CreateAsync(user);
                                await dbContext.SaveChangesAsync();
                            }
                            await signInManager.SignInAsync(user, false);
                            await Task.CompletedTask;
                        }),

                        OnRemoteFailure = (context =>
                        {
                            //TODO: on auth failure code
                            return Task.CompletedTask;
                        }),
                    };

                });
            services.AddAuthorization();

            services.AddSignalR(options =>
            {
                options.MaximumReceiveMessageSize = 102400000;
                options.EnableDetailedErrors = true;
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(120);
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
            app.UseSession();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Login}/{id?}");
                endpoints.MapHub<ChatHub>("/Chat");
            });

            
        }
    }
}
