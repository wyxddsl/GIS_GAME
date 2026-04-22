using System;
using System.Windows.Forms;

namespace GISGameFramework.Game.ArcEngine
{
    /// <summary>
    /// 地图加载完成后触发一次 Hillshade 重算回调，不再做定时光照计算。
    /// </summary>
    public class SunFieldUpdater : IDisposable
    {
        private readonly Action<string> _logger;
        private readonly Action _onFirstUpdateDone;

        public bool IsFirstUpdateDone { get; private set; }

        public SunFieldUpdater(Action<string> logger = null, Action onFirstUpdateDone = null)
        {
            _logger            = logger;
            _onFirstUpdateDone = onFirstUpdateDone;
        }

        /// <summary>
        /// 触发一次回调，通知地图已就绪，可以进行 Hillshade 重算。
        /// </summary>
        public void Start()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    WriteLog("[SunFieldUpdater] 地图加载完成，触发 Hillshade 初始化。");
                }
                finally
                {
                    if (!IsFirstUpdateDone)
                    {
                        IsFirstUpdateDone = true;
                        if (_onFirstUpdateDone != null)
                            _onFirstUpdateDone.Invoke();
                    }
                }
            });
        }

        public void Dispose() { }

        private void WriteLog(string str)
        {
            if (_logger != null)
                _logger(str);
        }
    }
}
