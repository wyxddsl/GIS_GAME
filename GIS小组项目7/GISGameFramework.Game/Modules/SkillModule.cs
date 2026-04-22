using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Geometry;
using GISGameFramework.Core;
using GISGameFramework.Core.Contracts;
using GISGameFramework.Game.ArcEngine;

namespace GISGameFramework.Game.Modules
{
    /// <summary>
    /// 技能模块：最邻近分析技能。
    /// </summary>
    public class SkillModule : ISkillService
    {
        private readonly IPlayerService _playerService;
        private readonly ITreasureService _treasureService;

        public SkillModule(IPlayerService playerService, ITreasureService treasureService)
        {
            _playerService = playerService;
            _treasureService = treasureService;
        }

        public ResponseResult<bool> Initialize()
        {
            return ResponseFactory.Ok(true, "技能初始化");
        }

        public ResponseResult<bool> Shutdown()
        {
            return ResponseFactory.Ok(true, "技能关闭");
        }

        public ResponseResult<SkillUsageResult> UseNearestTreasureHint(string playerId)
        {
            var playerResult = _playerService.GetPlayer(playerId);
            if (!playerResult.Success)
                return ResponseFactory.Fail<SkillUsageResult>(ErrorCodes.NotFound, "未发现玩家");

            var pointsResult = _treasureService.GetAllTreasurePoints();
            if (!pointsResult.Success || pointsResult.Data == null || pointsResult.Data.Count == 0)
                return ResponseFactory.Fail<SkillUsageResult>(ErrorCodes.NotFound, "暂无可用藏宝点位。");

            // 将玩家位置和所有藏宝点转为 IPoint，交给 AEUtil.GetNearestPointInfo 计算
            IPoint originPoint = AEUtil.ConvertGeoToPoint(playerResult.Data.CurrentPosition);
            var targetPoints = new List<IPoint>();
            var pointList = new List<InteractionPoint>(pointsResult.Data);
            foreach (var p in pointList)
                targetPoints.Add(AEUtil.ConvertGeoToPoint(p.Position));

            double nearestDistance, angleDegrees, azimuth;
            IPoint nearestIPoint = AEUtil.GetNearestPointInfo(
                originPoint, targetPoints,
                out nearestDistance, out angleDegrees, out azimuth);

            if (nearestIPoint == null)
                return ResponseFactory.Fail<SkillUsageResult>(ErrorCodes.NotFound, "未找到最近藏宝点。");

            double nearestDistanceMeters = nearestDistance * AEUtil.duZhuanMi;

            int nearestIdx = targetPoints.IndexOf(nearestIPoint);
            InteractionPoint nearest = pointList[nearestIdx];

            string directionText = AzimuthToDirectionText(azimuth);

            string treasureName = nearest.DisplayName ?? nearest.PointId;
            var trResult = _treasureService.GetTreasureByPointId(nearest.PointId);
            if (trResult.Success)
                treasureName = trResult.Data.TreasureName;

            return ResponseFactory.Ok(new SkillUsageResult
            {
                Success = true,
                SkillId = "nearest_treasure_hint",
                NearestTreasureHint = new NavigationHint
                {
                    TargetId = nearest.PointId,
                    Distance = Math.Round(nearestDistanceMeters, 1),
                    DirectionText = directionText
                },
                Message = string.Format("最近藏宝点：{0}，距离 {1:F1}m，方向：{2}。",
                    treasureName, nearestDistanceMeters, directionText)
            }, "最近的宝藏");
        }

        /// <summary>
        /// 将方位角（0=正北，顺时针，与 AEUtil.GetNearestPointInfo 输出的 azimuth 一致）
        /// </summary>
        private static string AzimuthToDirectionText(double azimuth)
        {
            // 归一化到 [0, 360)
            azimuth = ((azimuth % 360.0) + 360.0) % 360.0;

            if (azimuth < 22.5 || azimuth >= 337.5) return "正北";
            if (azimuth < 67.5)  return "东北";
            if (azimuth < 112.5) return "正东";
            if (azimuth < 157.5) return "东南";
            if (azimuth < 202.5) return "正南";
            if (azimuth < 247.5) return "西南";
            if (azimuth < 292.5) return "正西";
            return "西北";
        }
    }
}
