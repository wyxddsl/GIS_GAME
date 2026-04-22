using System;
using GISGameFramework.Core;
using GISGameFramework.Core.Contracts;
using GISGameFramework.Game.ArcEngine;

namespace GISGameFramework.Game.Modules
{
    public class GpsModule : IGpsService
    {
        public ResponseResult<double> CalculateDistance(GeoPosition from, GeoPosition to)
        {
            return ResponseFactory.Ok(AEUtil.CalcDistanceMeters(from, to), "Distance calculated.");
        }

        public ResponseResult<bool> Initialize()
        {
            return ResponseFactory.Ok(true, "GPS module initialized.");
        }

        public ResponseResult<GeoPosition> NormalizePosition(GeoPosition position)
        {
            if (position == null)
            {
                return ResponseFactory.Fail<GeoPosition>(ErrorCodes.InvalidArgument, "Position is required.");
            }

            return ResponseFactory.Ok(position, "Position normalized.");
        }

        public ResponseResult<bool> SetGpsEnabled(string playerId, bool enabled)
        {
            return ResponseFactory.Ok(true, "GPS state recorded.");
        }

        public ResponseResult<bool> Shutdown()
        {
            return ResponseFactory.Ok(true, "GPS module closed.");
        }
    }
}
