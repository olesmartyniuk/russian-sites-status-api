using RussianSitesStatus.Models.ViewModels;

namespace RussianSitesStatus.Services;
public class BaseInMemoryStorage<T> where T : BaseModel
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
}