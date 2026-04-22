using System;
using System.Collections.Generic;

namespace GISGameFramework.Core
{
    public enum InteractionPointType { Quiz = 1, Search = 2, OpenTreasure = 3 }
    public enum TreasureType { Search = 1, Open = 2 }
    public enum TreasureDiscoveryMode { SearchAtPoint = 1, VisibleWhenLitAndClear = 2 }
    public enum QuizMode { SingleChoice = 1, MultipleChoice = 2 }
    public enum QuizSessionType { Solo = 1, PlayerVsPlayer = 2 }
    public enum MessageType { C2S_POS = 1, C2S_QUIZ_RESULT = 2, C2S_USE_TR = 3, S2C_ENCOUNTER = 4, S2C_TREASURE = 5, S2C_ACH = 6, RANKING_LIST = 7, QUIZ_DATA = 8, SERVER_ACK = 9, QUIZ_START = 10, PK_START = 11, SCORE_CHANGED = 12, NAV_HINT = 13, PK_RESULT = 14 }
    public enum PlayerState { Offline = 0, Exploring = 1, InQuiz = 2, InPk = 3 }
    public enum AchievementTriggerType
    {
        FirstQuiz              = 1,  // 首次答题正确
        FirstTreasure          = 2,  // 首次获得宝藏
        TreasureCollectionCount = 3, // 累计宝藏数
        PkVictory              = 4,  // PK 胜利
        AreaExploration        = 5,  // 探索地点数
        QuizCorrectCount       = 6,  // 累计答题正确数
        ScoreThreshold         = 7,  // 积分达到阈值
        RankFirst              = 8,  // 积分榜第一
        AllAreasUnlocked       = 9,  // 解锁全部地点
        NavigationUsed         = 10, // 使用过导航
        FirstPk                = 11, // 首次参与 PK
        PkWinStreak            = 12, // 连续 PK 胜利
        PkWinCount             = 13, // 累计 PK 胜利数
        PkComeback             = 14, // 逆风翻盘
        MultiPlayerOnline      = 15, // 多人同时在线
        AllAreasAndRankFirst   = 16, // 全能冠军（积分第一且解锁全部地点）
    }
    public enum QuizSessionStatus { Pending = 0, InProgress = 1, Completed = 2 }
    public enum ErrorCodes { None = 0, NotImplemented = 1000, InvalidArgument = 1001, NotFound = 1002, AlreadyExists = 1003, InvalidState = 1004, SerializationFailed = 1005, RouteNotFound = 1006, InternalError = 1500 }

    public class ResponseResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int ErrorCode { get; set; }
        public T Data { get; set; }
        public DateTime Timestamp { get; set; }

