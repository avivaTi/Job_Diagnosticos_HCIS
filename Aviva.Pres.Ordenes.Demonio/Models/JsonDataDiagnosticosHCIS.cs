using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aviva.Pres.Ordenes.Demonio.Models
{
    public class JsonDataDiagnosticosHCIS
    {
        public int ENCUENTRO { get; set; }
        public string INICIOENCUENTRO { get; set; }
        public string FINENCUENTRO { get; set; }
        public string TIPODIAGNOSTICO { get; set; }
        public string CODDIAGNOSTICO { get; set; }
        public string DESCDIAGNOSTICO { get; set; }
        public int SEDEID { get; set; }

    }
}
