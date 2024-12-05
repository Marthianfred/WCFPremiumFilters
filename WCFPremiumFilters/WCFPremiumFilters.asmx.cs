using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Web.Script.Services;
using System.Web.Services;
using Newtonsoft.Json;
using WCFPremiumFilters.Comparers;

namespace WCFPremiumFilters
{
    [WebService(Namespace = "https://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [ScriptService]
    public class WCFPremiumFilters : WebService
    {
        private readonly DatabaseHelper _dbHelper = new DatabaseHelper();

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetYears()
        {
            try
            {
                var storedProcedureName = "App_Apl_03Year";
                var dt = _dbHelper.ExecuteStoredProcedure(storedProcedureName);
                return JsonConvert.SerializeObject(dt, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetTipoAplicacion(string sIdioma)
        {
            try
            {
                var storedProcedureName = string.IsNullOrEmpty(sIdioma) || sIdioma == "es"
                    ? "App_Apl_01Type"
                    : "App_Apl_01TypeEN";
                var dt = _dbHelper.ExecuteStoredProcedure(storedProcedureName);
                return JsonConvert.SerializeObject(dt, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetMarcaVehiculo(string sIdioma, string Id_TMk)
        {
            try
            {
                var storedProcedureName = (string.IsNullOrEmpty(sIdioma) || sIdioma == "es")
                    ? "App_Apl_02Manufacturer"
                    : "App_Apl_02ManufacturerEN";
                var nIdTMk = Convert.ToInt32(Id_TMk);
                var dt = _dbHelper.ExecuteStoredProcedure(storedProcedureName);
                return FilterAndSerialize(dt, row => row.Field<int>("Id_TMk") == nIdTMk);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetModelo(string sIdioma, int Id_Mk, int anio, int tipoAplicacion)
        {
            try
            {
                var storedProcedureName = (string.IsNullOrEmpty(sIdioma) || sIdioma == "es")
                    ? "App_Apl_04Model"
                    : "App_Apl_04ModelEN";

                var parameters = new OleDbParameter[]
                {
                    new OleDbParameter("@Id_Mk", Id_Mk),
                    new OleDbParameter("@Id_TMk", tipoAplicacion)
                };

                var dt = _dbHelper.ExecuteStoredProcedure(storedProcedureName, parameters);


                IEnumerable<DataRow> results = from myRow in dt.AsEnumerable()
                    where myRow.Field<int>("Id_Mk") == Id_Mk
                          && myRow.Field<int>("Id_TMk") == tipoAplicacion
                          && myRow.Field<double>("AñoIni") <= anio
                          && anio <= myRow.Field<double>("AñoFin")
                    select myRow;


                if (!results.Any()) return "No results found";
                results = results.Distinct(new ModeloComparer());
                var dt2 = results.CopyToDataTable<DataRow>();
                return JsonConvert.SerializeObject(dt2, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetCilindraje(int Id_RefMk, int Id_TMk, string sIdioma, int anio)
        {
            try
            {
                string storedProcedureName = (string.IsNullOrEmpty(sIdioma) || sIdioma == "es")
                    ? (Id_TMk == 1 || Id_TMk == 4 ? "App_Apl_05Motor" : "App_Apl_05MotorEN")
                    : (Id_TMk == 1 || Id_TMk == 4 ? "App_Apl_05Motor" : "App_Apl_05MotorEN");

                var dt = _dbHelper.ExecuteStoredProcedure(storedProcedureName);
                var filteredRows = dt.AsEnumerable().Where(row =>
                    row.Field<int>("Id_RefMk") == Id_RefMk &&
                    row.Field<double>("AñoIni") <= anio &&
                    anio <= row.Field<double>("AñoFin"));

                if (filteredRows.Any())
                {
                    var filteredTable = filteredRows.CopyToDataTable();
                    return JsonConvert.SerializeObject(filteredTable, Formatting.Indented);
                }

                return "No results found";
            }
            catch (Exception ex)
            {
                return "Error";
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetFAire(int Id_RefMk, double McilL, string MotCap, string sIdioma)
        {
            try
            {
                string storedProcedureName =
                    string.IsNullOrEmpty(sIdioma) || sIdioma == "es" ? "App_R01Air" : "App_R01AirEN";
                var dt = _dbHelper.ExecuteStoredProcedure(storedProcedureName);
                return FilterAndSerialize(
                    dt,
                    row => row.Field<int>("Id_RefMk") == Id_RefMk &&
                           (McilL != 0
                               ? row.Field<double>("McilL") == McilL
                               : row.Field<string>("MotCap") == MotCap));
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetFOil(int Id_RefMk, double McilL, string MotCap, string sIdioma)
        {
            try
            {
                string storedProcedureName =
                    string.IsNullOrEmpty(sIdioma) || sIdioma == "es" ? "App_R02Oil" : "App_R02OilEN";
                var dt = _dbHelper.ExecuteStoredProcedure(storedProcedureName);
                return FilterAndSerialize(dt,
                    row => row.Field<int>("Id_RefMk") == Id_RefMk &&
                           (McilL != 0
                               ? row.Field<double>("McilL") == McilL
                               : row.Field<string>("MotCap") == MotCap));
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetFFuel(int Id_RefMk, double McilL, string MotCap, string sIdioma)
        {
            try
            {
                string storedProcedureName =
                    string.IsNullOrEmpty(sIdioma) || sIdioma == "es" ? "App_R03Fuel" : "App_R03FuelEN";
                var dt = _dbHelper.ExecuteStoredProcedure(storedProcedureName);
                return FilterAndSerialize(dt, row => row.Field<int>("Id_RefMk") == Id_RefMk &&
                                                     (McilL != 0
                                                         ? row.Field<double>("McilL") == McilL
                                                         : row.Field<string>("MotCap") == MotCap));
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetFAirAC(int Id_RefMk, double McilL, string MotCap, string sIdioma)
        {
            try
            {
                string storedProcedureName =
                    string.IsNullOrEmpty(sIdioma) || sIdioma == "es" ? "App_R04AirAC" : "App_R04AirACEN";
                var dt = _dbHelper.ExecuteStoredProcedure(storedProcedureName);
                return FilterAndSerialize(dt,
                    row => row.Field<int>("Id_RefMk") == Id_RefMk && (McilL != 0
                        ? row.Field<double>("McilL") == McilL
                        : row.Field<string>("MotCap") == MotCap));
            }
            catch (InvalidCastException ex)
            {
                return ex.Message;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetFiltro(string PF_Ref, string sIdioma)
        {
            try
            {
                string storedProcedureName =
                    string.IsNullOrEmpty(sIdioma) || sIdioma == "es" ? "App_eme_Filtro" : "App_eme_FiltroEN";
                var dt = _dbHelper.ExecuteStoredProcedure(storedProcedureName);
                return FilterAndSerialize(dt, row => row.Field<string>("PF_Ref") == PF_Ref);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetSr_Referencias(string sIdioma, string PF_Ref)
        {
            try
            {
                string storedProcedureName =
                    string.IsNullOrEmpty(sIdioma) || sIdioma == "es" ? "App_sr_referencias" : "App_sr_referenciasEN";
                var dt = _dbHelper.ExecuteStoredProcedure(storedProcedureName);
                var sPfRef = PF_Ref.ToUpper();
                return FilterAndSerialize(dt, row => row.Field<string>("PREMIUM Ref").Contains(sPfRef));
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetSt_CompetidoresRef(string sIdioma, string OtraRef)
        {
            try
            {
                string sORef = OtraRef.ToUpper();
                string storedProcedureName =
                    string.IsNullOrEmpty(sIdioma) || sIdioma == "es"
                        ? "App_sr_competidoresref"
                        : "App_sr_competidoresrefEN";
                var dt = _dbHelper.ExecuteStoredProcedure(storedProcedureName);
                return FilterAndSerialize(dt, row => row.Field<string>("Otra Ref") == sORef);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetSr_Allfilters(string sIdioma)
        {
            try
            {
                string storedProcedureName =
                    string.IsNullOrEmpty(sIdioma) || sIdioma == "es" ? "App_sr_allfilters" : "App_sr_allfiltersEN";
                var dt = _dbHelper.ExecuteStoredProcedure(storedProcedureName);
                return JsonConvert.SerializeObject(dt, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetNumberPageAllfillters(string sIdioma, string Tipo)
        {
            try
            {
                string storedProcedureName =
                    string.IsNullOrEmpty(sIdioma) || sIdioma == "es" ? "App_sr_allfilters" : "App_sr_allfiltersEN";
                var dt = _dbHelper.ExecuteStoredProcedure(storedProcedureName);
                var results = dt.AsEnumerable()
                    .Where(row => string.IsNullOrEmpty(Tipo) || row.Field<string>("Tipo") == Tipo);
                int nCount = results.Count();
                int nRecordsPage = Convert.ToInt32(ConfigurationManager.AppSettings["RecordsPage"]);
                if (nCount > 0)
                {
                    int nResul2 = (int)Math.Ceiling((double)nCount / nRecordsPage);
                    var pages = Enumerable.Range(1, nResul2).Select(i => new { Numero = i, Cantidad = nResul2 });
                    return JsonConvert.SerializeObject(pages, Formatting.Indented);
                }

                return "Proceso no genero resultados";
            }
            catch (Exception ex)
            {
                return "Error";
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string MovePageAllfillters(string sIdioma, string Page, string Tipo)
        {
            try
            {
                int nPage = Convert.ToInt32(Page);
                int nRecordsPage = Convert.ToInt32(ConfigurationManager.AppSettings["RecordsPage"]);
                int nSkip = (nPage - 1) * nRecordsPage;
                string storedProcedureName =
                    string.IsNullOrEmpty(sIdioma) || sIdioma == "es" ? "App_sr_allfilters" : "App_sr_allfiltersEN";
                var dt = _dbHelper.ExecuteStoredProcedure(storedProcedureName);
                var results = dt.AsEnumerable()
                    .Where(row => string.IsNullOrEmpty(Tipo) || row.Field<string>("Tipo") == Tipo)
                    .Skip(nSkip).Take(nRecordsPage);
                if (results.Any())
                {
                    var filteredTable = results.CopyToDataTable();
                    return JsonConvert.SerializeObject(filteredTable, Formatting.Indented);
                }

                return "El proceso de paginación no genero resultados";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetSr_Fichas(string PF_Ref, string Tipo, string sIdioma)
        {
            try
            {
                string storedProcedureName = sIdioma == "es" ? "App_sr_fichas" : "App_sr_fichasEN";

                var dt = _dbHelper.ExecuteStoredProcedure(storedProcedureName);
                return FilterAndSerialize(dt, row => row.Field<string>("PF_Ref") == PF_Ref.ToUpper());
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private static string FilterAndSerialize(DataTable dt, Func<DataRow, bool> filter)
        {
            var filteredRows = dt.AsEnumerable().Where(filter);
            if (!filteredRows.Any()) return "No results found";
            var filteredTable = filteredRows.CopyToDataTable();
            return JsonConvert.SerializeObject(filteredTable, Formatting.Indented);
        }
    }
}