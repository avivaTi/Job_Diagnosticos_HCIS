using Aviva.Pres.Ordenes.Demonio.Data;
using Aviva.Pres.Ordenes.Demonio.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aviva.Pres.Ordenes.Demonio
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        public override Task StartAsync(CancellationToken cancellation)
        {
            _logger.LogInformation("Arrancando Demonio Diagnosticos HCIS");
            return base.StartAsync(cancellation);
        }
        public override Task StopAsync(CancellationToken cancellation)
        {
            _logger.LogInformation("Parando Demonio Diagnosticos HCIS");
            return base.StopAsync(cancellation);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                _logger.LogInformation("El demonio sigue ejecutandose correctamente");
                await getDiagnosticosHCIS();
                await Task.Delay(60000, stoppingToken);
            }
        }

        private async Task getDiagnosticosHCIS()

        {
            DataOrdenesColonial data = new DataOrdenesColonial(_configuration, _logger);
            //  List<usp_List_For_SalesForce> lista_For_SalesForce = new List<usp_List_For_SalesForce>();
            List<JsonDataDiagnosticosHCIS> var1 = new List<JsonDataDiagnosticosHCIS>();
            var1 = data.ObtenerDatosDiagnosticosHCIS();

        }
    }
}
