namespace webApiTribunal.Models.Responses;

public class PersonModel
{
    // DATOS DE LA PERSONA
    public string Cedula { get; set; } = "";
    public string CedulaProvincia { get; set; } = "";
    public string CedulaTomo { get; set; } = "";
    public string CedulaAsiento { get; set; } = "";
    public string CedulaNombre { get; set; } = "";
    public string CedulaVencimiento { get; set; } = "";

    public string PrimerNombre { get; set; } = "";
    public string ApellidoPaterno { get; set; } = "";
    public string ApellidoMeterno { get; set; } = "";

    public string Sexo { get; set; } = "";
    public string EstadoCivil { get; set; } = "";
    public string FechaNacimiento { get; set; } = "";
    public string TipoSangre { get; set; } = "";
    
    public string LugarNacimiento { get; set; } = "";
    public string InstalacionNacimiento { get; set; } = "";
 
    public string Pais { get; set; } = "";
    public string Provincia { get; set; } = "";
    public string Distrito { get; set; } = "";
    public string Corregimiento { get; set; } = "";
    
    public string ResidenciaBarrio { get; set; } = "";
    public string ResidenciaCalle { get; set; } = "";
    public string ResidenciaEdificioCasa { get; set; } = "";
    
    public string CentroVotacionNombre { get; set; } = "";
    public string CentroVotacionProvincia { get; set; } = "";
    public string CentroVotacionDistrito { get; set; } = "";
    public string CentroVotacionCorregimiento { get; set; } = "";
    
    // DATOS FAMILIARES
    public string MadrePrimerNombre { get; set; } = "";
    public string MadreSegundoNombre { get; set; } = "";
    public string MadreApellidoPaterno { get; set; } = "";
    public string MadreApellidoMaterno { get; set; } = "";
    public string MadreApellidoCasada { get; set; } = "";
    public string MadreCedula { get; set; } = "";
   
    public string PadrePrimerNombre { get; set; } = "";
    public string PadreSegundoNombre { get; set; } = "";
    public string PadreApellidoPaterno { get; set; } = "";
    public string PadreApellidoMaterno { get; set; } = "";
    public string PadreCedula { get; set; } = "";
}