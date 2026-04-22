using System.Collections.Generic;
using GISGameFramework.Core;
using GISGameFramework.Core.Contracts;

namespace GISGameFramework.Game.Modules
{
    public class AchievementModule : IAchievementService
    {
        private readonly Dictionary<string, AchievementInfo> _definitions = new Dictionary<string, AchievementInfo>();
        private readonly Dictionary<string, List<AchievementInfo>> _unlocked = new Dictionary<string, List<AchievementInfo>>();

        public ResponseResult<bool> AddAchievement(AchievementInfo achievement)
        {
            if (achievement == null || string.IsNullOrWhiteSpace(achievement.AchievementId))
                return ResponseFactory.Fail<bool>(ErrorCodes.InvalidArgument, "Achievement argument is invalid.");

            _definitions[achievement.AchievementId] = achievement;
            return ResponseFactory.Ok(true, "Achievement definition added.");
        }

        // 布尔触发（首次答题、首次宝藏、PK胜利等）
        public ResponseResult<IList<AchievementInfo>> Evaluate(string playerId, AchievementTriggerType triggerType)
        {
            return EvaluateWithValue(playerId, triggerType, 0);
        }

        // 带数值触发（答题数、积分、宝藏数、PK胜场等）
        public ResponseResult<IList<AchievementInfo>> EvaluateWithValue(string playerId, AchievementTriggerType triggerType, int value)
        {
            var result = new List<AchievementInfo>();
            var unlocked = EnsureUnlocked(playerId);

            foreach (var achievement in _definitions.Values)
            {
                if (achievement.TriggerType != triggerType) continue;
                if (ContainsAchievement(unlocked, achievement.AchievementId)) continue;

                bool conditionMet;
                switch (triggerType)
                {
                    case AchievementTriggerType.QuizCorrectCount:
                    case AchievementTriggerType.TreasureCollectionCount:
                    case AchievementTriggerType.ScoreThreshold:
                    case AchievementTriggerType.PkWinCount:
                    case AchievementTriggerType.PkWinStreak:
                    case AchievementTriggerType.AreaExploration:
                        conditionMet = (value >= achievement.Threshold);
                        break;
                    default:
                        conditionMet = true;
                        break;
                }

                if (conditionMet)
                {
                    unlocked.Add(achievement);
                    result.Add(achievement);
                }
            }

            return ResponseFactory.Ok(
                (IList<AchievementInfo>)result,
                result.Count > 0 ? "Achievement evaluation completed." : "No new achievement unlocked.");
        }

        public ResponseResult<IList<AchievementInfo>> GetUnlocked(string playerId)
        {
            return ResponseFactory.Ok((IList<AchievementInfo>)EnsureUnlocked(playerId), "Unlocked achievement list loaded.");
        }

        public ResponseResult<bool> Initialize()
        {
            return ResponseFactory.Ok(true, "Achievement module initialized.");
        }

        public ResponseResult<bool> Shutdown()
        {
            return ResponseFactory.Ok(true, "Achievement module closed.");
        }

        private List<AchievementInfo> EnsureUnlocked(string playerId)
        {
            List<AchievementInfo> achievements;
            if (!_unlocked.TryGetValue(playerId, out achievements))
            {
                achievements = new List<AchievementInfo>();
                _unlocked[playerId] = achievements;
            }
            return achievements;
        }

        private static bool ContainsAchievement(IEnumerable<AchievementInfo> achievements, string achievementId)
        {
            foreach (var a in achievements)
                if (a.AchievementId == achievementId) return true;
            return false;
        }
    }
}
