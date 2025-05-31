using MusicTree.Models.Entities;

namespace MusicTree.Utils
{
    public class MgpcCalculator
    {
        // A partir de la espe
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
            float bpmDistance = CalculateBpmDistance(genre1.Bpm, genre2.Bpm); // Fixed: use average BPM
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
            // Mode is between 0-1, so direct absolute difference is already normalized
            return Math.Abs(mode1 - mode2);
        }

        private float CalculateBpmDistance(int bpm1, int bpm2)
        {
            // Normalize BPM distance to 0-1 scale (assuming max difference is 250)
            float distance = Math.Abs(bpm1 - bpm2) / 250f;
            return Math.Min(1f, distance); // Cap at 1.0
        }

        private float CalculateVolumeDistance(int volume1, int volume2)
        {
            // Normalize volume distance to 0-1 scale (range is -60 to 0, so max difference is 60)
            float distance = Math.Abs(volume1 - volume2) / 60f;
            return Math.Min(1f, distance); // Cap at 1.0
        }

        private float CalculateCompasDistance(int compas1, int compas2)
        {
            // Handle undefined compas (value 0)
            if (compas1 == 0 || compas2 == 0)
                return 1f; // Maximum distance if either is undefined

            // For time signatures, we care about the mathematical relationship
            // Common signatures: 2/4, 3/4, 4/4, 6/8, etc.
            // Simple approach: treat as categorical with some ordering
            if (compas1 == compas2)
                return 0f; // Identical

            // Special cases for related time signatures
            if ((compas1 == 2 && compas2 == 4) || (compas1 == 4 && compas2 == 2))
                return 0.3f; // 2/4 and 4/4 are related

            if ((compas1 == 3 && compas2 == 6) || (compas1 == 6 && compas2 == 3))
                return 0.4f; // 3/4 and 6/8 are related

            // General case: normalize by max difference in range (2-8)
            float distance = Math.Abs(compas1 - compas2) / 6f;
            return Math.Min(1f, distance);
        }

        private float CalculateDurationDistance(int duration1, int duration2)
        {
            // Normalize duration distance to 0-1 scale (max is 3600 seconds = 1 hour)
            float distance = Math.Abs(duration1 - duration2) / 3600f;
            return Math.Min(1f, distance); // Cap at 1.0
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

        /// <summary>
        /// Calculate BPM overlap coefficient between two genres
        /// </summary>
        /// <param name="genre1">First genre</param>
        /// <param name="genre2">Second genre</param>
        /// <returns>Overlap coefficient (0-1)</returns>
        public float CalculateBpmOverlap(Genre genre1, Genre genre2)
        {
            //Find the overlap range
            int overlapStart = Math.Max(genre1.BpmLower, genre2.BpmLower);
            int overlapEnd = Math.Min(genre1.BpmUpper, genre2.BpmUpper);

            if (overlapStart > overlapEnd)
                return 0f; //No overlap

            //Calculate overlap amount
            int overlapRange = overlapEnd - overlapStart;
            int totalRange = Math.Max(genre1.BpmUpper - genre1.BpmLower, genre2.BpmUpper - genre2.BpmLower);

            if (totalRange == 0)
                return 1f; //Both have same single BPM value

            return (float)overlapRange / totalRange;
        }
    }
}