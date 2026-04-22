using GISGameFramework.Core;
using GISGameFramework.Core.Contracts;
using GISGameFramework.Game.ArcEngine;

namespace GISGameFramework.Game.Modules
{
    public class MultiplayerModule : IMultiplayerService
    {
        private readonly IPlayerService _playerService;

        public MultiplayerModule(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        public ResponseResult<PlayerProfile> FindNearbyOpponent(string playerId, double triggerDistance)
        {
            var playersResult = _playerService.GetAllPlayers();
            if (!playersResult.Success)
            {
                return ResponseFactory.Fail<PlayerProfile>(ErrorCodes.InvalidState, playersResult.Message);
            }

            var selfResult = _playerService.GetPlayer(playerId);
            if (!selfResult.Success)
            {
                return ResponseFactory.Fail<PlayerProfile>(ErrorCodes.NotFound, selfResult.Message);
            }

            foreach (var player in playersResult.Data)
            {
                if (player.PlayerId == playerId || player.State == PlayerState.InPk)
                {
                    continue;
                }

                var distance = AEUtil.CalcDistanceMeters(selfResult.Data.CurrentPosition, player.CurrentPosition);
                if (distance <= triggerDistance || triggerDistance <= 0)
                {
                    return ResponseFactory.Ok(player, "Nearby opponent found.");
                }
            }

            return ResponseFactory.Fail<PlayerProfile>(ErrorCodes.NotFound, "No nearby opponent.");
        }

        public ResponseResult<bool> Initialize()
        {
            return ResponseFactory.Ok(true, "Multiplayer module initialized.");
        }

        public ResponseResult<bool> Shutdown()
        {
            return ResponseFactory.Ok(true, "Multiplayer module closed.");
        }
    }
}
