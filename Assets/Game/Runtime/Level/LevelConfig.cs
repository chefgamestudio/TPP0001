using UnityEngine;

namespace gs.chef.game.level
{
    public class LevelConfig
    {
        #region Level_Attempts
        
        private const string LevelAttemptsPlayerPrefsKey = "Level_Attempts";
        
        private int _levelAttempts;

        public int LevelAttempts
        {
            get
            {
                _levelAttempts = PlayerPrefs.GetInt(LevelAttemptsPlayerPrefsKey, 3);
                return _levelAttempts;
            }
            set
            {
                _levelAttempts = value;
                PlayerPrefs.SetInt(LevelAttemptsPlayerPrefsKey, _levelAttempts);
            }
        }

        #endregion
    }
}