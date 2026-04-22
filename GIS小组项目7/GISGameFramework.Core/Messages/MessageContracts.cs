using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GISGameFramework.Core
{
    [DataContract]
    public class MessageHeader
    {
        [DataMember(Order = 1, Name = "msg_type")]
        public MessageType MessageType { get; set; }
        [DataMember(Order = 2)]
        public string ClientId { get; set; }
        [DataMember(Order = 3)]
        public DateTime SentAt { get; set; }

        public MessageHeader() { SentAt = DateTime.UtcNow; }
    }

    [DataContract]
    [KnownType(typeof(PositionPayload))]
    [KnownType(typeof(QuizResultPayload))]
    [KnownType(typeof(UseTreasurePayload))]
    [KnownType(typeof(EncounterPayload))]
    [KnownType(typeof(TreasureNotificationPayload))]
    [KnownType(typeof(AchievementNotificationPayload))]
    [KnownType(typeof(RankingListPayload))]
    [KnownType(typeof(QuizDataPayload))]
    [KnownType(typeof(ServerAckPayload))]
    [KnownType(typeof(QuizStartPayload))]
    [KnownType(typeof(PkStartPayload))]
    [KnownType(typeof(PkResultPayload))]
    [KnownType(typeof(ScoreChangedPayload))]
    [KnownType(typeof(NavigationHintPayload))]
    public abstract class PayloadBase { }

    [DataContract]
    public class MessageEnvelope
    {
        [DataMember(Order = 1)]
        public MessageHeader Header { get; set; }
        [DataMember(Order = 2)]
        public PayloadBase Payload { get; set; }

        public MessageEnvelope() { Header = new MessageHeader(); }
    }

    [DataContract]
    public class PositionPayload : PayloadBase
    {
        [DataMember(Order = 1, Name = "x")]
        public double X { get; set; }
        [DataMember(Order = 2, Name = "y")]
        public double Y { get; set; }
        [DataMember(Order = 3, Name = "acc")]
        public double Accuracy { get; set; }
    }
    [DataContract]
    public class QuizResultPayload : PayloadBase { [DataMember(Order = 1)] public string SessionId { get; set; } [DataMember(Order = 2)] public string PlayerId { get; set; } [DataMember(Order = 3)] public string QuestionId { get; set; } [DataMember(Order = 4)] public string Answer { get; set; } [DataMember(Order = 5)] public bool IsCorrect { get; set; } }
    [DataContract]
    public class UseTreasurePayload : PayloadBase { [DataMember(Order = 1)] public string TreasureId { get; set; } [DataMember(Order = 2)] public int AbilityId { get; set; } }
    [DataContract]
    public class EncounterPayload : PayloadBase { [DataMember(Order = 1)] public string TargetPlayerId { get; set; } }
    [DataContract]
    public class TreasureNotificationPayload : PayloadBase { [DataMember(Order = 1)] public string TreasureId { get; set; } [DataMember(Order = 2)] public string Content { get; set; } [DataMember(Order = 3)] public int Score { get; set; } }
    [DataContract]
    public class AchievementNotificationPayload : PayloadBase { [DataMember(Order = 1)] public string Name { get; set; } [DataMember(Order = 2)] public string Description { get; set; } [DataMember(Order = 3)] public string Image { get; set; } }
    [DataContract]
    public class ServerAckPayload : PayloadBase { [DataMember(Order = 1)] public bool Success { get; set; } [DataMember(Order = 2)] public string Message { get; set; } [DataMember(Order = 3)] public int ErrorCode { get; set; } }
    [DataContract]
    public class QuizStartPayload : PayloadBase { [DataMember(Order = 1)] public string SessionId { get; set; } [DataMember(Order = 2)] public string PlayerId { get; set; } [DataMember(Order = 3)] public IList<string> QuestionIds { get; set; } public QuizStartPayload() { QuestionIds = new List<string>(); } }
    [DataContract]
    public class PkStartPayload : PayloadBase { [DataMember(Order = 1)] public string SessionId { get; set; } [DataMember(Order = 2)] public string Player1Id { get; set; } [DataMember(Order = 3)] public string Player2Id { get; set; } [DataMember(Order = 4)] public IList<string> QuestionIds { get; set; } public PkStartPayload() { QuestionIds = new List<string>(); } }
    [DataContract]
    public class PkResultPayload : PayloadBase { [DataMember(Order = 1)] public string SessionId { get; set; } [DataMember(Order = 2)] public string WinnerPlayerId { get; set; } [DataMember(Order = 3)] public int Player1Score { get; set; } [DataMember(Order = 4)] public int Player2Score { get; set; } [DataMember(Order = 5)] public bool IsDraw { get; set; } }
    [DataContract]
    public class ScoreChangedPayload : PayloadBase { [DataMember(Order = 1)] public string PlayerId { get; set; } [DataMember(Order = 2)] public int Delta { get; set; } [DataMember(Order = 3)] public int TotalScore { get; set; } [DataMember(Order = 4)] public string Reason { get; set; } }
    [DataContract]
    public class NavigationHintPayload : PayloadBase { [DataMember(Order = 1)] public string TargetId { get; set; } [DataMember(Order = 2)] public double Distance { get; set; } [DataMember(Order = 3)] public string DirectionText { get; set; } }

    [DataContract]
    public class RankingListPayload : PayloadBase
    {
        [DataMember(Order = 1)]
        public IList<RankingListItem> Items { get; set; }

        public RankingListPayload() { Items = new List<RankingListItem>(); }
    }

    [DataContract]
    public class RankingListItem { [DataMember(Order = 1)] public int Rank { get; set; } [DataMember(Order = 2)] public string UserId { get; set; } [DataMember(Order = 3)] public int Score { get; set; } }

    [DataContract]
    public class QuizDataPayload : PayloadBase
    {
        [DataMember(Order = 1)]
        public string QuestionId { get; set; }
        [DataMember(Order = 2)]
        public string Content { get; set; }
        [DataMember(Order = 3)]
        public string Mode { get; set; }
        [DataMember(Order = 4)]
        public int OptionCount { get; set; }
        [DataMember(Order = 5)]
        public IList<string> Options { get; set; }
        [DataMember(Order = 6)]
        public string AnswerKey { get; set; }

        public QuizDataPayload() { Options = new List<string>(); }
    }
}
