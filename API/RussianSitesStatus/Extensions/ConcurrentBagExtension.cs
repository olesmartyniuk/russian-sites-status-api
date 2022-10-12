using System.Collections.Concurrent;

namespace RussianSitesStatus.Extensions;

public static class ConcurrentBagExtension
{
    public static void Add(this ConcurrentBag<string> bag, IEnumerable<string> items)
    {
        foreach (var item in items)
        {
            bag.Add(item);
        }
    }
}