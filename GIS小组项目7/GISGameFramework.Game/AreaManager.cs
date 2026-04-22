//2352025 杨麟烨
using System.Collections.Generic;

namespace GISGameFramework.Game
{
    public static class AreaManager
    {
        public static List<AreaData> AllAreas = new List<AreaData>();

        static AreaManager()
        {
            AllAreas.Add(new AreaData(1, "正门", 121.500375, 31.285095));
            AllAreas.Add(new AreaData(2, "衷和楼", 121.501874, 31.286174));
            AllAreas.Add(new AreaData(3, "三好坞", 121.499407, 31.287324));
            AllAreas.Add(new AreaData(4, "北苑", 121.495854, 31.288347));
            AllAreas.Add(new AreaData(5, "校区西侧", 121.495108, 31.286817));
            AllAreas.Add(new AreaData(6, "西苑", 121.493887, 31.284612));
            AllAreas.Add(new AreaData(7, "校医院", 121.496258, 31.283366));
            AllAreas.Add(new AreaData(8, "体育场", 121.499336, 31.283065));
            AllAreas.Add(new AreaData(9, "樱花大道", 121.496972, 31.284746));
            AllAreas.Add(new AreaData(10, "瑞安楼", 121.497845, 31.286222));
        }
    }
}