using RussianSitesStatus.Models;

namespace RussianSitesStatus.Services;

public class SiteStorage : InMemoryStorage<Site>
{
    public IEnumerable<Site> Search(string url)
    {
        var results = _items
            .Values
            .Where(x => x.Name.Contains(url));

        return results;
    }
}
