using RussianSitesStatus.Models.ViewModels;

namespace RussianSitesStatus.Models;

public class RegionVM : BaseModelVM
{
    public string Name { get; set; }
    public string ProxyUrl { get; set; }
    public string ProxyUser { get; set; }
    public string ProxyPassword { get; set; }
}