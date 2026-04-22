using GISGameFramework.Core;
using GISGameFramework.Core.Contracts;

namespace GISGameFramework.Game.ArcEngine
{
    public class NullArcEngineAdapter : IArcEngineAdapter
    {
        public ResponseResult<bool> CheckLineOfSight(GeoPosition observer, GeoPosition target)
        {
            return ResponseFactory.NotImplemented<bool>("NullArcEngineAdapter", "CheckLineOfSight");
        }

        public ResponseResult<bool> ComputeViewshed(GeoPosition observer, GeoPosition target)
        {
            return ResponseFactory.NotImplemented<bool>("NullArcEngineAdapter", "ComputeViewshed");
        }

        public ResponseResult<int> EvaluateLightCondition(GeoPosition position)
        {
            return ResponseFactory.NotImplemented<int>("NullArcEngineAdapter", "EvaluateLightCondition");
        }

        public ResponseResult<GameArea> QueryArea(GeoPosition position)
        {
            return ResponseFactory.NotImplemented<GameArea>("NullArcEngineAdapter", "QueryArea");
        }
    }
}
