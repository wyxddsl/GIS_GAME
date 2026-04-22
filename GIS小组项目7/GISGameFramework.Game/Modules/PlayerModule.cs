using System.Collections.Generic;
using GISGameFramework.Core;
using GISGameFramework.Core.Contracts;

namespace GISGameFramework.Game.Modules
{
    public class PlayerModule : IPlayerService
    {
        private readonly Dictionary<string, PlayerProfile> _players = new Dictionary<string, PlayerProfile>();

        public ResponseResult<IList<PlayerProfile>> GetAllPlayers()
        {
            return ResponseFactory.Ok((IList<PlayerProfile>)new List<PlayerProfile>(_players.Values), "Player list loaded.");
        }

        public ResponseResult<PlayerProfile> GetPlayer(string playerId)
        {
            PlayerProfile player;
            if (_players.TryGetValue(playerId, out player))
            {
                return ResponseFactory.Ok(player, "Player loaded.");
            }

            return ResponseFactory.Fail<PlayerProfile>(ErrorCodes.NotFound, "Player not found.");
        }

        public ResponseResult<bool> Initialize()
        {
            return ResponseFactory.Ok(true, "Player module initialized.");
        }

        public ResponseResult<bool> RegisterPlayer(PlayerProfile player)
        {
            if (player == null || string.IsNullOrWhiteSpace(player.PlayerId))
            {
                return ResponseFactory.Fail<bool>(ErrorCodes.InvalidArgument, "Player argument is invalid.");
            }

            _players[player.PlayerId] = player;
            return ResponseFactory.Ok(true, "Player registered.");
        }

        public ResponseResult<bool> Shutdown()
        {
            return ResponseFactory.Ok(true, "Player module closed.");
        }

        public ResponseResult<bool> UpdatePlayer(PlayerProfile player)
        {
            return RegisterPlayer(player);
        }

        public ResponseResult<bool> UpdatePlayerPosition(string playerId, GeoPosition position)
        {
            PlayerProfile player;
            if (!_players.TryGetValue(playerId, out player))
            {
                return ResponseFactory.Fail<bool>(ErrorCodes.NotFound, "Player not found.");
            }

            player.CurrentPosition = position;
            return ResponseFactory.Ok(true, "Player position updated.");
        }

        public ResponseResult<bool> UpdatePreferences(string playerId, PlayerPreferences preferences)
        {
            PlayerProfile player;
            if (!_players.TryGetValue(playerId, out player))
            {
                return ResponseFactory.Fail<bool>(ErrorCodes.NotFound, "Player not found.");
            }

            player.Preferences = preferences;
            return ResponseFactory.Ok(true, "Player preferences updated.");
        }
    }
}
