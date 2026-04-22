using System;

namespace GISGameFramework.Game.Modules
{
    public class ScoreRecord
    {
        public int ChangeAmount { get; set; }
        public int NewTotal { get; set; }
        public string Reason { get; set; }
        public string ReasonDetail { get; set; }
        public DateTime ChangeTime { get; set; }

        public ScoreRecord()
        {
            ChangeTime = DateTime.Now;
        }
    }
}
