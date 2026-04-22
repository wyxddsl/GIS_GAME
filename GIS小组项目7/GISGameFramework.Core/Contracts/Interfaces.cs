using System.Collections.Generic;

namespace GISGameFramework.Core.Contracts
{
    public interface IGameModule
    {
        ResponseResult<bool> Initialize();
        ResponseResult<bool> Shutdown();
    }

    // 区域服务后续可以接入点落面、缓冲区和叠加分析。
    public interface IMapAreaService : IGameModule
    {
        ResponseResult<bool> AddArea(GameArea area);
        ResponseResult<GameArea> QueryArea(GeoPosition position);
        ResponseResult<IList<GameArea>> GetAllAreas();
    }

    // 点位服务后续可以接入邻近查询、路径引导和网络分析。
    public interface IInteractionPointService : IGameModule
    {
        ResponseResult<bool> AddPoint(InteractionPoint point);
        ResponseResult<IList<InteractionPoint>> GetNearbyPoints(GeoPosition position, double radius);
        ResponseResult<NavigationHint> BuildNavigationHint(GeoPosition playerPosition, string pointId);
        ResponseResult<InteractionPoint> TryTriggerPoint(string playerId, string pointId);
    }

    public interface IPlayerService : IGameModule
    {
        ResponseResult<bool> RegisterPlayer(PlayerProfile player);
        ResponseResult<PlayerProfile> GetPlayer(string playerId);
        ResponseResult<bool> UpdatePlayer(PlayerProfile player);
        ResponseResult<bool> UpdatePlayerPosition(string playerId, GeoPosition position);
        ResponseResult<bool> UpdatePreferences(string playerId, PlayerPreferences preferences);
        ResponseResult<IList<PlayerProfile>> GetAllPlayers();
    }

    public interface IGpsService : IGameModule
    {
        ResponseResult<bool> SetGpsEnabled(string playerId, bool enabled);
        ResponseResult<GeoPosition> NormalizePosition(GeoPosition position);
        ResponseResult<double> CalculateDistance(GeoPosition from, GeoPosition to);
    }

    public interface IQuizService : IGameModule
    {
        ResponseResult<bool> AddQuestion(QuizQuestion question);
        ResponseResult<QuizQuestion> GetQuestion(string questionId);
        ResponseResult<QuizSession> GetSession(string sessionId);
        ResponseResult<IList<QuizQuestion>> GetQuestionsForSession(string sessionId);
        ResponseResult<QuizSession> StartSoloQuiz(string playerId, string pointId);
        ResponseResult<QuizSession> StartPkQuiz(string playerId, string opponentId);
        ResponseResult<QuizSubmissionResult> SubmitAnswer(QuizSubmission submission);
        ResponseResult<QuizSession> EndSession(string sessionId);
    }

    public interface ITreasureService : IGameModule
    {
        ResponseResult<bool> AddTreasure(TreasureInfo treasure);
        ResponseResult<TreasureDiscoveryResult> SearchTreasure(string playerId, string pointId);
        ResponseResult<VisibilityCheckResult> CheckOpenTreasureVisibility(string playerId, string treasureId, VisibilityCheckResult environmentVisibility);
        ResponseResult<TreasureDiscoveryResult> CollectOpenTreasure(string playerId, string treasureId);
        ResponseResult<IList<TreasureAlbumEntry>> GetAlbum(string playerId);
        ResponseResult<TreasureInfo> GetTreasure(string treasureId);
        // 随机将搜索宝藏和露天宝藏分配到固定点位
        ResponseResult<bool> RandomAllocateTreasures(IList<InteractionPoint> searchPoints, IList<InteractionPoint> openTreasurePoints);
        // 获取所有宝藏点位（用于最邻近分析）
        ResponseResult<IList<InteractionPoint>> GetAllTreasurePoints();
        // 根据点位ID查找绑定的宝藏
        ResponseResult<TreasureInfo> GetTreasureByPointId(string pointId);
        // 奖励初始宝藏（ID:26，开局自动获得）
        ResponseResult<TreasureDiscoveryResult> AwardInitialTreasure(string playerId);
        // 重置所有宝藏的收集状态（用于重新布设）
        void ResetAllTreasures();
    }

    public interface IAchievementService : IGameModule
    {
        ResponseResult<bool> AddAchievement(AchievementInfo achievement);
        ResponseResult<IList<AchievementInfo>> Evaluate(string playerId, AchievementTriggerType triggerType);
        ResponseResult<IList<AchievementInfo>> GetUnlocked(string playerId);
    }

    public interface IScoreService : IGameModule
    {
        ResponseResult<int> ApplyScoreChange(ScoreChange scoreChange);
        ResponseResult<int> GetPlayerScore(string playerId);
    }

    public interface ILeaderboardService : IGameModule
    {
        ResponseResult<IList<LeaderboardEntry>> Refresh(IList<PlayerProfile> players);
        ResponseResult<IList<LeaderboardEntry>> GetTop(int count);
        ResponseResult<LeaderboardEntry> GetPlayerRank(string playerId);
    }

    public interface IMultiplayerService : IGameModule
    {
        ResponseResult<PlayerProfile> FindNearbyOpponent(string playerId, double triggerDistance);
    }

    // 环境服务是 GIS 分析的重要接点，可替换成阴影、可视域和通视分析。
    public interface IEnvironmentService : IGameModule
    {
        ResponseResult<EnvironmentState> GetCurrentState();
        ResponseResult<VisibilityCheckResult> EvaluateVisibility(GeoPosition observer, GeoPosition target, int requiredLightLevel);
        // 设置全局光照等级（0-100）
        ResponseResult<bool> SetGlobalLightLevel(int lightLevel);
        // 获取当前全局光照等级
        ResponseResult<int> GetGlobalLightLevel();
        // 推进光照周期（模拟日夜变化，可定时调用）
        ResponseResult<bool> UpdateLightingCycle();
    }

    // 技能服务：最邻近分析，提供最近藏宝点的方向和距离提示
    public interface ISkillService : IGameModule
    {
        ResponseResult<SkillUsageResult> UseNearestTreasureHint(string playerId);
    }

    public interface IDataTransferService : IGameModule
    {
        ResponseResult<MessageDispatchResult> Receive(MessageEnvelope envelope);
        ResponseResult<MessageEnvelope> BuildPushMessage(MessageType messageType, PayloadBase payload);
        ResponseResult<string> SerializeEnvelope(MessageEnvelope envelope);
        ResponseResult<MessageEnvelope> DeserializeEnvelope(string json);
    }

    public interface ITransportChannel
    {
        ResponseResult<bool> Send(string message);
        ResponseResult<string> Receive();
    }

    public interface IMessageSerializer
    {
        ResponseResult<string> Serialize(MessageEnvelope envelope);
        ResponseResult<T> Deserialize<T>(string content) where T : class;
    }

    public interface IMessageRouter
    {
        ResponseResult<MessageDispatchResult> Route(MessageEnvelope envelope);
    }

    // ArcEngine 适配层是后续对接空间查询、叠加分析和通视分析的统一入口。
    public interface IArcEngineAdapter
    {
        ResponseResult<GameArea> QueryArea(GeoPosition position);
        ResponseResult<bool> CheckLineOfSight(GeoPosition observer, GeoPosition target);
        ResponseResult<bool> ComputeViewshed(GeoPosition observer, GeoPosition target);
        ResponseResult<int> EvaluateLightCondition(GeoPosition position);
    }
}
