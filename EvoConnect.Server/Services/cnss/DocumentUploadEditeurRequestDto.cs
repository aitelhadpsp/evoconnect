namespace EvoConnect.Server.Services.cnss;

public class DocumentUploadEditeurRequestDto
{
    public List<IFormFile> Files { get; set; }
    public long IdentifiantTechnique { get; set; }
    public string NumeroFse { get; set; }
}
