using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GloveManager : MonoBehaviour
{
    private HapticGlove glove = null;
    public float[] fingerBendValues = new float[5];
    private float[] values = new float[3];
    public Slider[] sliders;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < sliders.Length; i++)
        {
            sliders[i].onValueChanged.AddListener((val) => OnValueChange());
        }
        try
        {
            glove = new HapticGlove();
            glove.OnDeviceTrackingChanged = (isTracking) =>
            {
                Debug.Log("Tracking: " + isTracking);
            };
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    void OnValueChange()
    {
        for (int i = 0; i < sliders.Length; i++)
        {
            var val = sliders[i].value;
            if (val != values[i])
            {
                var data = new ForceHapticData();
                data.isLeft = false;
                data.fingerType = i;
                data.bendValue = val;
                glove.ExecuteForceHaptic(data);
                values[i] = val;
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (glove != null)
        {
            glove.Dispose();
        }
    }

    private void Update()
    {
        var handData = glove.GetHandData(false);
        for (int i = 0; i < handData.fingerBendValue.Length; i++)
        {
            fingerBendValues[i] = handData.fingerBendValue[i];
        }
    }
}