        public ResponseResult()
        {
            Timestamp = DateTime.UtcNow;
        }
    }

    public static class ResponseFactory
    {
        public static ResponseResult<T> Ok<T>(T data, string message)
        {
            return new ResponseResult<T> { Success = true, Message = message, ErrorCode = (int)ErrorCodes.None, Data = data };
        }

        public static ResponseResult<T> Fail<T>(ErrorCodes errorCode, string message)
        {
            return new ResponseResult<T> { Success = false, Message = message, ErrorCode = (int)errorCode, Data = default(T) };
        }

        public static ResponseResult<T> NotImplemented<T>(string moduleName, string methodName)
        {
            return Fail<T>(ErrorCodes.NotImplemented, moduleName + "." + methodName + " is not implemented in this framework stage.");
        }
    }

    public class GeoPosition { public double Latitude { get; set; } public double Longitude { get; set; } public double Altitude { get; set; } public double Accuracy { get; set; } }
    public class PlayerPreferences { public bool GpsEnabled { get; set; } public bool MusicEnabled { get; set; } public bool SoundEffectEnabled { get; set; } public string DeviceId { get; set; } }

    public class PlayerProfile
    {
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public GeoPosition CurrentPosition { get; set; }
        public int TotalScore { get; set; }
        public int CurrentRank { get; set; }
        public PlayerState State { get; set; }
        public PlayerPreferences Preferences { get; set; }
        public IList<string> AchievementIds { get; set; }
        public IList<string> CollectedTreasureIds { get; set; }
        public IList<int> ExploredAreaIds { get; set; }

        public PlayerProfile()
        {
            CurrentPosition = new GeoPosition();
            Preferences = new PlayerPreferences();
            AchievementIds = new List<string>();
            CollectedTreasureIds = new List<string>();
            ExploredAreaIds = new List<int>();
        }
    }

    public class AreaBoundary
    {
        public string BoundaryId { get; set; }
        public IList<GeoPosition> Vertices { get; set; }
        public GeoPosition Center { get; set; }
        public double Radius { get; set; }

        public AreaBoundary()
        {
            Vertices = new List<GeoPosition>();
        }
    }

    public class GameArea
    {
        public string AreaId { get; set; }
        public string AreaName { get; set; }
        public string Description { get; set; }
        public AreaBoundary Boundary { get; set; }
        public IList<string> InteractionPointIds { get; set; }

        public GameArea()
        {
            Boundary = new AreaBoundary();
            InteractionPointIds = new List<string>();
        }
    }

    public class InteractionPoint
    {
        public string PointId { get; set; }
        public string AreaId { get; set; }
        public InteractionPointType PointType { get; set; }
        public string DisplayName { get; set; }
        public GeoPosition Position { get; set; }
        public double TriggerRadius { get; set; }
        public bool IsActive { get; set; }
        public string LinkedContentId { get; set; }

        public InteractionPoint()
        {
            Position = new GeoPosition();
            IsActive = true;
        }
    }

    public class TreasureInfo
    {
        public string TreasureId { get; set; }
        public string TreasureName { get; set; }
        public TreasureType TreasureType { get; set; }
        public TreasureDiscoveryMode DiscoveryMode { get; set; }
        public string Description { get; set; }
        public int ScoreValue { get; set; }
        public string IconPath { get; set; }
        public GeoPosition Position { get; set; }
        public string SpawnPointId { get; set; }
        public bool IsCollected { get; set; }
        public int RequiredLightLevel { get; set; }
        public int Ability { get; set; }
        public int Rarity { get; set; }
        public string ImgName { get; set; }


        public TreasureInfo()
        {
            Position = new GeoPosition();
        }
    }

    public class TreasureAlbumEntry { public string TreasureId { get; set; } public string TreasureName { get; set; } public string ImagePath { get; set; } public DateTime? CollectedAt { get; set; } public bool IsUnlocked { get; set; } }
    public class TreasureDiscoveryResult { public TreasureInfo Treasure { get; set; } public int ScoreAwarded { get; set; } public bool AddedToAlbum { get; set; } public string Detail { get; set; } }

    public class QuizQuestion
    {
        public string QuestionId { get; set; }
        public string Content { get; set; }
        public QuizMode Mode { get; set; }
        public IList<string> Options { get; set; }
        public string AnswerKey { get; set; }
        public int ScoreValue { get; set; }

        public QuizQuestion()
        {
            Options = new List<string>();
        }
    }

    public class QuizAnswerRecord
    {
        public string PlayerId { get; set; }
        public string QuestionId { get; set; }
        public string SubmittedAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public int ScoreAwarded { get; set; }
        public DateTime SubmittedAt { get; set; }

        public QuizAnswerRecord()
        {
            SubmittedAt = DateTime.UtcNow;
        }
    }

    public class PkRoundResult
    {
        public string SessionId { get; set; }
        public string QuestionId { get; set; }
        public string PlayerId { get; set; }
        public bool IsCorrect { get; set; }
        public int ScoreAwarded { get; set; }
        public bool IsSessionCompleted { get; set; }
    }

    public class QuizSession
    {
        public string SessionId { get; set; }
        public QuizSessionType SessionType { get; set; }
        public QuizSessionStatus Status { get; set; }
        public string InitiatorPlayerId { get; set; }
        public string OpponentPlayerId { get; set; }
        public IList<string> QuestionIds { get; set; }
        public IList<QuizAnswerRecord> Answers { get; set; }
        public int CurrentQuestionIndex { get; set; }
        public int InitiatorScore { get; set; }
        public int OpponentScore { get; set; }
        public string WinnerPlayerId { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public QuizSession()
        {
            QuestionIds = new List<string>();
            Answers = new List<QuizAnswerRecord>();
            StartedAt = DateTime.UtcNow;
            Status = QuizSessionStatus.Pending;
        }
    }

    public class QuizSubmission { public string SessionId { get; set; } public string PlayerId { get; set; } public string QuestionId { get; set; } public string Answer { get; set; } }
    public class QuizSubmissionResult { public string SessionId { get; set; } public string QuestionId { get; set; } public string PlayerId { get; set; } public bool IsCorrect { get; set; } public int ScoreAwarded { get; set; } public bool IsSessionCompleted { get; set; } public QuizSession Session { get; set; } }
    public class AchievementInfo { public string AchievementId { get; set; } public string Name { get; set; } public string Description { get; set; } public string ImagePath { get; set; } public AchievementTriggerType TriggerType { get; set; } public int Threshold { get; set; } }
    public class ScoreChange { public string PlayerId { get; set; } public int Delta { get; set; } public int BaseScore { get; set; } public string Reason { get; set; } public DateTime CreatedAt { get; set; } public ScoreChange() { CreatedAt = DateTime.UtcNow; } }
    public class LeaderboardEntry { public int Rank { get; set; } public string PlayerId { get; set; } public string PlayerName { get; set; } public int Score { get; set; } }
    public class EnvironmentState { public int GlobalLightLevel { get; set; } public bool IsShadowPhase { get; set; } public string Weather { get; set; } public DateTime UpdatedAt { get; set; } public EnvironmentState() { UpdatedAt = DateTime.UtcNow; } }
    public class VisibilityCheckResult { public bool IsVisible { get; set; } public bool HasLineOfSight { get; set; } public bool MeetsLightRequirement { get; set; } public bool MeetsDistanceRequirement { get; set; } public double Distance { get; set; } public string DetailMessage { get; set; } public int ActualLightLevel { get; set; } }
    //技能结果
    public class SkillUsageResult {
        public bool Success { get; set; }
        public string SkillId { get; set; }
        public NavigationHint NearestTreasureHint { get; set; }
        public string Message { get; set; } 
    }
    public class NavigationHint { public string TargetId { get; set; } public double Distance { get; set; } public string DirectionText { get; set; } }
    public class MessageDispatchResult { public bool Success { get; set; } public string Detail { get; set; } public MessageEnvelope IncomingMessage { get; set; } public IList<MessageEnvelope> OutgoingMessages { get; set; } public object BusinessResult { get; set; } public MessageDispatchResult() { OutgoingMessages = new List<MessageEnvelope>(); } }

    public class PlayerDashboard
    {
        public PlayerProfile Player { get; set; }
        public IList<AchievementInfo> Achievements { get; set; }
        public IList<TreasureAlbumEntry> Album { get; set; }
        public IList<LeaderboardEntry> Leaderboard { get; set; }

        public PlayerDashboard()
        {
            Achievements = new List<AchievementInfo>();
            Album = new List<TreasureAlbumEntry>();
            Leaderboard = new List<LeaderboardEntry>();
        }
    }

    public class DemoSnapshot
    {
        public PlayerProfile Player { get; set; }
        public QuizSession CurrentSession { get; set; }
        public IList<LeaderboardEntry> Leaderboard { get; set; }
        public IList<AchievementInfo> Achievements { get; set; }
        public IList<TreasureAlbumEntry> Album { get; set; }
        public IList<string> RecentMessageTypes { get; set; }
        public string LastPkWinnerId { get; set; }
        public string SummaryText { get; set; }

        public DemoSnapshot()
        {
            Leaderboard = new List<LeaderboardEntry>();
            Achievements = new List<AchievementInfo>();
            Album = new List<TreasureAlbumEntry>();
            RecentMessageTypes = new List<string>();
        }
    }
}
