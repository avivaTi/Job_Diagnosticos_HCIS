using Aviva.Pres.Ordenes.Demonio.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aviva.Pres.Ordenes.Demonio.Data
{
    public class DataOrdenesColonial
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Worker> _logger;

        public DataOrdenesColonial(IConfiguration configuration, ILogger<Worker> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public List<JsonDataDiagnosticosHCIS> ObtenerDatosDiagnosticosHCIS()
        {
            List<JsonDataDiagnosticosHCIS> diagnosticos = new List<JsonDataDiagnosticosHCIS>();
            string cadenaConexionToInsert = _configuration.GetSection("ConnectionStrings").GetValue<string>("DefaultConnection");
            string cadenaConexion = _configuration.GetSection("ConnectionStrings").GetValue<string>("DefaultConnectionHCIS");

            _logger.LogInformation("Leyendo los diagnosticos de HCIS");

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                // 1️⃣ Consulta MV_PRESUPUESTOS
                string queryDiagnosticos = @"
                        SELECT DIAG.ENCUENTRO, DIAG.INICIOENCUENTRO, DIAG.FINENCUENTRO, 
                            DIAG.TIPODIAGNOSTICO,  DIAG.CODDIAGNOSTICO, DIAG.DESCDIAGNOSTICO, 
                            CASE (DIAG.CENTRO) WHEN 'COLONIAL' THEN 2 WHEN 'SMP San Martin de Porres' THEN 3 ELSE 1 END SEDEID
                              FROM [HCIS].[MV_DIAGENCUENTRO] DIAG
                              WHERE INICIOENCUENTRO >= CONVERT(VARCHAR(10), GETDATE(), 23)
                        ";

                using (SqlCommand cmdPres = new SqlCommand(queryDiagnosticos, conexion))
                {
                    cmdPres.CommandTimeout = 300; // 5 minutos

                    using (SqlDataReader dr = cmdPres.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            diagnosticos.Add(MapearDiagnosticos(dr));
                        }
                    }
                }

            }

            // 3️⃣ Inserción en base de datos destino
            using (SqlConnection conexionToInsert = new SqlConnection(cadenaConexionToInsert))
            {
                conexionToInsert.Open();

                foreach (var diagnostico in diagnosticos)
                {

                    int validaIns32 = ValidarEnTramas(
                        conexionToInsert,
                        "[dbo].[usp_TB_HCIS_DIAGENCUENTRO_Validate]",
                         diagnostico.ENCUENTRO,
                         diagnostico.CODDIAGNOSTICO,
                         diagnostico.SEDEID
                    );

                    if (validaIns32 == 0)
                    {
                        InsertarEnTramas(
                            conexionToInsert,
                            "dbo.usp_TB_HCIS_DIAGENCUENTRO_Ins",
                            diagnostico
                        );

                        _logger.LogInformation(
                            $"Inserto un registro en la tabla TB_HCIS_DIAGENCUENTRO : {diagnostico.ENCUENTRO} , {diagnostico.CODDIAGNOSTICO}, {diagnostico.DESCDIAGNOSTICO}, {diagnostico.SEDEID}"
                        );
                    }
                }
            }

            return diagnosticos;
        }
        // Mapeo reutilizable para evitar duplicación
        private JsonDataDiagnosticosHCIS MapearDiagnosticos(SqlDataReader dr)
        {
            return new JsonDataDiagnosticosHCIS
            {
                ENCUENTRO = dr["ENCUENTRO"] != DBNull.Value ? Convert.ToInt32(dr["ENCUENTRO"]) : 0,
                INICIOENCUENTRO = dr["INICIOENCUENTRO"]?.ToString() ?? string.Empty,
                FINENCUENTRO = dr["FINENCUENTRO"]?.ToString() ?? string.Empty,
                TIPODIAGNOSTICO = dr["TIPODIAGNOSTICO"]?.ToString() ?? string.Empty,
                CODDIAGNOSTICO = dr["CODDIAGNOSTICO"]?.ToString() ?? string.Empty,
                DESCDIAGNOSTICO = dr["DESCDIAGNOSTICO"]?.ToString() ?? string.Empty,
                SEDEID = dr["SEDEID"] != DBNull.Value ? Convert.ToInt32(dr["SEDEID"]) : 0,
            };
        }

        private int ValidarEnTramas(SqlConnection conexion, string procedimiento, int ENCUENTRO, string CODDIAGNOSTICO, int sedeId)
        {
            using (SqlCommand command = new SqlCommand(procedimiento, conexion))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ENCUENTRO", ENCUENTRO);
                command.Parameters.AddWithValue("@CODDIAGNOSTICO", CODDIAGNOSTICO);
                command.Parameters.AddWithValue("@sedeId", sedeId);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private void InsertarEnTramas(SqlConnection conexion, string procedimiento, JsonDataDiagnosticosHCIS diagnostico)
        {
            using (SqlCommand cmd = new SqlCommand(procedimiento, conexion))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@ENCUENTRO", SqlDbType.Int).Value = diagnostico.ENCUENTRO;
                cmd.Parameters.Add("@CODIGODIAGNOSTICO", SqlDbType.VarChar, 10).Value = diagnostico.CODDIAGNOSTICO;
                cmd.Parameters.Add("@DESCDIAGNOSTICO", SqlDbType.VarChar, 500).Value = diagnostico.DESCDIAGNOSTICO;
                cmd.Parameters.Add("@CENTRO", SqlDbType.Int).Value = diagnostico.SEDEID;

                cmd.ExecuteNonQuery();
            }
        }

    }
}
