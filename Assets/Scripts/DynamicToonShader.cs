using UnityEngine;

[ExecuteInEditMode]
public class DynamicToonShader : MonoBehaviour {
    public Material toonMaterial;

    void Start() {
        UpdateLighting();
    }

    void Update() {
        UpdateLighting();
    }

    void UpdateLighting() {
        Light[] lights = FindObjectsOfType<Light>();
        Vector3 totalLight = Vector3.zero;

        foreach (Light light in lights) {
            if (light != null) {
                Vector3 lightDir = (transform.position - light.transform.position).normalized;
                totalLight += lightDir * light.intensity;
            }
        }

        totalLight.Normalize();
        toonMaterial.SetVector("_WorldSpaceLightPos0", new Vector4(totalLight.x, totalLight.y, totalLight.z, 0));
    }
}
