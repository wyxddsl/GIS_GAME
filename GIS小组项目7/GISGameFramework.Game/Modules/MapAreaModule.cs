using System.Collections.Generic;
using GISGameFramework.Core;
using GISGameFramework.Core.Contracts;

namespace GISGameFramework.Game.Modules
{
    public class MapAreaModule : IMapAreaService
    {
        // 先用内存集合占位，后续可以替换成图层或要素类查询结果。
        private readonly Dictionary<string, GameArea> _areas = new Dictionary<string, GameArea>();

        public ResponseResult<bool> AddArea(GameArea area)
        {
            if (area == null || string.IsNullOrWhiteSpace(area.AreaId))
            {
                return ResponseFactory.Fail<bool>(ErrorCodes.InvalidArgument, "Area argument is invalid.");
            }

            _areas[area.AreaId] = area;
            return ResponseFactory.Ok(true, "Area registered.");
        }

        public ResponseResult<IList<GameArea>> GetAllAreas()
        {
            return ResponseFactory.Ok((IList<GameArea>)new List<GameArea>(_areas.Values), "Area list loaded.");
        }

        public ResponseResult<bool> Initialize()
        {
            return ResponseFactory.Ok(true, "Map area module initialized.");
        }

        public ResponseResult<GameArea> QueryArea(GeoPosition position)
        {
            if (_areas.Count == 0)
            {
                return ResponseFactory.NotImplemented<GameArea>("MapAreaModule", "QueryArea");
            }

            // 真实 GIS 版本通常会在这里做点落面、缓冲区、叠加分析等判断。
            foreach (var area in _areas.Values)
            {
                return ResponseFactory.Ok(area, "Placeholder area returned.");
            }

            return ResponseFactory.Fail<GameArea>(ErrorCodes.NotFound, "Area not found.");
        }

        public ResponseResult<bool> Shutdown()
        {
            return ResponseFactory.Ok(true, "Map area module closed.");
        }
    }
}
