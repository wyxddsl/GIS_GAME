using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.GeoAnalyst;
using ESRI.ArcGIS.Analyst3D;
using GISGameFramework.Core;
using GISGameFramework.Core.Contracts;
using System;
using System.IO;
using SysPath = System.IO.Path;

namespace GISGameFramework.Game.ArcEngine
{
    /// <summary>
    /// 真实 ArcEngine 适配器。
    /// 光照采样：基于 DEM 用 IShadedReliefOp 重算 Hillshade，采样像素亮度值；
    /// 通视分析：ISurface.GetLineOfSight；可视域分析：ISurfaceOp2.Visibility。
    /// </summary>
    ///        用 ISurface.LineOfSight 做点对点通视分析（这个方法GIS的组件内部报错，用底层的方法从新做通视）；
    ///        用 IViewshedOp 做可视域分析。
    /// </summary>
    public class ArcEngineAdapter : IArcEngineAdapter
    {
        private readonly IMap _map;
        private readonly string _demTifPath;

        private IRaster _currentHillshadeRaster;


        public ArcEngineAdapter(IMap map, string demTifPath)
        {
            _map = map;
            _demTifPath = demTifPath;
        }

        public ArcEngineAdapter(IMap map, string demTifPath, string vectorHillshadePath)
        {
            _map = map;
            _demTifPath = demTifPath;
        }


