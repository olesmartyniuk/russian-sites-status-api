using RussianSitesStatus.Models.ViewModels;

namespace RussianSitesStatus.Models;

public class Region : BaseModel
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string ProxyUrl { get; set; }
    public string ProxyUser { get; set; }
    public string ProxyPassword { get; set; }
    public bool ProxyIsActive { get; set; }
}