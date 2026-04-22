using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using GISGameFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GISGameFramework.Game.ArcEngine
{
    public class AEUtil
    {
        public const double duZhuanMi = 111000.0;

        public static readonly string[] SearchPointIds = {
            "sp_1","sp_2","sp_3","sp_4","sp_5","sp_6","sp_7","sp_8","sp_9","sp_10",
            "sp_11","sp_12","sp_13","sp_14","sp_15","sp_16","sp_17","sp_18","sp_19"
        };

        public static readonly string[] OpenPointIds = {
            "op_1","op_2","op_3","op_4","op_5","op_6"
        };

        #region 获取Elment
        public static IElement getTextLabel(IRgbColor textColor, IPoint mapPoint, string textDesc, string elementName)
        {
            ITextSymbol textSymbol = new TextSymbolClass();
            textSymbol.Size = 10;

            textSymbol.Color = textColor as IColor;

            IPoint labelPoint = new PointClass();
            labelPoint.X = mapPoint.X;
            labelPoint.Y = mapPoint.Y;

            ITextElement textElement = new TextElementClass();
            textElement.Text = textDesc;
            textElement.Symbol = textSymbol;
            IElement labelElement = textElement as IElement;
            labelElement.Geometry = labelPoint;
            IElementProperties labelProps = labelElement as IElementProperties;
            if (labelProps != null)
                labelProps.Name = elementName;
            return labelElement;
        }
        public static IElement getPoint(IRgbColor color, double lon, double lat, esriSimpleMarkerStyle style, string elementName)
        {
            IPoint mapPoint = new PointClass();
            mapPoint.X = lon;
            mapPoint.Y = lat;

            ISimpleMarkerSymbol markerSym = new SimpleMarkerSymbolClass();
            markerSym.Style = style;
            markerSym.Size = 12;
            markerSym.Color = color as IColor;
            markerSym.Outline = true;
            IRgbColor outlineColor = new RgbColorClass();
            outlineColor.Red = 80; outlineColor.Green = 80; outlineColor.Blue = 80;
            markerSym.OutlineColor = outlineColor as IColor;
            markerSym.OutlineSize = 1;

            IMarkerElement markerElem = new MarkerElementClass();
            markerElem.Symbol = markerSym as IMarkerSymbol;
            IElement pointElem = markerElem as IElement;
            pointElem.Geometry = mapPoint;
            IElementProperties pointProps = pointElem as IElementProperties;
            if (pointProps != null) pointProps.Name = elementName;
            return pointElem;
        }
        public static IElement getPicElment(IPoint mapPoint, string imgPath, string treasureName)
        {
            IPictureMarkerSymbol picSym = new PictureMarkerSymbolClass();
            picSym.Size = 20;
            ((IMarkerSymbol)picSym).Size = 20;
            picSym.CreateMarkerSymbolFromFile(esriIPictureType.esriIPicturePNG, imgPath);
            picSym.BackgroundColor = (new RgbColorClass { Red = 255, Green = 255, Blue = 255 }) as IColor;

            IPoint picPt = new PointClass();
            picPt.X = mapPoint.X - 0.0001;
            picPt.Y = mapPoint.Y;
            picPt.SpatialReference = mapPoint.SpatialReference;

            IMarkerElement picElem = new MarkerElementClass();
            picElem.Symbol = picSym as IMarkerSymbol;
            IElement picElement = picElem as IElement;
            picElement.Geometry = picPt;
            IElementProperties picProps = picElement as IElementProperties;
            if (picProps != null) picProps.Name = treasureName;
            return picElement;
        }
        #endregion

        public static bool SamplePointsFromGeoJson(out List<InteractionPoint> searchPoints, out List<InteractionPoint> openPoints, Action<string> addLog)
        {
            searchPoints = new List<InteractionPoint>();
            openPoints = new List<InteractionPoint>();

            string geojsonPath = System.IO.Path.Combine(Application.StartupPath, "gezi_json.geojson");
            if (!System.IO.File.Exists(geojsonPath))
            {
                addLog("未找到 geojson 文件：" + geojsonPath);
                return false;
            }

            try
            {
                string json = System.IO.File.ReadAllText(geojsonPath, System.Text.Encoding.UTF8);
                var coords = ParseGeoJsonCoordinates(json);
                if (coords.Count < SearchPointIds.Length + OpenPointIds.Length)
                {
                    addLog(string.Format("geojson 点位不足 {0} 个。", SearchPointIds.Length + OpenPointIds.Length));
                    return false;
                }

                var rnd = new Random();
                var indices = new List<int>();
                for (int i = 0; i < coords.Count; i++)
                {
                    indices.Add(i);
                }
                for (int i = indices.Count - 1; i > 0; i--)
                {
                    int j = rnd.Next(i + 1);
                    int tmp = indices[i]; 
                    indices[i] = indices[j]; 
                    indices[j] = tmp;
                }

                for (int i = 0; i < SearchPointIds.Length; i++)
                {
                    searchPoints.Add(new InteractionPoint
                    {
                        PointId = SearchPointIds[i],
                        PointType = InteractionPointType.Search,
                        DisplayName = "搜索点" + (i + 1),
                        TriggerRadius = ConUtil.SearchTreaRadius,
                        Position = new GeoPosition { Latitude = coords[indices[i]][0], Longitude = coords[indices[i]][1] }
                    });
                }
                for (int i = 0; i < OpenPointIds.Length; i++)
                {
                    openPoints.Add(new InteractionPoint
                    {
                        PointId = OpenPointIds[i],
                        PointType = InteractionPointType.OpenTreasure,
                        DisplayName = "露天点" + (i + 1),
                        TriggerRadius = ConUtil.OpenTreaDistance,
                        Position = new GeoPosition { Latitude = coords[indices[SearchPointIds.Length + i]][0], Longitude = coords[indices[SearchPointIds.Length + i]][1] }
                    });
                }
                return true;
            }
            catch (Exception ex)
            {
                addLog("解析 geojson 失败：" + ex.Message);
                return false;
            }
        }

        private static List<double[]> ParseGeoJsonCoordinates(string json)
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
                {
                    result.Add(new double[] { lat, lon });
                }
            }
            return result;
        }

        public static string GetHillshadeShpPath()
        {
            return System.IO.Path.Combine(Application.StartupPath, "同济校园地图", "hillshade.shp");
        }
        public static string GetDEMPath()
        {
            string demPath = System.IO.Path.Combine(Application.StartupPath, "同济校园地图", "dem_cgcs2000.tif");
            return demPath;
        }

        public static double CalcDistanceMeters(GeoPosition a, GeoPosition b)
        {
            IPoint ptA = new PointClass { X = a.Longitude, Y = a.Latitude };
            IPoint ptB = new PointClass { X = b.Longitude, Y = b.Latitude };

            ISpatialReference wgs84 = new SpatialReferenceEnvironmentClass()
                .CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
            ptA.SpatialReference = wgs84;
            ptB.SpatialReference = wgs84;

            double distDegrees = ((IProximityOperator)ptA).ReturnDistance(ptB);
            return distDegrees * duZhuanMi;
        }

        public static IPoint ConvertGeoToPoint(GeoPosition positon)
        {
            IPoint point = new PointClass();
            point.X = positon.Longitude;
            point.Y = positon.Latitude;
            return point;
        }

        public static IPoint GetNearestPointInfo(
            IPoint originPoint,
            List<IPoint> targetPoints,
            out double nearestDistance,
            out double angleDegrees,
            out double azimuth)
        {
            nearestDistance = double.MaxValue;
            angleDegrees = 0;
            azimuth = 0;
            IPoint resultPoint = null;

            if (originPoint == null || targetPoints == null || targetPoints.Count == 0)
                return null;
            IProximityOperator proximityOp = originPoint as IProximityOperator;
            foreach (IPoint currentPoint in targetPoints)
            {
                double dist = proximityOp.ReturnDistance(currentPoint);
                if (dist < nearestDistance)
                {
                    nearestDistance = dist;
                    resultPoint = currentPoint;
                }
            }
            if (resultPoint != null)
            {
                ILine line = new LineClass();
                line.PutCoords(originPoint, resultPoint);
                angleDegrees = line.Angle * (180.0 / Math.PI);
                azimuth = (450.0 - angleDegrees) % 360.0;
            }
            return resultPoint;
        }

        /// <summary>
        /// 随机数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="random"></param>
        public static void Shuffle<T>(List<T> list, Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                T tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }
    }
}
