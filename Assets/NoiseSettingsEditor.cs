#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NoiseSettings))]
public class NoiseSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        NoiseSettings noiseSettings = (NoiseSettings)target;

        if (GUILayout.Button("Update HeightMap"))
        {
            noiseSettings.UpdateHeightMap();
        }

        GUI.DrawTexture(new Rect(10, 200, 256, 256), noiseSettings.GetHeightTexture(), ScaleMode.ScaleToFit, true, 0f);
    }
}
#endif