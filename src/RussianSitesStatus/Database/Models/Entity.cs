namespace RussianSitesStatus.Database.Models;

public class Entity : IEquatable<Entity>
{
    public long Id { get; set; }

    public override int GetHashCode()
    {
        return (int)Id;
    }

    public override bool Equals(object? obj)
    {
        var entity = obj as Entity;
        if (entity == null)
        {
            return false;
        }

        return entity.Id == this.Id;
    }

    public override string ToString()
    {
        return $"{GetType().Name} [{Id}]";
    }

    public bool Equals(Entity other)
    {
        if (other == null)
        {
            return false;
        }

        return other.Id == this.Id;
    }
}
