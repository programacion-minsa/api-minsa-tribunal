using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using webApiTribunal.Models.Entities;
using webApiTribunal.Models.Responses;
using webApiTribunal.Repositories.Interfaces;

namespace webApiTribunal.Repositories.Services;

public class TribunalService : ITribunalService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDbContextFactory<DBContext> _dbContext;

    public TribunalService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor,
        IDbContextFactory<DBContext> dbContext)
    {
        _httpClient = httpClientFactory.CreateClient("HttpClientApiTribunalElectoral");
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

    public async Task<ResponseModel<FindPersonModel>> GetPatientById(string id)
    {
        var response = await _httpClient.GetAsync($"VerificarPersona?Cedula={id}");
        response.EnsureSuccessStatusCode();
        string xmlResponse = await response.Content.ReadAsStringAsync();
        var jsonResponse = TransformXmlToJson(xmlResponse);
        if (!jsonResponse.Success)
        {
            return new ResponseModel<FindPersonModel>()
            {
                StatusCode = jsonResponse.StatusCode,
                Success = false,
                Data = null,
                Message = jsonResponse.Message
            };
        }

        return new ResponseModel<FindPersonModel>()
        {
            StatusCode = 200,
            Success = true,
            Data = jsonResponse.Data,
            Message = jsonResponse.Message
        };
    }

    public async Task<ResponseModel<bool>> StoreUserPetitionData(string id, bool responseStatus, string responseMessage)
    {
        ResponseModel<bool> response = new ResponseModel<bool>();
        var httpContext = _httpContextAccessor.HttpContext;
        var headers = httpContext?.Request.Headers;

        string userAgent = headers != null && headers.ContainsKey("User-Agent") ? headers["User-Agent"].ToString() : "N/A";
        string apiKey = headers != null && headers.ContainsKey("X-api-key") ? headers["X-api-key"].ToString() : "";
        int appId = 0;
        string appName = "";

        await using var localContext = await _dbContext.CreateDbContextAsync();
        if (!String.IsNullOrEmpty(apiKey))
        {
            var appData = await localContext.AppsAccessToken.Where(x => x.AppToken == apiKey).FirstOrDefaultAsync();
            if (appData != null)
            {
                appId = appData.Id;
                appName = appData.AppName ?? "";
            }
        }

        string remoteIp = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "N/A";

        AppsAccessLog log = new AppsAccessLog()
        {
            UserAgent = userAgent,
            UserIpAddress = remoteIp,
            AppId = appId,
            AppApiKey = apiKey,
            AppName = appName,
            RequestId = id,
            RequestDate = DateTime.Now,
            ResponseStatus = responseStatus,
            ResponseMessage = responseMessage
        };

        localContext.AppsAccessLog.Add(log);
        try
        {
            await localContext.SaveChangesAsync();
            response.Success = true;
            response.Message = "OK";
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = "Error: " + ex.Message;
        }

        return response;
    }

    private ResponseModel<FindPersonModel> TransformXmlToJson(string xmlContent)
    {
        FindPersonModel findData = new FindPersonModel();
        if (string.IsNullOrEmpty(xmlContent))
        {
            return new ResponseModel<FindPersonModel>()
            {
                StatusCode = 400,
                Success = false,
                Message = "error: no se cargo la información del servicio del tribunal",
                Data = null
            };
        }

        try
        {
            // DATOS QUE SE RETORNAN
            PersonModel personaData = new PersonModel();
            MessageModel messageData;

            // LEYENDO EL XML RETORNADO POR EL WEBSERVICE DEL TRIBUNAL
            XDocument doc = XDocument.Parse(xmlContent);
            string innerXmlString = doc.Root.Value;
            XDocument innerDoc = XDocument.Parse(innerXmlString);
            XNamespace ns = "http://tempuri.org/DatasetPersona.xsd";
            XElement personaPublica = innerDoc.Descendants(ns + "PersonaPublica").FirstOrDefault();
            XElement personaConfidencial = innerDoc.Descendants(ns + "PersonaConfidencial").FirstOrDefault();
            //XElement imagenes = innerDoc.Descendants(ns + "Imagenes").FirstOrDefault();
            XElement mensajes = innerDoc.Descendants(ns + "Mensajes").FirstOrDefault();


            if (personaPublica != null)
            {
                personaData.Cedula = ((string)personaPublica.Element(ns + "cedula") ?? "").Trim();
                personaData.CedulaProvincia = ((string)personaPublica.Element(ns + "provincia") ?? "").Trim();
                personaData.CedulaTomo = ((string)personaPublica.Element(ns + "tomo") ?? "").Trim();
                personaData.CedulaAsiento = ((string)personaPublica.Element(ns + "asiento") ?? "").Trim();
                personaData.CedulaNombre = ((string)personaPublica.Element(ns + "nombreCedula") ?? "").Trim()
                    .Replace(" , ", ", ");

                string cedulaVencimiento =
                    ((string)personaPublica.Element(ns + "fecha_vencimiento_cedula")! ?? "").Trim();
                if (!string.IsNullOrEmpty(cedulaVencimiento))
                {
                    DateTimeOffset dto = DateTimeOffset.Parse(cedulaVencimiento);
                    personaData.CedulaVencimiento = dto.ToString("yyyy-MM-dd");
                }

                personaData.PrimerNombre = ((string)personaPublica.Element(ns + "primer_nombre") ?? "").Trim();
                personaData.ApellidoPaterno = ((string)personaPublica.Element(ns + "apellido_paterno") ?? "").Trim();
                personaData.ApellidoMeterno = ((string)personaPublica.Element(ns + "apellido_materno" ?? "")).Trim();

                personaData.Sexo = ((string)personaPublica.Element(ns + "sexo")).Trim();
                personaData.EstadoCivil = ((string)personaPublica.Element(ns + "estado_civil") ?? "").Trim();
                personaData.TipoSangre = ((string)personaPublica.Element(ns + "descripcion_tipo_sangre") ?? "").Trim();

                string fechaNacimiento = ((string)personaPublica.Element(ns + "fecha_nacimiento") ?? "").Trim();
                if (!string.IsNullOrEmpty(cedulaVencimiento))
                {
                    DateTimeOffset dto = DateTimeOffset.Parse(fechaNacimiento);
                    personaData.FechaNacimiento = dto.ToString("yyyy-MM-dd");
                }

                personaData.LugarNacimiento = ((string)personaPublica.Element(ns + "LugarDeNacimiento") ?? "").Trim();
                personaData.InstalacionNacimiento =
                    ((string)personaPublica.Element(ns + "lugarnacimientope") ?? "").Trim();

                personaData.Pais = ((string)personaPublica.Element(ns + "pais") ?? "").Trim();
                personaData.Provincia = ((string)personaPublica.Element(ns + "prov") ?? "").Trim();
                personaData.Distrito = ((string)personaPublica.Element(ns + "distrito") ?? "").Trim();
                personaData.Corregimiento = ((string)personaPublica.Element(ns + "corregimiento") ?? "").Trim();


                personaData.ResidenciaBarrio = ((string)personaPublica.Element(ns + "barrio_residencia") ?? "").Trim();
                personaData.ResidenciaCalle = ((string)personaPublica.Element(ns + "calle_residencia") ?? "").Trim();
                personaData.ResidenciaEdificioCasa =
                    ((string)personaPublica.Element(ns + "edificio_casa") ?? "").Trim();
            }

            if (personaConfidencial != null)
            {
                personaData.CentroVotacionNombre =
                    ((string)personaConfidencial.Element(ns + "nombre_centro") ?? "").Trim();
                personaData.CentroVotacionProvincia =
                    ((string)personaConfidencial.Element(ns + "provincia_nombre") ?? "").Trim();
                personaData.CentroVotacionDistrito =
                    ((string)personaConfidencial.Element(ns + "distrito_nombre") ?? "").Trim();
                personaData.CentroVotacionCorregimiento =
                    ((string)personaConfidencial.Element(ns + "corregimiento_nombre") ?? "").Trim();

                personaData.MadrePrimerNombre =
                    ((string)personaConfidencial.Element(ns + "primer_nombre_madre") ?? "").Trim();
                personaData.MadreSegundoNombre =
                    ((string)personaConfidencial.Element(ns + "segundo_nombre_madre") ?? "").Trim();
                personaData.MadreApellidoMaterno =
                    ((string)personaConfidencial.Element(ns + "apellido_paterno_madre") ?? "").Trim();
                personaData.MadreApellidoPaterno =
                    ((string)personaConfidencial.Element(ns + "apellido_materno_madre") ?? "").Trim();
                personaData.MadreApellidoCasada =
                    ((string)personaConfidencial.Element(ns + "apellido_casada_madre") ?? "").Trim();
                personaData.MadreCedula = ((string)personaConfidencial.Element(ns + "cedula_madre") ?? "").Trim();

                personaData.PadrePrimerNombre =
                    ((string)personaConfidencial.Element(ns + "primer_nombre_padre") ?? "").Trim();
                personaData.PadreSegundoNombre =
                    ((string)personaConfidencial.Element(ns + "segundo_nombre_padre") ?? "").Trim();
                personaData.PadreApellidoPaterno =
                    ((string)personaConfidencial.Element(ns + "apellido_paterno_padre") ?? "").Trim();
                personaData.PadreApellidoMaterno =
                    ((string)personaConfidencial.Element(ns + "apellido_materno_padre") ?? "").Trim();
                personaData.PadreCedula = ((string)personaConfidencial.Element(ns + "cedula_padre") ?? "").Trim();
            }

            if (personaPublica != null && personaConfidencial != null)
            {
                findData.Person = personaData;
            }

            int webServiceCode = 0;
            string webServiceMessage = "";

            if (mensajes != null)
            {
                webServiceCode = Convert.ToInt32(((string)mensajes.Element(ns + "CodMensaje")! ?? "0").Trim());
                if (webServiceCode == 534)
                {
                    webServiceMessage = ((string)mensajes.Element(ns + "Mensaje")! ?? "").Trim();
                }
            }

            messageData = new MessageModel()
            {
                Code = webServiceCode,
                Message = GetErrorMessage(webServiceCode, webServiceMessage),
            };

            findData.Message = messageData;

            return new ResponseModel<FindPersonModel>()
            {
                StatusCode = 200,
                Success = true,
                Message = "información cargada exitosamente.",
                Data = findData
            };
        }
        catch (System.Xml.XmlException e)
        {
            return new ResponseModel<FindPersonModel>()
            {
                StatusCode = 400,
                Success = false,
                Message = $"server error: XML parsing error: {e.Message}",
                Data = null
            };
        }
        catch (Exception e)
        {
            return new ResponseModel<FindPersonModel>()
            {
                StatusCode = 400,
                Success = false,
                Message = $"server error: transformation error: {e.Message}",
                Data = null
            };
        }
    }

    private string GetErrorMessage(int code, string message = "")
    {
        // LIMPIANDO MENSAJE CUANDO ES PERSONA DIFUNTA
        string fechaDefuncion = "";
        if (code == 534)
        {
            // BORRANDO PRIMER BLOQUE DEL MENSAJE
            message = message.Replace("Cedula de ciudadano Difunto. Fecha Defuncion : ", "").ToLower();

            // BORRANDO DIAS
            message = message.Replace("lunes", "").Trim();
            message = message.Replace("martes", "").Trim();
            message = message.Replace("miercoles", "").Trim();
            message = message.Replace("jueves", "").Trim();
            message = message.Replace("viernes", "").Trim();
            message = message.Replace("sabado", "").Trim();
            message = message.Replace("domingo", "").Trim();
            message = message.Trim();
            message = message.Replace(" de ", "-").Trim();

            string[] dateStringArray = message.Split('-');
            int monthCode = dateStringArray[1] switch
            {
                "enero" => 1,
                "febrero" => 2,
                "marzo" => 3,
                "abril" => 4,
                "mayo" => 5,
                "junio" => 6,
                "julio" => 7,
                "agosto" => 8,
                "septiembre" => 9,
                "octubre" => 10,
                "noviembre" => 11,
                "diciembre" => 12,
            };
            fechaDefuncion = $"{dateStringArray[0].PadLeft(2, '0')}-{monthCode.ToString().PadLeft(2, '0')}-{dateStringArray[2]}";
        }

        return code switch
        {
            530 => "El número de cédula no existe, por favor verifique",
            531 => "Cédula que se está consultando está en investigación",
            532 => "Cédula que se está consultando no existe",
            533 => "Inscripción de cédula cancelada",
            534 => $"Cédula de ciudadano difunto. fecha defunción: {fechaDefuncion}",
            535 => "Menor emancipado",
            536 => "Menor de edad",
            542 => "El cliente ha llegado a su máximo de consultas diarias permitidas",
            999 => "En este momento no podemos atender su consulta. Intente más tarde",
            106 => "Basado en el artículo 76 del código de la familia",
            0 => "Persona cargada exitosamente.",
            _ => "Error",
        };
    }
}