using UnityEngine;
public static class GeneralCalculations{
    public static float ClampedValue(float value) {
        value = Mathf.Clamp(value, 0, float.MaxValue);
        return value;
    }

    public static float ToPercent(float value, float max) {
        if (max <= Mathf.Epsilon)
            return 0f;

        float ratio = value / max;
        ratio = Mathf.Clamp01(ratio);
        return ratio * 100f;
    }
    public static float LogarithmicScale(float baseValue, float maxLimit) {
        if (baseValue <= 0f)
            return 0f;
        if (maxLimit <= 0f)
            return baseValue;

        float scaled = maxLimit * (1f - Mathf.Exp(-baseValue / maxLimit));
        Debug.Log(1 + ((scaled) / 100));
        return 1 + ((scaled) / 100);
    }
}
