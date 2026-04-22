using GISGameFramework.Core;
using GISGameFramework.Core.Contracts;
using GISGameFramework.Game.ArcEngine;

namespace GISGameFramework.Game.Modules
{
    public class EnvironmentModule : IEnvironmentService
    {
        // ArcEngine 还没接入时，先用可替换的简化规则保证主流程能演示。
        // 后续接入真实通视分析、可视域分析、阴影分析时，替换 EvaluateVisibility 内部实现即可。
        private static readonly double OpenTreasureVisibleDistance = ArcEngine.ConUtil.OpenTreaDistance;//;

        private readonly IArcEngineAdapter _arcEngineAdapter;
        private readonly EnvironmentState _state;

        // 全局光照等级（0-100），模拟日夜变化
        private int _globalLightLevel = 50;
        // 每次 UpdateLightingCycle 调用时的变化步长，正值为增亮，负值为变暗
        private int _lightingCycleStep = 5;

        public EnvironmentModule(IArcEngineAdapter arcEngineAdapter)
        {
            _arcEngineAdapter = arcEngineAdapter;
            _state = new EnvironmentState
            {
                GlobalLightLevel = _globalLightLevel,
                IsShadowPhase = false,
                Weather = "Clear"
            };
        }

        public ResponseResult<bool> Initialize()
        {
            return ResponseFactory.Ok(true, "Environment module initialized.");
        }

        public ResponseResult<bool> Shutdown()
        {
            return ResponseFactory.Ok(true, "Environment module closed.");
        }

        public ResponseResult<EnvironmentState> GetCurrentState()
        {
            return ResponseFactory.Ok(_state, "Environment state loaded.");
        }

        /// <summary>
        /// 设置全局光照等级（0-100）。
        /// 低于30时进入阴影阶段，露天宝藏光照条件将不满足。
        /// </summary>
        public ResponseResult<bool> SetGlobalLightLevel(int lightLevel)
        {
            if (lightLevel < 0 || lightLevel > 100)
            {
                return ResponseFactory.Fail<bool>(ErrorCodes.InvalidArgument, "光照等级必须在 0-100 之间。");
            }

            _globalLightLevel = lightLevel;
            _state.GlobalLightLevel = lightLevel;
            _state.IsShadowPhase = lightLevel < ConUtil.ShadowThreshold;
            return ResponseFactory.Ok(true, string.Format("全局光照已设置为 {0}。", lightLevel));
        }

        public ResponseResult<int> GetGlobalLightLevel()
        {
            return ResponseFactory.Ok(_globalLightLevel, "Current light level retrieved.");
        }

        /// <summary>
        /// 推进光照周期，模拟日夜变化。
        /// 到达上限(100)后开始下降，到达下限(0)后开始上升。
        /// 可由 UI 定时器定期调用。
        /// </summary>
        public ResponseResult<bool> UpdateLightingCycle()
        {
            if (_globalLightLevel >= 100) _lightingCycleStep = -5;
            else if (_globalLightLevel <= 0) _lightingCycleStep = 5;

            _globalLightLevel += _lightingCycleStep;
            _state.GlobalLightLevel = _globalLightLevel;
            _state.IsShadowPhase = _globalLightLevel < ConUtil.ShadowThreshold;

            return ResponseFactory.Ok(true, string.Format("光照周期更新，当前光照：{0}。", _globalLightLevel));
        }

        /// <summary>
        /// 评估露天宝藏可见性：光照叠加通视分析。
        /// 后续接入 ArcEngine 时，替换 CheckLineOfSight 和 EvaluateLightCondition 的实现即可。
        /// </summary>
        public ResponseResult<VisibilityCheckResult> EvaluateVisibility(GeoPosition observer, GeoPosition target, int requiredLightLevel)
        {
            // 通视分析接入点（后续替换为 ArcEngine 真实通视/可视域分析）
            var lineOfSight = _arcEngineAdapter.CheckLineOfSight(observer, target);
            // 光照分析接入点（后续替换为 ArcEngine 阴影/亮度分析）
            var lightResult = _arcEngineAdapter.EvaluateLightCondition(target);

            // 优先使用 ArcEngine 返回的光照值，否则使用全局光照
            var actualLightLevel = lightResult.Success ? lightResult.Data : _globalLightLevel;
            var hasLineOfSight = lineOfSight.Success ? lineOfSight.Data : true;
            var meetsLightRequirement = actualLightLevel >= requiredLightLevel;
            var distance = AEUtil.CalcDistanceMeters(observer, target);
            var meetsDistanceRequirement = distance <= OpenTreasureVisibleDistance;

            return ResponseFactory.Ok(new VisibilityCheckResult
            {
                IsVisible = hasLineOfSight && meetsLightRequirement && meetsDistanceRequirement,
                HasLineOfSight = hasLineOfSight,
                MeetsLightRequirement = meetsLightRequirement,
                MeetsDistanceRequirement = meetsDistanceRequirement,
                Distance = distance,
                ActualLightLevel = actualLightLevel,
                DetailMessage = string.Format(
                    "距离:{0:F1}m 光照:{1}/{2} 通视:{3}",
                    distance, actualLightLevel, requiredLightLevel, hasLineOfSight ? "✓" : "✗")
            }, "Visibility evaluated.");
        }

       
    }
}