        /// <summary>
        /// </summary>
        public ResponseResult<bool> UpdateHillshade(double altitude, double azimuth = 315.0, double zFactor = 1.0)
        {
            try
            {
                IRasterLayer demLayer = OpenDemAsRasterLayer();
                if (demLayer == null)
                    return ResponseFactory.Fail<bool>(ErrorCodes.NotFound, "无法打开 DEM 文件：" + _demTifPath);

                ISurfaceOp surfaceOp = new RasterSurfaceOpClass() as ISurfaceOp;

                IRasterAnalysisEnvironment env = surfaceOp as IRasterAnalysisEnvironment;
                if (env != null)
                    env.OutSpatialReference = ((IGeoDataset)demLayer.Raster).SpatialReference;

                object zFactorObj = (object)zFactor;
                IGeoDataset newHillshadeDs = surfaceOp.HillShade(
                    (IGeoDataset)demLayer.Raster,
                    azimuth,
                    altitude,
                    false,      // modelShadows
                    ref zFactorObj);

                _currentHillshadeRaster = (IRaster)newHillshadeDs;

                //// 更新地图中已有的 hillshade 图层（如果存在）
                //IRasterLayer existingLayer = FindLayerByKeyword("hillshadeAll") as IRasterLayer;
                //if (existingLayer != null)
                //{
                //    existingLayer.CreateFromRaster(_currentHillshadeRaster);
                //    IActiveView av = _map as IActiveView;
                //    if (av != null)
                //        av.PartialRefresh(esriViewDrawPhase.esriViewGeography, existingLayer, null);
                //}
                //else
                {
                    //  直接加载，不做判断
                    string outPath = SaveRasterToTiff(_currentHillshadeRaster,
                        SysPath.GetDirectoryName(_demTifPath), "hillshade_current.tif");

                    if (outPath != null)
                    {
                        IRasterLayer newLayer = LoadRasterLayerFromFile(outPath);
                        if (newLayer != null)
                        {
                            newLayer.Name = "hillshadeAll";
                            _map.AddLayer(newLayer);
                            _map.MoveLayer(newLayer, _map.LayerCount - 1);

                            IActiveView av = _map as IActiveView;
                            if (av != null)
                                av.PartialRefresh(esriViewDrawPhase.esriViewGeography, newLayer, null);
                        }
                    }
                }

                return ResponseFactory.Ok(true,
                    string.Format("Hillshade 重算完成（高度角:{0:F1}° 方位角:{1:F1}°）", altitude, azimuth));
            }
            catch (Exception ex)
            {
                return ResponseFactory.Fail<bool>(ErrorCodes.InternalError, "UpdateHillshade 异常：" + ex.Message);
            }
        }
        /// <summary>
        /// 光照采样：仅从 DEM 重算的 Hillshade 栅格采样。
        /// </summary>
        public ResponseResult<int> EvaluateLightCondition(GeoPosition position)
        {
            if (_currentHillshadeRaster == null)
                return ResponseFactory.Fail<int>(ErrorCodes.InvalidState, "Hillshade 栅格尚未就绪，请先调用 UpdateHillshade。");
            return SampleRaster(_currentHillshadeRaster, position);
        }
        //gis的可视域方法有问题
        public ResponseResult<bool> CheckLineOfSight(GeoPosition observer, GeoPosition target)
        {
            
            //IRasterLayer demLayer = null;
            //IRasterSurface rs = null;
            try
            {
                //demLayer = OpenDemAsRasterLayer();
                //if (demLayer == null) return ResponseFactory.Ok(true, "无 DEM");

                //IGeoDataset demDs = (IGeoDataset)demLayer.Raster;

                //IPoint fromPt = ProjectToSr(observer, demDs.SpatialReference);
                //IPoint toPt = ProjectToSr(target, demDs.SpatialReference);

                //((IZAware)fromPt).ZAware = true;
                //((IZAware)toPt).ZAware = true;

                //if (!PointInExtent(fromPt, demDs.Extent) || !PointInExtent(toPt, demDs.Extent))
                //    return ResponseFactory.Ok(true, "超出 DEM 范围");

                //rs = new RasterSurfaceClass();
                //rs.PutRaster(demLayer.Raster, 0);
                //ISurface surface = (ISurface)rs;

                //bool isVisible = false;
                //IPoint obstructionPt;
                //IPolyline vLine, invLine;
                //object refactor = 0.13;

                //surface.GetLineOfSight(fromPt, toPt, out obstructionPt, out vLine, out invLine,
                //                       out isVisible, false, false, ref refactor);
                bool isVisible = CheckVisibilityStepMethod(observer, target).Success;
                return ResponseFactory.Ok(isVisible, isVisible ? "通视" : "被遮挡");
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                return ResponseFactory.Fail<bool>(ErrorCodes.None, string.Format("GIS组件错误(0x{0}): {1}", ex.ErrorCode, ex.Message));
                //// 必须清理，否则下次调用可能因为资源占用返回 E_FAIL
                //if (rs != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(rs);
                //if (demLayer != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(demLayer);
            }
        }

