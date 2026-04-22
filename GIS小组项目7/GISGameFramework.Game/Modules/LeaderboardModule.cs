using System.Collections.Generic;
using System.Linq;
using GISGameFramework.Core;
using GISGameFramework.Core.Contracts;

namespace GISGameFramework.Game.Modules
{
    public class LeaderboardModule : ILeaderboardService
    {
        private readonly List<LeaderboardEntry> _entries = new List<LeaderboardEntry>();

        public ResponseResult<LeaderboardEntry> GetPlayerRank(string playerId)
        {
            var entry = _entries.FirstOrDefault(item => item.PlayerId == playerId);
            if (entry == null)
            {
                return ResponseFactory.Fail<LeaderboardEntry>(ErrorCodes.NotFound, "Player is not on the leaderboard.");
            }
            return ResponseFactory.Ok(entry, "Player rank loaded.");
        }

        public ResponseResult<IList<LeaderboardEntry>> GetTop(int count)
        {
            return ResponseFactory.Ok((IList<LeaderboardEntry>)_entries.Take(count).ToList(), "Leaderboard loaded.");
        }

        public ResponseResult<bool> Initialize()
        {
            return ResponseFactory.Ok(true, "Leaderboard module initialized.");
        }

        public ResponseResult<IList<LeaderboardEntry>> Refresh(IList<PlayerProfile> players)
        {
            _entries.Clear();
            var ordered = players.OrderByDescending(player => player.TotalScore).ToList();
            for (int i = 0; i < ordered.Count; i++)
            {
                _entries.Add(new LeaderboardEntry
                {
                    Rank = i + 1,
                    PlayerId = ordered[i].PlayerId,
                    PlayerName = ordered[i].PlayerName,
                    Score = ordered[i].TotalScore
                });
            }
            return ResponseFactory.Ok((IList<LeaderboardEntry>)new List<LeaderboardEntry>(_entries), "Leaderboard refreshed.");
        }

        public ResponseResult<bool> Shutdown()
        {
            return ResponseFactory.Ok(true, "Leaderboard module closed.");
        }
    }
}
