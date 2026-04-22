using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
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
    public interface IMapControlInteract
    {
        void loadMap();
        void PlacePlayersOnMap();
        void PlaceTreasuresOnMap();
        ResponseResult<IList<PlayerProfile>> AllPlayers { get; set; }
        GameCoreManager GameCoreManager { get; set; }

        bool ShowTreasureMarkerAndText { get; set; }
        bool ShowTreasureImage { get; set; }
        
        void RemoveTreasureFromMap(string pointId);
        void ShowTreasureImageOnMap(string pointId);
        event Action<string, GeoPosition> OnPlayerPositionChanged;
    }
    public class AEMapControlInteract : IMapControlInteract
    {
        private IMapControl3 mapControl;
        private string treasureName = "treasure";
        private string _DemoPlayer1Id = "";

        private bool _showTreasureMarkerAndText = true;
        private bool _showTreasureImage = false;

        public event Action<string, GeoPosition> OnPlayerPositionChanged;
        
        public bool ShowTreasureMarkerAndText
        {
            get { return _showTreasureMarkerAndText; }
            set { _showTreasureMarkerAndText = value; }
        }

        public bool ShowTreasureImage
        {
            get { return _showTreasureImage; }
            set { _showTreasureImage = value; }
        }
        private string DemoPlayer1Id
        {
            get
            {
                return _DemoPlayer1Id;
            }
            set
            {
                _DemoPlayer1Id = value;
            }
        }
        public AEMapControlInteract(IMapControl3 pMapControl)
        {
            mapControl = pMapControl;
        }
        private ResponseResult<IList<PlayerProfile>> allPlayers;
        public ResponseResult<IList<PlayerProfile>> AllPlayers
        {
            set
            {
                allPlayers = value;
            }
            get
            {
                return allPlayers;
            }
        }

        private GameCoreManager _gameCoreManager;
        public GameCoreManager GameCoreManager
        {
            get {
                return _gameCoreManager;
            }
            set
            {
                _gameCoreManager = value;
            }
        }
        public void loadMap()
        {
            string mxd =System.IO.Path.Combine(Application.StartupPath,"map1.mxd");
            this.mapControl.LoadMxFile(mxd);
        }
       
        public void PlacePlayersOnMap()
        {

            if (!AllPlayers.Success)
            {
                return;
            }

            IGraphicsContainer graphicsContainer = this.mapControl.Map as IGraphicsContainer;
            if (graphicsContainer == null)
            {
                return;
            }

            graphicsContainer.Reset();
            IElement elem;
            while ((elem = graphicsContainer.Next()) != null)
            {
                IElementProperties props = elem as IElementProperties;
                if (props != null && props.Name == "player")
                {
                    graphicsContainer.DeleteElement(elem);
                    graphicsContainer.Reset();
                }
            }

            string[] labels = { "玩家一号", "玩家二号" };
            int[] colors = { System.Drawing.Color.Red.ToArgb(), System.Drawing.Color.Blue.ToArgb() };
            int idx = 0;

            foreach (var player in this.allPlayers.Data)
            {
                if (idx >= 2) break;

                double lon = player.CurrentPosition.Longitude;
                double lat = player.CurrentPosition.Latitude;

                IPoint point = new PointClass();
                point.X = lon;
                point.Y = lat;

                ISpatialReferenceFactory srFactory = new SpatialReferenceEnvironmentClass();
                IGeographicCoordinateSystem gcs = srFactory.CreateGeographicCoordinateSystem(
                    (int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
                ISpatialReference sr = gcs as ISpatialReference;
                point.SpatialReference = sr;

                IPoint mapPoint = new PointClass();
                mapPoint.X = lon;
                mapPoint.Y = lat;
                mapPoint.SpatialReference = sr;
                IMap map = this.mapControl.Map;
                ISpatialReference mapSr = ((IGeoDataset)map.get_Layer(0)).SpatialReference;
                if (mapSr != null)
                {
                    mapPoint.Project(mapSr);
                }

                ISimpleMarkerSymbol markerSymbol = new SimpleMarkerSymbolClass();
                markerSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;
                markerSymbol.Size = 14;
                IRgbColor rgbColor = new RgbColorClass();
                rgbColor.Red = System.Drawing.Color.FromArgb(colors[idx]).R;
                rgbColor.Green = System.Drawing.Color.FromArgb(colors[idx]).G;
                rgbColor.Blue = System.Drawing.Color.FromArgb(colors[idx]).B;
                markerSymbol.Color = rgbColor as IColor;
                markerSymbol.Outline = true;
                IRgbColor outlineColor = new RgbColorClass();
                outlineColor.Red = 255; outlineColor.Green = 255; outlineColor.Blue = 255;
                markerSymbol.OutlineColor = outlineColor as IColor;
                markerSymbol.OutlineSize = 1;

                IMarkerElement markerElement = new MarkerElementClass();
                markerElement.Symbol = markerSymbol as IMarkerSymbol;
                IElement pointElement = markerElement as IElement;
                pointElement.Geometry = mapPoint;
                IElementProperties pointProps = pointElement as IElementProperties;
                if (pointProps != null) pointProps.Name = "player";
                graphicsContainer.AddElement(pointElement, 0);

                IRgbColor textColor = new RgbColorClass();
                textColor.Red = System.Drawing.Color.FromArgb(colors[idx]).R;
                textColor.Green = System.Drawing.Color.FromArgb(colors[idx]).G;
                textColor.Blue = System.Drawing.Color.FromArgb(colors[idx]).B;
                IElement labelElement = AEUtil.getTextLabel(textColor, mapPoint, labels[idx], "player");
                graphicsContainer.AddElement(labelElement, 0);

                idx++;
            }

            IActiveView activeView = this.mapControl.Map as IActiveView;
            if (activeView != null)
            {
                activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            }
        }

        public void PlaceTreasuresOnMap()
        {
            IGraphicsContainer graphicsContainer = mapControl.Map as IGraphicsContainer;
            if (graphicsContainer == null) return;

            graphicsContainer.Reset();
            IElement elem;
            var toDelete = new List<IElement>();
            while ((elem = graphicsContainer.Next()) != null)
            {
                IElementProperties props = elem as IElementProperties;
                if (props != null && props.Name == treasureName)
                    toDelete.Add(elem);
            }
            foreach (var e2 in toDelete)
                graphicsContainer.DeleteElement(e2);

            ISpatialReference mapSr = null;
            if (mapControl.Map.LayerCount > 0)
                mapSr = ((IGeoDataset)mapControl.Map.get_Layer(0)).SpatialReference;

            IRgbColor searchColor = new RgbColorClass { Red = 255, Green = 0, Blue = 0 };
            IRgbColor openColor = new RgbColorClass { Red = 0, Green = 0, Blue = 0 };

            var pointToName = new Dictionary<string, string>();
            var pointToIndex = new Dictionary<string, int>();
            var allTreasurePoints = _gameCoreManager.TreasureService.GetAllTreasurePoints();
            if (allTreasurePoints.Success)
            {
                foreach (var sp in allTreasurePoints.Data)
                {
                    foreach (var tid in GetTreasureIdsForPoint(sp.PointId))
                    {
                        var tr = _gameCoreManager.TreasureService.GetTreasure(tid);
                        if (tr.Success)
                        {
                            pointToName[sp.PointId] = tr.Data.TreasureName;
                            pointToIndex[sp.PointId] = int.Parse(tr.Data.TreasureId);
                        }
                    }
                }
            }

            for (int i = 0; i < AEUtil.SearchPointIds.Length; i++)
            {
                string pid = AEUtil.SearchPointIds[i];
                var r = _gameCoreManager.InteractionPointService.TryTriggerPoint(DemoPlayer1Id, pid);
                if (!r.Success) continue;
                var pt = r.Data.Position;

                string label = pointToName.ContainsKey(pid) ? pointToName[pid] : ("搜索" + (i + 1));
                int idx = pointToIndex.ContainsKey(pid) ? pointToIndex[pid] : (i + 1);

                PlaceTreasureMarker(graphicsContainer, mapSr,
                    pt.Latitude, pt.Longitude,
                    esriSimpleMarkerStyle.esriSMSDiamond, searchColor, label, idx);
            }

            for (int i = 0; i < AEUtil.OpenPointIds.Length; i++)
            {
                string pid = AEUtil.OpenPointIds[i];
                var r = _gameCoreManager.InteractionPointService.TryTriggerPoint(DemoPlayer1Id, pid);
                if (!r.Success) continue;
                var pt = r.Data.Position;

                string label = pointToName.ContainsKey(pid) ? pointToName[pid] : ("露天" + (i + 1));
                int idx = pointToIndex.ContainsKey(pid) ? pointToIndex[pid] : (20 + i);

                PlaceTreasureMarker(graphicsContainer, mapSr,
                    pt.Latitude, pt.Longitude,
                    esriSimpleMarkerStyle.esriSMSCircle, openColor, label, idx);
            }

            IActiveView activeView = mapControl.Map as IActiveView;
            if (activeView != null)
                activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        private IEnumerable<string> GetTreasureIdsForPoint(string pointId)
        {
            for (int id = 1; id <= 25; id++)
            {
                var tr = _gameCoreManager.TreasureService.GetTreasure(id.ToString());
                if (tr.Success && tr.Data.SpawnPointId == pointId)
                    yield return id.ToString();
            }
        }

        private void PlaceTreasureMarker(IGraphicsContainer gc, ISpatialReference mapSr,
            double lat, double lon, esriSimpleMarkerStyle style, IRgbColor color,
            string label, int treasureIndex)
        {
            if (_showTreasureMarkerAndText)
            {
                IElement pointElem = AEUtil.getPoint(color, lon, lat, style, treasureName);
                gc.AddElement(pointElem, 0);

                IRgbColor textColor = new RgbColorClass { Red = 0, Green = 0, Blue = 0 };
                IPoint labelPt = new PointClass { X = lon, Y = lat };

                IElement elementText = AEUtil.getTextLabel(textColor, labelPt, label, treasureName);
                gc.AddElement(elementText, 0);
            }

            if (_showTreasureImage)
            {
                IPoint labelPt = new PointClass { X = lon, Y = lat };
                string imgPath = System.IO.Path.Combine(Application.StartupPath, "Treasure", treasureIndex + ".png");
                if (System.IO.File.Exists(imgPath))
                {
                    try
                    {
                        IElement picElement = AEUtil.getPicElment(labelPt, imgPath, treasureName);
                        gc.AddElement(picElement, 0);
                    }
                    catch { }
                }
            }
        }

        public void RemoveTreasureFromMap(string pointId)
        {
            IGraphicsContainer gc = mapControl.Map as IGraphicsContainer;
            if (gc == null) return;

            string label = null;
            if (_gameCoreManager != null)
            {
                var tr = _gameCoreManager.TreasureService.GetTreasureByPointId(pointId);
                if (tr.Success)
                    label = tr.Data.TreasureName;
            }

            gc.Reset();
            var toDelete = new List<IElement>();
            IElement elem;
            while ((elem = gc.Next()) != null)
            {
                IElementProperties props = elem as IElementProperties;
                if (props == null) continue;
                if (props.Name == treasureName)
                {
                    ITextElement te = elem as ITextElement;
                    if (te != null && label != null && te.Text == label)
                    {
                        toDelete.Add(elem);
                    }
                    else if (te == null && label == null)
                    {
                        toDelete.Add(elem);
                    }
                }
            }

            if (toDelete.Count > 0)
            {
                IElement textElem = toDelete[0];
                IPoint textPt = textElem.Geometry as IPoint;

                gc.Reset();
                while ((elem = gc.Next()) != null)
                {
                    IElementProperties props = elem as IElementProperties;
                    if (props == null || props.Name != treasureName) continue;
                    if (toDelete.Contains(elem)) continue;

                    IPoint ePt = elem.Geometry as IPoint;
                    if (ePt != null && textPt != null &&
                        Math.Abs(ePt.X - textPt.X) < 1e-6 &&
                        Math.Abs(ePt.Y - textPt.Y) < 1e-6)
                    {
                        toDelete.Add(elem);
                    }
                }
            }

            foreach (var e2 in toDelete)
                gc.DeleteElement(e2);

            IActiveView activeView = mapControl.Map as IActiveView;
            if (activeView != null)
                activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        public void ShowTreasureImageOnMap(string pointId)
        {
            IGraphicsContainer gc = mapControl.Map as IGraphicsContainer;
            if (gc == null) return;

            if (_gameCoreManager == null) return;
            
            var pointResult = _gameCoreManager.InteractionPointService.TryTriggerPoint(DemoPlayer1Id, pointId);
            if (!pointResult.Success) return;
            
            var treasureResult = _gameCoreManager.TreasureService.GetTreasureByPointId(pointId);
            if (!treasureResult.Success) return;

            var treasure = treasureResult.Data;
            var point = pointResult.Data;
            
            IPoint labelPt = new PointClass { X = point.Position.Longitude, Y = point.Position.Latitude };
            
            string imgPath = System.IO.Path.Combine(Application.StartupPath, "Treasure", treasure.TreasureId + ".png");
            if (System.IO.File.Exists(imgPath))
            {
                try
                {
                    IElement picElement = AEUtil.getPicElment(labelPt, imgPath, treasureName);
                    gc.AddElement(picElement, 0);
                    
                    IActiveView activeView = mapControl.Map as IActiveView;
                    if (activeView != null)
                        activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                }
                catch { }
            }
        }

        
    }
}