        private static bool PointInExtent(IPoint pt, IEnvelope extent)
        {
            return pt.X >= extent.XMin && pt.X <= extent.XMax
                && pt.Y >= extent.YMin && pt.Y <= extent.YMax;
        }
        /// <summary>
        /// 使用步进采样法进行通视分析
        /// <returns>是否通视</returns>
    public ResponseResult<bool> CheckVisibilityStepMethod(GeoPosition observer, GeoPosition target, double observerOffset = ConUtil.ObserverHeightOffset, double targetOffset = ConUtil.TargetHeightOffset)
    {
        try
        {
            IRasterLayer demLayer = OpenDemAsRasterLayer();
            if (demLayer == null) return ResponseFactory.Ok(true, "无 DEM，默认通视。");

            IRaster raster = demLayer.Raster;
            IRasterProps rasterProps = (IRasterProps)raster;
            ISpatialReference demSr = ((IGeoDataset)raster).SpatialReference;

            IPoint fromPt = ProjectToSr(observer, demSr);
            IPoint toPt = ProjectToSr(target, demSr);

            fromPt.Z = (double.IsNaN(fromPt.Z) ? GetElevation(raster, fromPt.X, fromPt.Y) : fromPt.Z) + observerOffset;
            toPt.Z = (double.IsNaN(toPt.Z) ? GetElevation(raster, toPt.X, toPt.Y) : toPt.Z) + targetOffset;

            double dx = toPt.X - fromPt.X;
            double dy = toPt.Y - fromPt.Y;
            double dz = toPt.Z - fromPt.Z;
            double horizontalDistance = Math.Sqrt(dx * dx + dy * dy);

            double stepSize = (rasterProps.MeanCellSize().X + rasterProps.MeanCellSize().Y) / 2.0;
            int stepCount = (int)Math.Ceiling(horizontalDistance / stepSize);
            
            if (stepCount < 2) stepCount = 2; // 至少采样两端

            for (int i = 1; i < stepCount; i++)
            {
                double ratio = (double)i / stepCount;

                double currentX = fromPt.X + dx * ratio;
                double currentY = fromPt.Y + dy * ratio;
                double lineZ = fromPt.Z + dz * ratio; // 视线在此处的高度
                double groundZ = GetElevation(raster, currentX, currentY);
                if (!double.IsNaN(groundZ) && groundZ > lineZ)
                {
                    return ResponseFactory.Ok(false, string.Format("被遮挡：在距离起点 {0} 米处高度为 {Math.Round(groundZ, 2)} 米", Math.Round(ratio * horizontalDistance, 2)));
                }
            }

            return ResponseFactory.Ok(true, "通视");
        }
        catch (Exception ex)
        {
            return ResponseFactory.Fail<bool>(ErrorCodes.None,"步进分析异常：" + ex.Message);
        }
    }

    /// <summary>
    /// 获取指定坐标在栅格上的高度值
    /// </summary>
    private double GetElevation(IRaster raster, double x, double y)
    {
        try
        {
            int col, row;
            IRaster2 raster2 = (IRaster2)raster;
            raster2.MapToPixel(x, y, out col, out row);
        
            object val = raster2.GetPixelValue(0, col, row);
            if (val == null || val is DBNull) return double.NaN;

            return Convert.ToDouble(val);
        }
        catch
        {
            return double.NaN; // 超出范围或无数据
        }
    }
        /// <summary>
        /// 可视域分析
        /// </summary>
        public ResponseResult<bool> ComputeViewshed(GeoPosition observer, GeoPosition target)
        {
            try
            {
                IRasterLayer demLayer = OpenDemAsRasterLayer();
                if (demLayer == null)
                    return ResponseFactory.Ok(true, "无 DEM，默认可见。");

                ISpatialReference demSr = ((IGeoDataset)demLayer.Raster).SpatialReference;

                ISurfaceOp2 surfaceOp2 = new RasterSurfaceOpClass() as ISurfaceOp2;

                IFeatureClass observerFc = CreateInMemoryPointFc(demSr);
                IFeature feature = observerFc.CreateFeature();
                feature.Shape = ProjectToSr(observer, demSr);
                feature.Store();

                object outFrequency = Type.Missing;
                object outObserverCount = Type.Missing;
                IGeoDataset viewshedDs = surfaceOp2.Visibility(
                    (IGeoDataset)demLayer.Raster,
                    (IGeoDataset)observerFc,
                    esriGeoAnalysisVisibilityEnum.esriGeoAnalysisVisibilityFrequency,
                    ref outFrequency,
                    ref outObserverCount);

                IRaster viewshedRaster = (IRaster)viewshedDs;

                IPoint targetPt = ProjectToSr(target, demSr);
                IRasterCursor cursor = GetRasterCursorAtPoint(viewshedRaster, targetPt);
                bool visible = SampleViewshedValue(viewshedRaster, targetPt);
               
                return ResponseFactory.Ok(visible, visible ? "在可视域内" : "不在可视域内");
            }
            catch (Exception ex)
            {
                return ResponseFactory.Ok(true, "Viewshed 异常（默认可见）：" + ex.Message);
            }
        }

