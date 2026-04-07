using BigBrother.Application.Interfaces;
using BigBrother.Infrustructure.Persistance;
using BigBrother.Infrustructure.Repositories;
using BigBrother.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using System.Windows;
using BigBrother.Presentation.ViewModels;

namespace BigBrotherApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private IServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Settings DI
            var services = new ServiceCollection();

            var configuration = BuildConfiguration();
            services.AddSingleton(configuration);

            // Setting of logging
            ConfigureLogging(services);

            // Configure DB
            ConfigureDatabase(services, configuration);

            //  Setting repositories
            RegisterRepositories(services);

            // Setting services
            RegisterServices(services);

            // Registration ViewModels и windows
            RegisterUIComponents(services);

            // Building of provider
            _serviceProvider = services.BuildServiceProvider();

            // Inicialization of db
            InitializeDatabase();

            // Starting of main window
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
            mainWindow.Show();
        }

        private IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        private void ConfigureLogging(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.AddDebug();        
                builder.AddConsole();     
                builder.SetMinimumLevel(LogLevel.Debug); 
            });
        }

        // Setting db 
        private void ConfigureDatabase(IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("DefaultConnection");
            
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(connectionString));
        }

        // Registration of repositories to DI
        private void RegisterRepositories(IServiceCollection services)
        {
            services.AddScoped<IActivitySessionRepository, ActivitySessionRepository>();
        }

        // Registration of services to DI
        private void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<ITrackerService, TrackerService>();
        }

        // Regsitartion of UI
        private void RegisterUIComponents(IServiceCollection services)
        {
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainWindow>();
        }

        // Initialization of db with migrations
        private void InitializeDatabase()
        {
            try { 
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.Migrate();
            } catch (Exception ex) {
                var logger = _serviceProvider.GetService<ILogger<App>>();
                logger?.LogError(ex, "Error while initializing db");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                if (_serviceProvider == null) return;
                using var scope = _serviceProvider.CreateScope();
                var trackerService = scope.ServiceProvider.GetRequiredService<ITrackerService>();
                _ = trackerService.StopTrackingAsync().ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        var logger = _serviceProvider?.GetService<ILogger<App>>();
                        logger?.LogError(t.Exception, "Error stopping tracker");
                    }
                }, TaskScheduler.Default);
            }
            catch (Exception ex)
            {
                var logger = _serviceProvider?.GetService<ILogger<App>>();
                logger?.LogError(ex, "Error while stopping db");
            }
            finally
            {
                base.OnExit(e);
            }
        }
    }

}
