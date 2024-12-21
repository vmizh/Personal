using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using Personal.Domain.Entities.Base;

namespace Personal.Domain.Entities;
[DebuggerDisplay("'{_id}' Name")]
public class Author : IIdentity
{
    [BsonId(IdGenerator = typeof(GuidGenerator))]
    [BsonRepresentation(BsonType.String)]
    public Guid _id { get; set; }

    public string? LastName { set; get; }
    public string? FirstName { set; get; }
    public string? SecondName { set; get; }

    [NotMapped] public string Name => $"{LastName} {FirstName} {SecondName}";

    public DateTime? BirthDate { set; get; }
    public DateTime? DeathDate { set; get; }
    public RefName? Country { set; get; }
}
