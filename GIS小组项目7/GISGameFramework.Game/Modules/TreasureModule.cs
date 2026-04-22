using System;
using System.Collections.Generic;
using System.Linq;
using GISGameFramework.Core;
using GISGameFramework.Core.Contracts;
using GISGameFramework.Game.ArcEngine;

namespace GISGameFramework.Game.Modules
{
    public class TreasureModule : ITreasureService
    {
        private static readonly double OpenTreasureVisibleDistance = ArcEngine.ConUtil.OpenTreaDistance;
        private const string InitialTreasureId = "26";
        private const string ReservedTreasureId = "27";

        private readonly Dictionary<string, TreasureInfo> _treasures = new Dictionary<string, TreasureInfo>();
        private readonly Dictionary<string, List<TreasureAlbumEntry>> _albums = new Dictionary<string, List<TreasureAlbumEntry>>();

        // 记录所有宝藏点位，供最邻近分析使用
        private readonly List<InteractionPoint> _allTreasurePoints = new List<InteractionPoint>();

        public ResponseResult<bool> Initialize()
        {
            return ResponseFactory.Ok(true, "Treasure module initialized.");
        }

        public ResponseResult<bool> Shutdown()
        {
            return ResponseFactory.Ok(true, "Treasure module closed.");
        }

        public ResponseResult<bool> AddTreasure(TreasureInfo treasure)
        {
            if (treasure == null || string.IsNullOrWhiteSpace(treasure.TreasureId))
            {
                return ResponseFactory.Fail<bool>(ErrorCodes.InvalidArgument, "Treasure argument is invalid.");
            }

            _treasures[treasure.TreasureId] = treasure;
            return ResponseFactory.Ok(true, "Treasure registered.");
        }

        public ResponseResult<TreasureInfo> GetTreasure(string treasureId)
        {
            TreasureInfo treasure;
            if (_treasures.TryGetValue(treasureId, out treasure))
            {
                return ResponseFactory.Ok(treasure, "Treasure loaded.");
            }

            return ResponseFactory.Fail<TreasureInfo>(ErrorCodes.NotFound, "Treasure not found.");
        }

        /// <summary>
        /// 重置所有宝藏的收集状态，用于重新布设宝物。
        /// </summary>
        public void ResetAllTreasures()
        {
            foreach (var treasure in _treasures.Values)
            {
                treasure.IsCollected = false;
            }
            // 清空图册（重新布设后图册也重置）
            _albums.Clear();
            // 清空点位记录，等待重新分配
            _allTreasurePoints.Clear();
        }

        /// <summary>
        /// 随机将搜索宝藏(1-19)和露天宝藏(20-25)分配到固定点位。
        /// 每次加载游戏调用一次，保证每局宝藏位置不同。
        /// </summary>
        public ResponseResult<bool> RandomAllocateTreasures(IList<InteractionPoint> searchPoints, IList<InteractionPoint> openTreasurePoints)
        {
            if (searchPoints == null || openTreasurePoints == null)
            {
                return ResponseFactory.Fail<bool>(ErrorCodes.InvalidArgument, "Points collection cannot be null.");
            }

            var random = new Random();

            // 搜索宝藏：TreasureType.Search，排除初始宝藏26
            var searchTreasures = _treasures.Values
                .Where(t => t.TreasureType == TreasureType.Search && t.TreasureId != InitialTreasureId)
                .ToList();

            // 露天宝藏：TreasureType.Open，排除特殊宝藏26和27
            var openTreasures = _treasures.Values
                .Where(t => t.TreasureType == TreasureType.Open
                         && t.TreasureId != InitialTreasureId
                         && t.TreasureId != ReservedTreasureId)
                .ToList();

            if (searchTreasures.Count != searchPoints.Count)
            {
                return ResponseFactory.Fail<bool>(ErrorCodes.InvalidArgument,
                    string.Format("搜索宝藏数量({0})与搜索点位数量({1})不匹配。", searchTreasures.Count, searchPoints.Count));
            }

            if (openTreasures.Count != openTreasurePoints.Count)
            {
                return ResponseFactory.Fail<bool>(ErrorCodes.InvalidArgument,
                    string.Format("露天宝藏数量({0})与露天点位数量({1})不匹配。", openTreasures.Count, openTreasurePoints.Count));
            }

            AEUtil.Shuffle(searchTreasures, random);
            AEUtil.Shuffle(openTreasures, random);

            for (int i = 0; i < searchTreasures.Count; i++)
            {
                var point = searchPoints[i];
                searchTreasures[i].SpawnPointId = point.PointId;
                searchTreasures[i].Position = point.Position;
            }

            for (int i = 0; i < openTreasures.Count; i++)
            {
                var point = openTreasurePoints[i];
                openTreasures[i].SpawnPointId = point.PointId;
                openTreasures[i].Position = point.Position;
            }

            // 记录所有宝藏点位（搜索 + 露天），供最邻近分析使用
            _allTreasurePoints.Clear();
            _allTreasurePoints.AddRange(searchPoints);
            _allTreasurePoints.AddRange(openTreasurePoints);

            return ResponseFactory.Ok(true, "宝藏随机分配完成。");
        }

