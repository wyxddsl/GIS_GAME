namespace GISGameFramework.Game.ArcEngine
{
    public static class ConUtil
    {
        // 宝藏触发半径
        public const double SearchTreaRadius = 15.0;
        public const double OpenTreaDistance = 50.0;
        // 光照
        public const int OpenTreaLightLevel = 40;
        public const int DefaultLightLevel = 50;
        public const int ShadowThreshold = 30;

        // 通视分析
        public const double ObserverHeightOffset = 2.0;
        public const double TargetHeightOffset = 0.0;

        // 附近点位查询半径
        public const double NearbyPointScanRadius = 100.0;
        public const double ManualSearchRadius = 20.0;
        //
        public const int StepIntervalMs = 500;
    }
}
