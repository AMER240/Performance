using System;
using System.Net.Http;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Performance.Infrastructure.Application;
using Performance.Application.Services;

namespace Performance
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            System.Windows.Forms.Application.SetHighDpiMode(System.Windows.Forms.HighDpiMode.SystemAware);
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            var builder = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // Try to load configuration from the app context base directory to ensure appsettings.json is found
                    var fileConfig = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                        .SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .Build();

                    var conn = fileConfig.GetConnectionString("DefaultConnection") ?? context.Configuration.GetConnectionString("DefaultConnection");
                    if (string.IsNullOrWhiteSpace(conn))
                    {
                        // optional environment variable fallback
                        conn = Environment.GetEnvironmentVariable("PERF_CONNECTION") ?? string.Empty;
                    }

                    if (string.IsNullOrWhiteSpace(conn))
                    {
                        throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured. Set it in appsettings.json or environment variable PERF_CONNECTION.");
                    }

                    services.AddDbContext<PerformanceDbContext>(options => options.UseSqlServer(conn));
                    services.AddScoped<MainForm>();
                    
                    // Register HttpClient for Gemini API
                    services.AddSingleton<HttpClient>();
                    
                    // AI Services - Gemini-powered
                    services.AddScoped<ITaskSuggestionService, Performance.Application.Services.GeminiTaskSuggestionService>();
                    services.AddScoped<IProjectSuggestionService, Performance.Application.Services.GeminiProjectSuggestionService>();
                    
                    // Other services
                    services.AddScoped<IUserService, UserService>();
                    services.AddScoped<Performance.Application.Services.IProjectService, Performance.Application.Services.ProjectService>();
                    services.AddScoped<Performance.Application.Services.ITaskService, Performance.Application.Services.TaskService>();
                    services.AddScoped<LoginForm>();
                    services.AddScoped<ProjectListForm>();
                });

            var host = builder.Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var db = services.GetRequiredService<PerformanceDbContext>();
                db.Database.Migrate();

                // Manual migration: Add Sector column if it doesn't exist
                try
                {
                    db.Database.ExecuteSqlRaw(@"
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Users') AND name = 'Sector')
                        BEGIN
                            ALTER TABLE Users ADD Sector NVARCHAR(MAX) NULL
                        END
                    ");
                }
                catch
                {
                    // Column already exists or error, ignore
                }

                // seed users
                var userService = services.GetRequiredService<Performance.Application.Services.IUserService>();
                userService.EnsureSeedUsersAsync().GetAwaiter().GetResult();

                // show login
                var login = services.GetRequiredService<LoginForm>();
                var loginResult = login.ShowDialog();
                if (loginResult != System.Windows.Forms.DialogResult.OK || login.AuthenticatedUser == null)
                {
                    return;
                }

                var mainForm = services.GetRequiredService<MainForm>();
                mainForm.SetAuthenticatedUser(login.AuthenticatedUser);
                System.Windows.Forms.Application.Run(mainForm);
            }
        }
    }
}