        /// <summary>
        /// 根据点位ID查找绑定的宝藏（通过 SpawnPointId 匹配）。
        /// </summary>
        public ResponseResult<TreasureInfo> GetTreasureByPointId(string pointId)
        {
            foreach (var treasure in _treasures.Values)
            {
                if (treasure.SpawnPointId == pointId)
                    return ResponseFactory.Ok(treasure, "Treasure found by point.");
            }
            return ResponseFactory.Fail<TreasureInfo>(ErrorCodes.NotFound, "No treasure bound to point: " + pointId);
        }

        /// <summary>
        /// 获取所有宝藏点位，供 SkillModule 做最邻近分析。
        /// </summary>
        public ResponseResult<IList<InteractionPoint>> GetAllTreasurePoints()
        {
            return ResponseFactory.Ok((IList<InteractionPoint>)_allTreasurePoints, "All treasure points returned.");
        }

        /// <summary>
        /// 奖励初始宝藏（ID:26），游戏开局时自动调用一次。
        /// </summary>
        public ResponseResult<TreasureDiscoveryResult> AwardInitialTreasure(string playerId)
        {
            TreasureInfo treasure;
            if (!_treasures.TryGetValue(InitialTreasureId, out treasure))
            {
                return ResponseFactory.Fail<TreasureDiscoveryResult>(ErrorCodes.NotFound, "初始宝藏(26)未注册。");
            }

            if (treasure.IsCollected)
            {
                return ResponseFactory.Fail<TreasureDiscoveryResult>(ErrorCodes.InvalidState, "初始宝藏已领取。");
            }

            treasure.IsCollected = true;
            EnsureAlbum(playerId).Add(new TreasureAlbumEntry
            {
                TreasureId = treasure.TreasureId,
                TreasureName = treasure.TreasureName,
                ImagePath = treasure.IconPath,
                CollectedAt = DateTime.UtcNow,
                IsUnlocked = true
            });

            return ResponseFactory.Ok(new TreasureDiscoveryResult
            {
                Treasure = treasure,
                ScoreAwarded = treasure.ScoreValue,
                AddedToAlbum = true,
                Detail = "开局获得初始宝藏。"
            }, "Initial treasure awarded.");
        }

