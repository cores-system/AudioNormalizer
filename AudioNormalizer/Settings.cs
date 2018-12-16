using System;

namespace AudioNormalizer
{
    public class Settings
    {
        public static Settings app = new Settings();


        /// <summary>
        /// 
        /// </summary>
        public static int WaitToNewPeakValue = 10;

        /// <summary>
        /// 
        /// </summary>
        public string AudioOutName = "chrome";


        /// <summary>
        /// 
        /// </summary>
        public int AudioCells = 40;

        /// 
        /// </summary>
        public int UpTime = 1200;


        /// <summary>
        /// Максимальный звук зеленой полоски
        /// </summary>
        public float MaxPeakVol = 0.6f;

        /// <summary>
        /// 
        /// </summary>
        public float MinVol = 0.15f;

        /// <summary>
        /// Установленый звук ползунка
        /// </summary>
        public float InitVol = 0.3f;

        /// <summary>
        /// 
        /// </summary>
        public float AvgVol = 0.1f;
    }
}
