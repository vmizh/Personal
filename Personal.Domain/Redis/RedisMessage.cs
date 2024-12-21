namespace Personal.Domain.Redis;

public class RedisMessage
{
    public DateTime? DocDate { get; set; }
    public required RedisMessageDocumentOperationTypeEnum MessageType { set; get; } =
        RedisMessageDocumentOperationTypeEnum.NotDefined;
    public required string Message { get; set; }

    public Dictionary<string,object> ExternalValues { get; set; } = new Dictionary<string,object>();
}