        public ResponseResult<TreasureDiscoveryResult> SearchTreasure(string playerId, string pointId)
        {
            foreach (var treasure in _treasures.Values)
            {
                if (treasure.DiscoveryMode == TreasureDiscoveryMode.SearchAtPoint && treasure.SpawnPointId == pointId)
                {
                    if (treasure.IsCollected)
                    {
                        return ResponseFactory.Fail<TreasureDiscoveryResult>(ErrorCodes.InvalidState, "Treasure already collected.");
                    }

                    treasure.IsCollected = true;
                    EnsureAlbum(playerId).Add(new TreasureAlbumEntry
                    {
                        TreasureId = treasure.TreasureId,
                        TreasureName = treasure.TreasureName,
                        ImagePath = treasure.IconPath,
                        CollectedAt = DateTime.UtcNow,
                        IsUnlocked = true
                    });

                    return ResponseFactory.Ok(new TreasureDiscoveryResult
                    {
                        Treasure = treasure,
                        ScoreAwarded = treasure.ScoreValue,
                        AddedToAlbum = true,
                        Detail = "Search treasure collected."
                    }, "Search treasure collected.");
                }
            }

            return ResponseFactory.Fail<TreasureDiscoveryResult>(ErrorCodes.NotFound, "No treasure is bound to this search point.");
        }

        public ResponseResult<VisibilityCheckResult> CheckOpenTreasureVisibility(string playerId, string treasureId, VisibilityCheckResult environmentVisibility)
        {
            TreasureInfo treasure;
            if (!_treasures.TryGetValue(treasureId, out treasure))
            {
                return ResponseFactory.Fail<VisibilityCheckResult>(ErrorCodes.NotFound, "Treasure not found.");
            }

            if (treasure.IsCollected)
            {
                return ResponseFactory.Fail<VisibilityCheckResult>(ErrorCodes.InvalidState, "Treasure already collected.");
            }

            var result = environmentVisibility ?? new VisibilityCheckResult();
            result.MeetsDistanceRequirement = result.Distance <= OpenTreasureVisibleDistance;
            result.IsVisible = result.HasLineOfSight && result.MeetsLightRequirement && result.MeetsDistanceRequirement;
            result.DetailMessage = result.IsVisible
                ? "Open treasure is visible."
                : string.Format("不可见：距离{0}，光照{1}，通视{2}。",
                    result.MeetsDistanceRequirement ? "是" : "否",
                    result.MeetsLightRequirement ? "是" : "否",
                    result.HasLineOfSight ? "是" : "否");

            return ResponseFactory.Ok(result, "Open treasure visibility checked.");
        }

        public ResponseResult<TreasureDiscoveryResult> CollectOpenTreasure(string playerId, string treasureId)
        {
            TreasureInfo treasure;
            if (!_treasures.TryGetValue(treasureId, out treasure))
            {
                return ResponseFactory.Fail<TreasureDiscoveryResult>(ErrorCodes.NotFound, "Treasure not found.");
            }

            if (treasure.IsCollected)
            {
                return ResponseFactory.Fail<TreasureDiscoveryResult>(ErrorCodes.InvalidState, "Treasure already collected.");
            }

            treasure.IsCollected = true;
            EnsureAlbum(playerId).Add(new TreasureAlbumEntry
            {
                TreasureId = treasure.TreasureId,
                TreasureName = treasure.TreasureName,
                ImagePath = treasure.IconPath,
                CollectedAt = DateTime.UtcNow,
                IsUnlocked = true
            });

            return ResponseFactory.Ok(new TreasureDiscoveryResult
            {
                Treasure = treasure,
                ScoreAwarded = treasure.ScoreValue,
                AddedToAlbum = true,
                Detail = "Open treasure collected."
            }, "Open treasure collected.");
        }

        public ResponseResult<IList<TreasureAlbumEntry>> GetAlbum(string playerId)
        {
            return ResponseFactory.Ok((IList<TreasureAlbumEntry>)EnsureAlbum(playerId), "Album loaded.");
        }

        private List<TreasureAlbumEntry> EnsureAlbum(string playerId)
        {
            List<TreasureAlbumEntry> album;
            if (!_albums.TryGetValue(playerId, out album))
            {
                album = new List<TreasureAlbumEntry>();
                _albums[playerId] = album;
            }
            return album;
        }
        
    }
}
