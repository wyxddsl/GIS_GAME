using System;
using System.Collections.Generic;
using GISGameFramework.Core;
using GISGameFramework.Core.Contracts;
using GISGameFramework.Game.ArcEngine;
using ESRI.ArcGIS.Geometry;

namespace GISGameFramework.Game.Modules
{
    public class InteractionPointModule : IInteractionPointService
    {
        private readonly Dictionary<string, InteractionPoint> _points = new Dictionary<string, InteractionPoint>();

        private readonly List<IPoint>           _cachedIPoints = new List<IPoint>();
        private readonly List<InteractionPoint> _cachedPoints  = new List<InteractionPoint>();
        private ESRI.ArcGIS.Geometry.ISpatialReference _wgs84;

        private Func<string, bool> _isPointCollected;

        public void SetCollectedChecker(Func<string, bool> checker)
        {
            _isPointCollected = checker;
        }

        private ESRI.ArcGIS.Geometry.ISpatialReference Wgs84
        {
            get
            {
                if (_wgs84 == null)
                    _wgs84 = new SpatialReferenceEnvironmentClass()
                        .CreateGeographicCoordinateSystem(
                            (int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
                return _wgs84;
            }
        }

        public ResponseResult<bool> AddPoint(InteractionPoint point)
        {
            if (point == null || string.IsNullOrWhiteSpace(point.PointId))
                return ResponseFactory.Fail<bool>(ErrorCodes.InvalidArgument, "Interaction point argument is invalid.");

            _points[point.PointId] = point;
            RebuildCache();
            return ResponseFactory.Ok(true, "Interaction point registered.");
        }

        private void RebuildCache()
        {
            _cachedIPoints.Clear();
            _cachedPoints.Clear();
            foreach (var p in _points.Values)
            {
                IPoint ipt = new PointClass { X = p.Position.Longitude, Y = p.Position.Latitude };
                ipt.SpatialReference = Wgs84;
                _cachedIPoints.Add(ipt);
                _cachedPoints.Add(p);
            }
        }

        public ResponseResult<NavigationHint> BuildNavigationHint(GeoPosition playerPosition, string pointId)
        {
            InteractionPoint point;
            if (!_points.TryGetValue(pointId, out point))
                return ResponseFactory.Fail<NavigationHint>(ErrorCodes.NotFound, "Interaction point not found.");

            var distance = AEUtil.CalcDistanceMeters(playerPosition, point.Position);
            return ResponseFactory.Ok(new NavigationHint
            {
                TargetId = pointId,
                Distance = Math.Round(distance, 2),
                DirectionText = "Direction arrow should be implemented in the UI layer."
            }, "Navigation hint built.");
        }

        public ResponseResult<IList<InteractionPoint>> GetNearbyPoints(GeoPosition position, double radius)
        {
            var nearbyPoints = new List<InteractionPoint>();
            if (_cachedIPoints.Count == 0)
                return ResponseFactory.Ok((IList<InteractionPoint>)nearbyPoints, "No points registered.");

            // 
            var candidateIPoints = new List<IPoint>();
            var candidatePoints  = new List<InteractionPoint>();
            for (int i = 0; i < _cachedPoints.Count; i++)
            {
                var p = _cachedPoints[i];
                if (_isPointCollected != null && _isPointCollected(p.PointId))
                    continue;
                candidateIPoints.Add(_cachedIPoints[i]);
                candidatePoints.Add(p);
            }

            if (candidateIPoints.Count == 0)
                return ResponseFactory.Ok((IList<InteractionPoint>)nearbyPoints, "All treasures collected.");

            IPoint origin = new PointClass { X = position.Longitude, Y = position.Latitude };
            origin.SpatialReference = Wgs84;

            double nearestDist, angle, azimuth;
            IPoint nearest = AEUtil.GetNearestPointInfo(origin, candidateIPoints,
                out nearestDist, out angle, out azimuth);

            if (nearest == null)
                return ResponseFactory.Ok((IList<InteractionPoint>)nearbyPoints, "Nearby point list returned.");

            double nearestMeters = nearestDist * AEUtil.duZhuanMi;
            if (nearestMeters <= radius)
            {
                int idx = candidateIPoints.IndexOf(nearest);
                if (idx >= 0)
                    nearbyPoints.Add(candidatePoints[idx]);
            }

            return ResponseFactory.Ok((IList<InteractionPoint>)nearbyPoints, "Nearby point list returned.");
        }

        public ResponseResult<bool> Initialize()
        {
            return ResponseFactory.Ok(true, "Interaction point module initialized.");
        }

        public ResponseResult<bool> Shutdown()
        {
            return ResponseFactory.Ok(true, "Interaction point module closed.");
        }

        public ResponseResult<InteractionPoint> TryTriggerPoint(string playerId, string pointId)
        {
            InteractionPoint point;
            if (!_points.TryGetValue(pointId, out point))
                return ResponseFactory.Fail<InteractionPoint>(ErrorCodes.NotFound, "Target point does not exist.");

            return ResponseFactory.Ok(point, "Interaction point triggered. Game logic should be filled in later.");
        }
    }
}
