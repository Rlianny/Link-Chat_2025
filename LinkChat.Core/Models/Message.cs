using System.Text.Json.Serialization;

namespace LinkChat.Core.Models;

[JsonDerivedType(typeof(ChatAck), typeDiscriminator: "ChatAck")]
[JsonDerivedType(typeof(File), typeDiscriminator: "File")]
[JsonDerivedType(typeof(FileAck), typeDiscriminator: "FileAck")]
[JsonDerivedType(typeof(FileChunk), typeDiscriminator: "FileChunk")]
[JsonDerivedType(typeof(FileStart), typeDiscriminator: "FileStart")]
[JsonDerivedType(typeof(HeartbeatMessage), typeDiscriminator: "HeartbeatMessage")]
[JsonDerivedType(typeof(MessageReaction), typeDiscriminator: "MessageReaction")]
[JsonDerivedType(typeof(TextMessage), typeDiscriminator: "TextMessage")]
[JsonDerivedType(typeof(UserStatus), typeDiscriminator: "UserStatus")]
public abstract class Message
{
    public DateTime TimeStamp { get { return timeStamp; } private set { } }
    public string UserName { get { return userName; } private set { } }
    protected DateTime timeStamp;
    protected string userName;

    public Message(string userName, DateTime timeStamp)
    {
        (this.userName, this.timeStamp) = (userName, timeStamp);
    }
}

