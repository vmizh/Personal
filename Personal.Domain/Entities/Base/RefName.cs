using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Personal.Domain.Entities.Base;

[DebuggerDisplay("'{Id}' {Name}")]
public class RefName : IEquatable<RefName>
{
    [BsonRepresentation(BsonType.String)]
    [Display(Name = "Id", AutoGenerateField = false)]
    public Guid Id { set; get; } = Guid.NewGuid();

    [Display(Name = "Наименование", AutoGenerateField = true)]
    public required string Name { set; get; } = string.Empty;

    public bool Equals(RefName? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    public override string ToString()
    {
        return Name;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((RefName)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(RefName? left, RefName? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RefName? left, RefName? right)
    {
        return !Equals(left, right);
    }
}
