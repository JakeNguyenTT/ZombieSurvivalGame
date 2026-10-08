using System;
using System.Collections.Generic;

public static class WeightedRandom
{
    // roll01 in [0, 1). Returns -1 when there is nothing to pick.
    public static int Pick(IList<float> weights, float roll01)
    {
        float total = 0f;
        for (int i = 0; i < weights.Count; i++)
            if (weights[i] > 0f) total += weights[i];
        if (total <= 0f) return -1;

        float target = roll01 * total;
        int last = -1;
        for (int i = 0; i < weights.Count; i++)
        {
            if (weights[i] <= 0f) continue;
            last = i;
            if (target < weights[i]) return i;
            target -= weights[i];
        }
        return last; // floating point fallback for roll01 ~ 1
    }

    // Picks up to `count` distinct indices, skipping zero weights.
    public static List<int> PickIndices(IList<float> weights, int count, Func<float> roll01)
    {
        var remaining = new List<float>(weights);
        var result = new List<int>();
        while (result.Count < count)
        {
            int index = Pick(remaining, roll01());
            if (index < 0) break;
            result.Add(index);
            remaining[index] = 0f;
        }
        return result;
    }
}