        public ResponseResult<GameArea> QueryArea(GeoPosition position)
        {
            return ResponseFactory.NotImplemented<GameArea>("ArcEngineAdapter", "QueryArea");
        }

        private IRasterLayer OpenDemAsRasterLayer()
        {
            if (string.IsNullOrEmpty(_demTifPath) || !File.Exists(_demTifPath))
                return null;
            try
            {
                string folder = SysPath.GetDirectoryName(_demTifPath);
                string fileName = SysPath.GetFileName(_demTifPath);
                IWorkspaceFactory wsf = new RasterWorkspaceFactoryClass();
                IRasterWorkspace rws = (IRasterWorkspace)wsf.OpenFromFile(folder, 0);
                IRasterDataset rds = rws.OpenRasterDataset(fileName);
                IRasterLayer rl = new RasterLayerClass();
                rl.CreateFromDataset(rds);
                return rl;
            }
            catch { return null; }
        }

        /// <summary>将 WGS84 GeoPosition 投影到目标坐标系的 IPoint。</summary>
        private static IPoint ProjectToSr(GeoPosition pos, ISpatialReference targetSr)
        {
            IPoint pt = new PointClass { X = pos.Longitude, Y = pos.Latitude };
            ISpatialReference wgs84 = new SpatialReferenceEnvironmentClass()
                .CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
            pt.SpatialReference = wgs84;
            if (targetSr != null)
                pt.Project(targetSr);
            return pt;
        }

        /// <summary>归一化到 0-100。</summary>
        private static ResponseResult<int> SampleRasterLayer(IRasterLayer layer, GeoPosition pos)
        {
            try
            {
                IPoint pt = new PointClass { X = pos.Longitude, Y = pos.Latitude };
                ISpatialReference wgs84 = new SpatialReferenceEnvironmentClass()
                    .CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
                pt.SpatialReference = wgs84;
                var rasterSr = ((IGeoDataset)layer.Raster).SpatialReference;
                if (rasterSr != null) pt.Project(rasterSr);

                IIdentify identify = layer as IIdentify;
                if (identify == null)
                    return ResponseFactory.Fail<int>(ErrorCodes.InternalError, "图层不支持 Identify。");

                IArray results = identify.Identify(pt);
                if (results == null || results.Count == 0)
                    return ResponseFactory.Fail<int>(ErrorCodes.NotFound, "采样点无栅格值。");

                IRasterIdentifyObj rasterObj = results.get_Element(0) as IRasterIdentifyObj;
                if (rasterObj == null)
                    return ResponseFactory.Fail<int>(ErrorCodes.InternalError, "无法读取栅格值。");

                double pixelValue;
                if (!double.TryParse(rasterObj.MapTip, out pixelValue))
                    return ResponseFactory.Fail<int>(ErrorCodes.InternalError, "栅格值解析失败：" + rasterObj.MapTip);

                int lightLevel = (int)(pixelValue / 255.0 * 100.0);
                return ResponseFactory.Ok(lightLevel,
                    string.Format("栅格光照采样值：{0}（像素：{1:F0}）", lightLevel, pixelValue));
            }
            catch (Exception ex)
            {
                return ResponseFactory.Fail<int>(ErrorCodes.InternalError, "SampleRasterLayer 异常：" + ex.Message);
            }
        }

        private static ResponseResult<int> SampleRaster(IRaster raster, GeoPosition pos)
        {
            try
            {
                IRasterLayer tempLayer = new RasterLayerClass();
                tempLayer.CreateFromRaster(raster);
                return SampleRasterLayer(tempLayer, pos);
            }
            catch (Exception ex)
            {
                return ResponseFactory.Fail<int>(ErrorCodes.InternalError, "SampleRaster 异常：" + ex.Message);
            }
        }

