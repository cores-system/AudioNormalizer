using CSCore.CoreAudioAPI;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;

namespace AudioNormalizer
{
    public static class Normalizer
    {
        #region Normalizer
        static List<float> AudioMass = new List<float>() { Capacity = 200 };

        static AudioEndpointVolume AudioVolume;
        static DateTime LastDownVolume = DateTime.Now;
        #endregion

        #region MonitorPeakValue
        public static void MonitorPeakValue()
        {
            using (var sessionManager = DefaultAudioSessionManager(DataFlow.Render))
            {
                while (true)
                {
                    try
                    {
                        using (var sessionEnumerator = sessionManager.GetSessionEnumerator())
                        {
                            foreach (var session in sessionEnumerator)
                            {
                                using (var audioSessionControl2 = session.QueryInterface<AudioSessionControl2>())
                                {
                                    using (var audioMeterInformation = session.QueryInterface<AudioMeterInformation>())
                                    {
                                        var value = audioMeterInformation.GetPeakValue();
                                        string name = audioSessionControl2.Process?.ProcessName;

                                        // Что-то пошло не так
                                        if (AudioVolume == null || name == null || !name.Contains(Settings.app.AudioOutName)) {
                                            session.Dispose();
                                            continue;
                                        }

                                        //
                                        AudioMass.Add((value >= 0.01 && value <= 1) ? value : 0.01f);

                                        //
                                        if (AudioMass.Count >= Settings.app.AudioCells)
                                            AudioMass.RemoveAt(0);

                                        // Получаем среднее число
                                        float avgVol = AudioMass.Average();

                                        // Звук выше порога
                                        if (avgVol > Settings.app.MaxPeakVol)
                                        {
                                            #region Локальный метод - "SoundMoreVol"
                                            float SoundMoreVol()
                                            {
                                                // На сколько звук выше максимального порога
                                                return (float)((Settings.app.InitVol / 100.0) * (100 - ((Settings.app.MaxPeakVol / avgVol) * 100)));
                                            }
                                            #endregion

                                            // 
                                            LastDownVolume = DateTime.Now;

                                            // Звук который нужно установить
                                            float vol = (float)Math.Round(Settings.app.InitVol - SoundMoreVol(), 2);

                                            // Если звук ниже порога
                                            if (Settings.app.MinVol > vol)
                                                vol = Settings.app.MinVol;

                                            // 
                                            if (AudioVolume.MasterVolumeLevelScalar > vol)
                                            {
                                                // Понижаем звук
                                                AudioVolume.MasterVolumeLevelScalar -= 0.01f;
                                            }
                                            else
                                            {
                                                // Если разница больше установленого значения
                                                if (Math.Abs(AudioVolume.MasterVolumeLevelScalar - vol) >= Settings.app.AvgVol)
                                                    AudioVolume.MasterVolumeLevelScalar += 0.01f;
                                            }
                                        }
                                        else
                                        {
                                            // Звук ниже порога больше указаного времени
                                            if ((DateTime.Now - LastDownVolume).TotalMilliseconds > Settings.app.UpTime)
                                            {
                                                #region Локальный метод - "SoundMoreVol"
                                                float SoundMoreVol()
                                                {
                                                    // На сколько звук ниже максимального порога
                                                    return (float)((Settings.app.InitVol / 100.0) * (100 - ((avgVol / Settings.app.MaxPeakVol) * 100)));
                                                }
                                                #endregion

                                                // Звук который нужно установить
                                                float vol = (float)Math.Round(Settings.app.MinVol + SoundMoreVol(), 2);

                                                // Если звук выше порога
                                                if (vol > Settings.app.InitVol)
                                                    vol = Settings.app.InitVol;

                                                // 
                                                if (vol > AudioVolume.MasterVolumeLevelScalar)
                                                {
                                                    // Повышаем звук звук
                                                    AudioVolume.MasterVolumeLevelScalar += 0.01f;
                                                }
                                                else
                                                {
                                                    // Если разница больше установленого значения
                                                    if (Math.Abs(AudioVolume.MasterVolumeLevelScalar - vol) >= Settings.app.AvgVol)
                                                        AudioVolume.MasterVolumeLevelScalar -= 0.01f;
                                                }
                                            }
                                        }
                                    }
                                }

                                //
                                session.Dispose();
                            }
                        }
                        
                        Thread.Sleep(Settings.WaitToNewPeakValue);
                    }
                    catch (CoreAudioAPIException) { }
                }
            }
        }
        #endregion

        #region VolUp
        public static void VolUp()
        {
            //while (true)
            //{
            //    // Ждем
            //    Thread.Sleep(Settings.GainTime);

            //    // Получаем среднее число
            //    float avgVol = Average(GainMass);

            //    // Звук ниже порога
            //    if (avgVol < Settings.MaxVol)
            //    {
            //        // Текущая громкость
            //        float lastVol = AudioVolume.MasterVolumeLevelScalar;

            //        while (true)
            //        {
            //            // Если понизатор изменил звук
            //            if (lastVol > (AudioVolume.MasterVolumeLevelScalar + 0.05f) || AudioVolume.MasterVolumeLevelScalar >= Settings.InitVol)
            //                break;

            //            // Повышаем звук
            //            lastVol = AudioVolume.MasterVolumeLevelScalar += 0.01f;
            //            Thread.Sleep(Settings.VolumeLevelUpTime);
            //        }
            //    }
            //}
        }
        #endregion

      

        #region DefaultAudioSessionManager
        static AudioSessionManager2 DefaultAudioSessionManager(DataFlow dataFlow)
        {
            using (var enumerator = new MMDeviceEnumerator())
            {
                using (var device = enumerator.GetDefaultAudioEndpoint(dataFlow, Role.Multimedia))
                {
                    AudioVolume = AudioEndpointVolume.FromDevice(device);
                    return AudioSessionManager2.FromMMDevice(device);
                }
            }
        }
        #endregion
    }
}
