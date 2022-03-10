using RussianSitesStatus.Models;

namespace RussianSitesStatus.Services;
public class InMemoryStorage<T> where T : SiteVM
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Dictionary<long, T> _items = new();

    public T Get(long id)
    {
        _lock.EnterReadLock();
        try
        {
            if (_items.TryGetValue(id, out var value))
            {
                return value;
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }
        return default(T);
    }

    public void ReplaceAll(IEnumerable<T> items)
    {
        _lock.EnterWriteLock();
        try
        {
            _items.Clear();
            foreach (var item in items)
            {
                _items.Add(item.Id, item);
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public IEnumerable<T> GetAll()
    {
        _lock.EnterReadLock();
        try
        {
            return new List<T>(_items.Values);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Replace(T item)
    {
        _lock.EnterWriteLock();
        try
        {
            _items[item.Id] = item;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public IEnumerable<T> Search(string url)
    {
        //url = url.NormalizeSiteName();
        //var searchRegex = new Regex($@"((http|https)\:\/\/)?(www.)?\.*{Regex.Escape(url)}", RegexOptions.Compiled);

        var results = _items
            .Values
            .Where(x => x.Name.Contains(url));

        return results;
    }
}