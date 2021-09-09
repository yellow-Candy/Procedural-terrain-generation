using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class VolumetricTerrainGenerator : MonoBehaviour
{
    public const int threadGroupSize = 8;

    public bool autoUpdateInEditor = true;
    public ComputeShader shader;

    [Range(2, 100)]
    public int numPointsPerAxis = 30;
    public float isoLevel;
    public bool settingsUpdated = true;


    public float boundsSize = 1;

    [Header("Gizmos")]
    public bool showBoundsGizmo = true;
    public Color boundsGizmoCol = Color.white;


    private void Start() {

    }


    void Update() {
        if (settingsUpdated) {
            VolTerrainMeshGenerator meshGenerator = new VolTerrainMeshGenerator(threadGroupSize, autoUpdateInEditor, numPointsPerAxis, isoLevel, settingsUpdated, boundsSize, shader);
            meshGenerator.RequestMeshUpdate();
            settingsUpdated = false;
        }
    }

    void OnValidate() {
        settingsUpdated = true;
    }

    void OnDrawGizmos() {
        Vector3 coord = new Vector3(0, 0, 0);
        Gizmos.color = boundsGizmoCol;
        Gizmos.DrawWireCube(coord, Vector3.one * boundsSize);
    }
}
