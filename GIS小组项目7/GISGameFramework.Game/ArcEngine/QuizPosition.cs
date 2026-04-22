using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using GISGameFramework.Core;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GISGameFramework.Game.ArcEngine
{
    public class QuizPosition
    {
        public const int PointCount = 7;
        public const double BufferMeters = 20.0;
        private const string ElementName = "quiz_point";

        private readonly IMapControl3 _mapControl;
        private readonly List<InteractionPoint> _points = new List<InteractionPoint>();

        public event Action<string> OnQuizPointTriggered;
        public IReadOnlyList<InteractionPoint> Points { get { return _points; } }

        public QuizPosition(IMapControl3 mapControl)
        {
            _mapControl = mapControl;
        }

        public bool Load(Action<string> log)
        {
            _points.Clear();

            string path = System.IO.Path.Combine(Application.StartupPath, "gezi_json.geojson");
            if (!System.IO.File.Exists(path))
            {
                if (log != null) log("未找到答题点位文件：" + path);
                return false;
            }

            try
            {
                string json = System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8);
                var coords = ParseCoordinates(json);

                if (coords.Count < PointCount)
                {
                    if (log != null) log(string.Format("geojson 点位数量不足，需要 {0} 个，实际 {1} 个。", PointCount, coords.Count));
                    return false;
                }

                // 随机打乱后取前 7 个
                Shuffle(coords);
                List<double[]> coords7 = new List<double[]>();
                //31.287740995575255,121.49515797727919
                //31.286442035126679,121.49828765365613

                //31.2853219087703,121.49380723555223
                //31.286234508474553,121.49287657411178

                //31.282926971870413,121.49812705937367
                //31.287049265207962,121.50034488676854

                //31.284247162380293,121.49704539917985
                coords7.Add(new double[] { 31.287740995575255, 121.49515797727919 });
                coords7.Add(new double[] { 31.286442035126679, 121.49828765365613 });

                coords7.Add(new double[] { 31.2853219087703, 121.49380723555223 });
                coords7.Add(new double[] { 31.286234508474553, 121.49287657411178 });

                coords7.Add(new double[] { 31.282926971870413, 121.49812705937367 });
                coords7.Add(new double[] { 31.287049265207962, 121.50034488676854 });
                coords7.Add(new double[] { 31.284247162380293, 121.49704539917985 });
                for (int i = 0; i < PointCount; i++)
                {
                    _points.Add(new InteractionPoint
                    {
                        PointId = "qp_" + (i + 1),
                        PointType = InteractionPointType.Quiz,
                        DisplayName = "答题点" + (i + 1),
                        TriggerRadius = BufferMeters,
                        IsActive = true,
                        Position = new GeoPosition { Latitude = coords7[i][0], Longitude = coords7[i][1] }
                    });
                }

                PlaceMarkersOnMap();
                if (log != null) log(string.Format("答题点位已随机部署，共 {0} 个。", PointCount));
                return true;
            }
            catch (Exception ex)
            {
                if (log != null) log("加载答题点位失败：" + ex.Message);
                return false;
            }
        }

        public void CheckProximity(string playerId, GeoPosition playerPos)
        {
            foreach (var pt in _points)
            {
                if (!pt.IsActive) continue;
                if (AEUtil.CalcDistanceMeters(playerPos, pt.Position) <= BufferMeters)
                {
                    pt.IsActive = false;
                    if (OnQuizPointTriggered != null)
                        OnQuizPointTriggered(pt.PointId);
                }
            }
        }

        public void ReactivatePoint(string pointId)
        {
            foreach (var pt in _points)
                if (pt.PointId == pointId) { pt.IsActive = true; break; }
        }

        public void PlaceMarkersOnMap()
        {
            IGraphicsContainer gc = _mapControl.Map as IGraphicsContainer;
            if (gc == null) return;

            RemoveMarkersFromMap();

            IRgbColor green = new RgbColorClass { Red = 0, Green = 180, Blue = 0 };
            IRgbColor black = new RgbColorClass { Red = 0, Green = 0, Blue = 0 };

            foreach (var pt in _points)
            {
                gc.AddElement(AEUtil.getPoint(green, pt.Position.Longitude, pt.Position.Latitude,
                    esriSimpleMarkerStyle.esriSMSSquare, ElementName), 0);

                IPoint lp = new PointClass { X = pt.Position.Longitude, Y = pt.Position.Latitude };
                gc.AddElement(AEUtil.getTextLabel(black, lp, pt.DisplayName, ElementName), 0);
            }

            RefreshMap();
        }

        public void RemoveMarkersFromMap()
        {
            IGraphicsContainer gc = _mapControl.Map as IGraphicsContainer;
            if (gc == null) return;

            gc.Reset();
            var toDelete = new List<IElement>();
            IElement elem;
            while ((elem = gc.Next()) != null)
            {
                IElementProperties props = elem as IElementProperties;
                if (props != null && props.Name == ElementName)
                    toDelete.Add(elem);
            }
            foreach (var e in toDelete) gc.DeleteElement(e);
            RefreshMap();
        }

        private void RefreshMap()
        {
            IActiveView av = _mapControl.Map as IActiveView;
            if (av != null) av.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        private static void Shuffle(List<double[]> list)
        {
            var rnd = new Random();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                var tmp = list[i]; list[i] = list[j]; list[j] = tmp;
            }
        }

        private static List<double[]> ParseCoordinates(string json)
        {
            var result = new List<double[]>();
            var regex = new System.Text.RegularExpressions.Regex(
                @"""coordinates""\s*:\s*\[\s*([-\d.]+)\s*,\s*([-\d.]+)");
            foreach (System.Text.RegularExpressions.Match m in regex.Matches(json))
            {
                double lon, lat;
                if (double.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out lon) &&
                    double.TryParse(m.Groups[2].Value, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out lat))
                    result.Add(new double[] { lat, lon });
            }
            return result;
        }
    }
}
