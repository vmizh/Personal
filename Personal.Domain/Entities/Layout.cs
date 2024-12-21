using System.Diagnostics;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using Personal.Domain.Entities.Base;
using Personal.Domain.Helper;

namespace Personal.Domain.Entities;

[DebuggerDisplay("'{_id}' Name")]
public class Layout : IIdentity
{
    [BsonId(IdGenerator = typeof(GuidGenerator))]
    [BsonRepresentation(BsonType.String)]
    public Guid _id { get; set; }

    public double FormHeight { set; get; }
    public double FormWidth { set; get; }
    public double FormLeft { set; get; }
    public double FormTop { set; get; }

    public WindowStartupLocation FormStartLocation { set; get; }
    public WindowState FormState { set; get; }

    public string? LayoutString { set; get; }


}
