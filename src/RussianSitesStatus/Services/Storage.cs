using RussianSitesStatus.Models;

namespace RussianSitesStatus.Services;
public class Storage<T> where T : Site
{
    private readonly Dictionary<string, T> _items = new Dictionary<string, T>();

    public T Get(string id)
    {
        if (_items.TryGetValue(id, out var value))
        {
            return value;
        }

        return default(T);
    }

    public void ReplaceAll(IEnumerable<T> items)
    {
        _items.Clear();
        foreach (var item in items)
        {
            _items.Add(item.Id, item);
        }
    }

    public IEnumerable<T> GetAll()
    {
        return _items.Values;
    }

    public void Replace(T item)
    {
        _items[item.Id] = item;
    }
}