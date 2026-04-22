using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using GISGameFramework.Core;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GISGameFramework.Game.ArcEngine
{
    public class MultiPlayerWalker
    {
        private const double StepMeters = 3.0;

        public event Action<string, GeoPosition> OnPlayerPositionChanged;
        public Action RefreshPlayersOnMap { get; set; }

        private readonly IMapControl3 _mapControl;
        private readonly Dictionary<string, WalkState> _states = new Dictionary<string, WalkState>();
        private Timer _timer;
        private int _intervalMs;

        private class WalkState
        {
            public List<IPoint> Path = new List<IPoint>();
            public int SegIndex;
            public double SegT;
        }

        public MultiPlayerWalker(IMapControl3 mapControl)
        {
            _mapControl = mapControl;
        }

        public bool IsAnyWalking
        {
            get { return _timer != null && _timer.Enabled; }
        }

        public bool IsWalking(string playerId)
        {
            return _states.ContainsKey(playerId);
        }

        public void StartWalk(string playerId, int stepIntervalMs)
        {
            StartWalk(playerId, 0, stepIntervalMs);
        }

        public void StartWalk(string playerId, int pathIndex, int stepIntervalMs)
        {
            var path = ExtractPathByIndex(pathIndex);
            if (path.Count < 2) return;

            _intervalMs = stepIntervalMs;
            _states[playerId] = new WalkState { Path = path };

            FirePosition(playerId, path[0]);
            StartTimer();
        }

        public void StopWalk(string playerId)
        {
            _states.Remove(playerId);
            if (_states.Count == 0) StopTimer();
        }

        public void StopAll()
        {
            _states.Clear();
            StopTimer();
        }

        public void PauseAll()
        {
            if (_timer != null) _timer.Stop();
        }

        public void ResumeAll()
        {
            if (_timer != null && _states.Count > 0) _timer.Start();
        }

        private void StartTimer()
        {
            if (_timer != null) return;
            _timer = new Timer { Interval = _intervalMs };
            _timer.Tick += OnTick;
            _timer.Start();
        }

        private void StopTimer()
        {
            if (_timer == null) return;
            _timer.Stop();
            _timer.Dispose();
            _timer = null;
        }

        private void OnTick(object sender, EventArgs e)
        {
            double stepUnit = StepMeters / 111000.0;
            var finished = new List<string>();
            var keys = new List<string>(_states.Keys);

            foreach (var pid in keys)
            {
                WalkState s;
                if (!_states.TryGetValue(pid, out s)) continue;

                if (s.SegIndex >= s.Path.Count - 1) { finished.Add(pid); continue; }

                IPoint from = s.Path[s.SegIndex];
                IPoint to   = s.Path[s.SegIndex + 1];
                double segLen = Dist(from, to);

                if (segLen < 1e-10) { s.SegIndex++; s.SegT = 0; continue; }

                s.SegT += stepUnit / segLen;

                while (s.SegT >= 1.0)
                {
                    s.SegT -= 1.0;
                    s.SegIndex++;
                    if (s.SegIndex >= s.Path.Count - 1) { finished.Add(pid); goto next; }
                    from = s.Path[s.SegIndex];
                    to   = s.Path[s.SegIndex + 1];
                    segLen = Dist(from, to);
                    if (segLen < 1e-10) { s.SegIndex++; s.SegT = 0; goto next; }
                    s.SegT = s.SegT * (stepUnit / segLen);
                }

                FirePosition(pid, new PointClass
                {
                    X = from.X + (to.X - from.X) * s.SegT,
                    Y = from.Y + (to.Y - from.Y) * s.SegT
                });

                next:;
            }

            foreach (var pid in finished) StopWalk(pid);
            if (RefreshPlayersOnMap != null) RefreshPlayersOnMap();
        }

        private void FirePosition(string playerId, IPoint mapPt)
        {
            if (OnPlayerPositionChanged != null)
                OnPlayerPositionChanged(playerId, MapPointToGeo(mapPt));
        }

        private List<IPoint> ExtractPathByIndex(int index)
        {
            var points = new List<IPoint>();
            IGraphicsContainer gc = _mapControl.Map as IGraphicsContainer;
            if (gc == null) return points;

            gc.Reset();
            IElement elem;
            int found = 0;
            while ((elem = gc.Next()) != null)
            {
                if (elem.Geometry is IPolyline)
                {
                    if (found == index)
                    {
                        var ptCol = elem.Geometry as IPointCollection;
                        if (ptCol != null)
                            for (int i = 0; i < ptCol.PointCount; i++)
                                points.Add(ptCol.get_Point(i));
                        break;
                    }
                    found++;
                }
            }
            return points;
        }

        private GeoPosition MapPointToGeo(IPoint pt)
        {
            IPoint clone = new PointClass { X = pt.X, Y = pt.Y };
            if (_mapControl.Map.LayerCount > 0)
            {
                var mapSr = ((IGeoDataset)_mapControl.Map.get_Layer(0)).SpatialReference;
                var wgs84 = new SpatialReferenceEnvironmentClass()
                    .CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
                clone.SpatialReference = mapSr;
                clone.Project(wgs84 as ISpatialReference);
            }
            return new GeoPosition { Longitude = clone.X, Latitude = clone.Y, Accuracy = 5 };
        }

        private static double Dist(IPoint a, IPoint b)
        {
            double dx = a.X - b.X, dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
