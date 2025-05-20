using MusicTree.Models.Entities;

namespace MusicTree.Utils
{
    public class MgpcCalculator
    {
        // Configurable weights (per specification)
        public static float WeightMode { get; set; } = 0.2f;
        public static float WeightBpm { get; set; } = 0.15f;
        public static float WeightVolume { get; set; } = 0.1f;
        public static float WeightCompas { get; set; } = 0.15f;
        public static float WeightDuration { get; set; } = 0.1f;
        public static float WeightKey { get; set; } = 0.3f;

        public float Calculate(Genre genre1, Genre genre2)
        {
            // Distance functions between genre1 and genre2
            float modeDistance = CalculateModeDistance(genre1.GenreTipicalMode, genre2.GenreTipicalMode);
            float bpmDistance = CalculateBpmDistance(genre1.BpmLower, genre2.BpmLower);
            float volumeDistance = CalculateVolumeDistance(genre1.Volume, genre2.Volume);
            float compasDistance = CalculateCompasDistance(genre1.CompasMetric, genre2.CompasMetric);
            float durationDistance = CalculateDurationDistance(genre1.AvrgDuration, genre2.AvrgDuration);
            float keyDistance = CalculateKeyDistance(genre1.Key, genre2.Key);

            // MGPC formula: 1 - (weighted sum of distances)
            float mgpc = 1 - (
                WeightMode * modeDistance +
                WeightBpm * bpmDistance +
                WeightVolume * volumeDistance +
                WeightCompas * compasDistance +
                WeightDuration * durationDistance +
                WeightKey * keyDistance
            );

            // Ensure result is between 0 and 1
            return Math.Max(0, Math.Min(1, mgpc));
        }

        private float CalculateModeDistance(float mode1, float mode2)
        {
            return Math.Abs(mode1 - mode2);
        }

        private float CalculateBpmDistance(int bpm1, int bpm2)
        {
            // Normalize BPM distance to 0-1 scale (assuming max difference is 250)
            return Math.Abs(bpm1 - bpm2) / 250f;
        }

        private float CalculateVolumeDistance(int volume1, int volume2)
        {
            // Normalize volume distance to 0-1 scale (range is -60 to 0, so max difference is 60)
            return Math.Abs(volume1 - volume2) / 60f;
        }

        private float CalculateCompasDistance(int compas1, int compas2)
        {
            // Handle undefined compas (value 0)
            if (compas1 == 0 || compas2 == 0)
                return 1f; // Maximum distance if either is undefined

            // Normalize compas distance to 0-1 scale (range is 2-8, so max difference is 6)
            return Math.Abs(compas1 - compas2) / 6f;
        }

        private float CalculateDurationDistance(int duration1, int duration2)
        {
            // Normalize duration distance to 0-1 scale (max is 3600 seconds)
            return Math.Abs(duration1 - duration2) / 3600f;
        }

        private float CalculateKeyDistance(int key1, int key2)
        {
            // Handle undefined keys (value -1)
            if (key1 == -1 || key2 == -1)
                return 1f; // Maximum distance if either is undefined

            // Calculate circular distance on chromatic circle (0-11)
            int directDistance = Math.Abs(key1 - key2);
            int circularDistance = Math.Min(directDistance, 12 - directDistance);
            
            // Normalize to 0-1 scale (max circular distance is 6)
            return circularDistance / 6f;
        }
    }
}