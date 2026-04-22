using System.Collections.Generic;
using GISGameFramework.Core;
using GISGameFramework.Core.Contracts;

namespace GISGameFramework.Game.Modules
{
    public class ScoreModule : IScoreService
    {
        private readonly Dictionary<string, int> _scores = new Dictionary<string, int>();

        public ResponseResult<int> ApplyScoreChange(ScoreChange scoreChange)
        {
            int score;
            if (!_scores.TryGetValue(scoreChange.PlayerId, out score))
            {
                score = scoreChange.BaseScore;
            }
            score += scoreChange.Delta;
            _scores[scoreChange.PlayerId] = score;
            return ResponseFactory.Ok(score, "Score updated.");
        }

        public ResponseResult<int> GetPlayerScore(string playerId)
        {
            int score;
            _scores.TryGetValue(playerId, out score);
            return ResponseFactory.Ok(score, "Score loaded.");
        }

        public ResponseResult<bool> Initialize()
        {
            return ResponseFactory.Ok(true, "Score module initialized.");
        }

        public ResponseResult<bool> Shutdown()
        {
            return ResponseFactory.Ok(true, "Score module closed.");
        }
    }
}
