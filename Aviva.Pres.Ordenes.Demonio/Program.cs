using Aviva.Pres.Ordenes.Demonio.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aviva.Pres.Ordenes.Demonio
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DateTime currentDay = DateTime.Now.Date;
            Log.Logger = new LoggerConfiguration()
                //.MinimumLevel.ControlledBy(levelSwitch)
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.File(@"D:\log\Diagnosticos_hcis\diagnosticos_hcis_.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("Iniciando Demonio de Diagnosticos HCIS");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {

                Log.Fatal($"Error al iniciar Demonio de Diagnosticos HCIS - {ex.Message}");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddScoped<ILoggerService, LoggerService>();
                }).UseSerilog()
                .UseWindowsService();
    }
}
