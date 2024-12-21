using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace Personal.Domain.Entities.Base;

public interface IIdentity
{
    Guid _id { set; get; }
}

public interface IName
{
    string Name { set; get; }
    string Note { set; get; }
}

public class BaseReference : IIdentity, IName
{
    [BsonId(IdGenerator = typeof(GuidGenerator))]
    [BsonRepresentation(BsonType.String)]
    public Guid _id { get; set; }
    [MaxLength(500)]
    public virtual string? Name { get; set; }

    [MaxLength(500)]
    public virtual string? Note { get; set; }
}
