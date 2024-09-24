using System;
using System.Numerics;

public struct HandData
{
    public bool isLeft;
    public bool isTracked;
    public float[] fingerBendValue; // (0, 1)
    public JointTransform wristTransforms;
}

public struct JointTransform
{
    public Vector3 position;
    public Quaternion rotation;
}
public struct ForceHapticData
{
    public float intensity;
    public float bendValue;
    public bool inward;
    public bool isLeft;
    public int fingerType;
}

public struct VibrationHapticData
{
    public float intensity;
    public float duration;
    public bool isLeft;
    public int fingerType;
}
public interface IHapticGlove
{
    public void ExecuteForceHaptic(ForceHapticData data);
    public void ExecuteVibrationHaptic(VibrationHapticData data);
    public void StopAllForceHaptic();
    public void StopAllVibrationHaptic();
    public HandData GetHandData(bool isLeft);
}
