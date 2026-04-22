//2352025 杨麟烨
using System;

namespace GISGameFramework.Game
{
    public class AreaData
    {
        public int AreaId { get; set; }
        public string AreaName { get; set; }
        public double CenterLon { get; set; }
        public double CenterLat { get; set; }
        public int OccupyStatus { get; set; }
        public double QuizRadius { get; set; }
        public string QuizIds { get; set; }
        public string TrPointIds { get; set; }
        public int OccupyUid { get; set; }
        public DateTime? OccupyTime { get; set; }

        public AreaData(int areaId, string areaName, double centerLon, double centerLat)
        {
            AreaId = areaId;
            AreaName = areaName;
            CenterLon = centerLon;
            CenterLat = centerLat;
            OccupyStatus = 0;
            QuizRadius = 0;
            QuizIds = string.Empty;
            TrPointIds = string.Empty;
            OccupyUid = 0;
            OccupyTime = null;
        }
    }
}