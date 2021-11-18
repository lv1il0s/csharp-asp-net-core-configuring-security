using ConferenceTracker.Data;
using ConferenceTracker.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace ConferenceTracker
{
    public class Startup
    {
        private readonly string _allowedOrigins = "_allowedOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public string SecretMessage { get; set; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            SecretMessage = Configuration["SecretMessage"];
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("ConferenceTracker"));
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddTransient<IPresentationRepository, PresentationRepository>();
            services.AddTransient<ISpeakerRepository, SpeakerRepository>();

            services.AddCors(options =>
            {
                options.AddPolicy(_allowedOrigins, builder =>
                {
                    builder.WithOrigins("http://pluralsight.com");
                });
            });
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {   
            if (env.IsDevelopment())
            {
                logger.LogInformation("Environment is in development");
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            using (var context = scope.ServiceProvider.GetService<ApplicationDbContext>())
                context.Database.EnsureCreated();

            app.UseCors(_allowedOrigins);

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}


/*
 * 
 * 
Redirect HTTP Request to Use HTTPS Instead

In Startup class's Configure method, call the UseHttpsRedirection method on app. (This should be done after our database is created, but before UseStaticFiles is called.)

2
Set Up the Web Application to Use the HSTS Header

In Startup class's Configure method, as the very first thing we do in that method, call the IsDevelopment method on env, and check if it returns true or false.

If false, call UseHsts on app.
3
Setup Separate Error Pages for Developers and Users

In Startup class's Configure method, if IsDevelopment returns:

false call the UseExceptionHandler method on app with an argument of "/Home/Error" before the call to UseHsts.
true call the UseDeveloperExceptionPage method on app and the UseDatabaseErrorPage method on app.
4
Add ReadOnly Field For CORS to Startup

In our Startup class, create a new private readonly field of type string named _allowedOrigins, and set it to the value "_allowedOrigins".

5
Add CORS Support to ConfigureServices

In our Startup class's ConfigureServices method, add a call to the method AddCors on services and provide it an argument of options => { options.AddPolicy(_allowedOrigins, builder => { builder.WithOrigins("http://pluralsight.com"); }); }. (This is specifying the name of our CORS policy, and providing what domains will be permitted)

6
Add CORS Support to Configure

In our Startup class's Configure method, add a call to UseCors on app with _allowedOrigins as the arguement. Do this before our call to UseHttpsRedirection.

7
Add Logging to Startup

Update our Startup class's Configure method, to log if the application is running in development.

Add a using directive for Microsoft.Extensions.Logging.
Update the Configure method's signature to take a third parameter of type ILogger<Startup> with a name of logger.
In our existing condition that checks if IsDevelopment is true, when true call LogInformation on logger with an argument of "Environment is in development" before our exception handling.
8
Add ILogger Field to PresentationsController

In our PresentationsController class, add a private readonly field of type ILogger named _logger.

9
Update PresentationsController's Constructor

Update our PresentationsController update the constructor to take a third parameter of type ILogger<PresentationsController> named logger and set _logger to logger.

10
Add Logging to Edit Action

Update our PresentationsController's Edit method (the HTTP Get not the HTTP Post Edit method), add the following logging.

As the very first line call LogInformation on _logger with a message "Getting presentation id:" + id +  " for edit.".
Inside the condition where we check if id is null, before we return NotFound(), call LogError on _logger with a message "Presentation id was null.".
Inside the condition where we check if presentation is null, before we return NotFound(), call LogWarning on _logger with a message "Presentation id," + id + ", was not found.".
Immediately before we set our ViewData, call LogInformation on _logger with a message "Presentation id," + id + ", was found. Returning 'Edit view'.
11
Add a call to retrieve `SecretMessage` from `Configuration`

Inside our Startup class's ConfigureServices method, set our SecretMessage property using to the returned value from Configuration["SecretMessage"]. Normally, you'd use this to contain things like connection strings. However, since we're using an InMemory database, this is simply being used as an example, and serves no functional purpose.

12
Create a "Password" Secret

Create a "Password" secret Note: because they're a secret only stored on your local computer, we can't actually check to see if you did it right!

We're going to use the .NET Core CLI (Command Line Interface).
In the CLI navigate to the ConferenceTracker directory, not the solution's directory. (You can use the cd command to navigate between directories. Example: cd ConferenceTracker)
Enter the command dotnet user-secrets init this sets the secretsId for your project
Enter the command dotnet user-secrets set "SecretMessage" "Keep it secret, Keep it safe." this sets a secret with the key "SecretMessage" with a value "Keep it secret, Keep it safe."
13
Add Cookie Policy to ConfigureServices

Inside Startup class's ConfigureServices method, anywhere before our call to AddControllersWithViews, call Configure<CookiePolicyOptions> on services with the argument options => { options.CheckConsentNeeded = context => true; options.MinimumSameSitePolicy = SameSiteMode.None; }. (Don't worry about specifically what these arguments mean. You'll need to change them based on what cookies you use and what they store)

14
Add Cookie Policy to Configure

Inside Startup class's Configure method, anywhere before our call to UseRouting, call UseCookiePolicy on app.
*/