        private ILayer FindLayerByKeyword(string keyword)
        {
            for (int i = 0; i < _map.LayerCount; i++)
            {
                ILayer layer = _map.get_Layer(i);
                if (layer != null && layer.Name.ToLower().Contains(keyword.ToLower()))
                    return layer;
            }
            return null;
        }

        private static IFeatureClass CreateInMemoryPointFc(ISpatialReference sr)
        {
            IWorkspaceFactory wsf = new InMemoryWorkspaceFactoryClass();
            IWorkspaceName wsn = wsf.Create("", "TempWs", null, 0);
            IName nm = wsn as IName;
            IWorkspace ws = nm.Open() as IWorkspace;
            IFeatureWorkspace fws = ws as IFeatureWorkspace;

            IGeometryDef geomDef = new GeometryDefClass();
            IGeometryDefEdit geomDefEdit = geomDef as IGeometryDefEdit;
            geomDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;
            geomDefEdit.SpatialReference_2 = sr;

            IFields fields = new FieldsClass();
            IFieldsEdit fe = fields as IFieldsEdit;
            IField oidField = new FieldClass();
            IFieldEdit oidEdit = oidField as IFieldEdit;
            oidEdit.Name_2 = "OBJECTID";
            oidEdit.Type_2 = esriFieldType.esriFieldTypeOID;
            fe.AddField(oidField);

            IField shpField = new FieldClass();
            IFieldEdit shpEdit = shpField as IFieldEdit;
            shpEdit.Name_2 = "Shape";
            shpEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            shpEdit.GeometryDef_2 = geomDef;
            fe.AddField(shpField);

            return fws.CreateFeatureClass("ObserverPts", fields, null, null,
                esriFeatureType.esriFTSimple, "Shape", "");
        }

        private static bool SampleViewshedValue(IRaster raster, IPoint pt)
        {
            try
            {
                IRasterLayer tempLayer = new RasterLayerClass();
                tempLayer.CreateFromRaster(raster);
                IIdentify identify = tempLayer as IIdentify;
                if (identify == null) return true;

                IArray results = identify.Identify(pt);
                if (results == null || results.Count == 0) return true;

                IRasterIdentifyObj obj = results.get_Element(0) as IRasterIdentifyObj;
                if (obj == null) return true;

                double val;
                return double.TryParse(obj.MapTip, out val) && val > 0;
            }
            catch { return true; }
        }

        private static IRasterCursor GetRasterCursorAtPoint(IRaster raster, IPoint pt) { return null; }

        
        /// <summary>
        /// 把内存栅格保存为 GeoTIFF，返回完整路径；失败返回 null。
        /// </summary>
        private static string SaveRasterToTiff(IRaster raster, string folder, string fileName)
        {
            try
            {
                string outPath = SysPath.Combine(folder, fileName);

                if (File.Exists(outPath))
                    File.Delete(outPath);

                IWorkspaceFactory wsf = new RasterWorkspaceFactoryClass();
                IRasterWorkspace2 rws = (IRasterWorkspace2)wsf.OpenFromFile(folder, 0);

                ISaveAs2 saveAs = (ISaveAs2)raster;
                saveAs.SaveAs(fileName, (IWorkspace)rws, "TIFF");

                return outPath;
            }
            catch { return null; }
        }

        /// <summary>
        /// 从本地文件路径加载 IRasterLayer。
        /// </summary>
        private static IRasterLayer LoadRasterLayerFromFile(string fullPath)
        {
            try
            {
                string folder = SysPath.GetDirectoryName(fullPath);
                string fileName = SysPath.GetFileName(fullPath);

                IWorkspaceFactory wsf = new RasterWorkspaceFactoryClass();
                IRasterWorkspace rws = (IRasterWorkspace)wsf.OpenFromFile(folder, 0);
                IRasterDataset rds = rws.OpenRasterDataset(fileName);

                IRasterLayer rl = new RasterLayerClass();
                rl.CreateFromDataset(rds);
                return rl;
            }
            catch { return null; }
        }
    }
}
