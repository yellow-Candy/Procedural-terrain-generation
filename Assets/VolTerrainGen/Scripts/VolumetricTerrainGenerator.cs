using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class VolumetricTerrainGenerator : MonoBehaviour
{
    public const int threadGroupSize = 8;

    [Header("General settings")]
    public bool autoUpdateInEditor = true;

    [Range(2, 100)]
    public int numPointsPerAxis = 26;
    public float isoLevel;
    bool settingsUpdated = true;

    [Header("Noise settings")]
    public float noiseScale;
    public int seed;
    public int numOctaves = 4;
    public float lacunarity = 2;
    public float persistence = .5f;
    public float noiseWeight = 1;
    public bool closeEdges;
    public float floorOffset = 1;
    public float weightMultiplier = 1;

    public float hardFloorHeight;
    public float hardFloorWeight;

    [Header("Chunk settings")]
    public float chunkSize = 1;
    public Vector3Int numChunks = Vector3Int.one;
    List<Chunk> chunks;

    [Header("Color settings")]
    public Gradient gradient;
    public float normalOffsetWeight;

    [Header("References")]
    public ComputeShader marchingCube;
    public Material mat;


    [Header("Gizmos")]
    public bool showBoundsGizmo = true;
    public Color boundsGizmoCol = Color.white;


    NoiseGenerator noiseGenerator;
    VolTerrainMeshGenerator meshGenerator;
    ColourGenerator colourGenerator;


    void Update() {
        if (settingsUpdated) {
            noiseGenerator = new NoiseGenerator();
            noiseGenerator.setNoiseValues(noiseScale, seed, numOctaves, lacunarity, persistence, noiseWeight, closeEdges, floorOffset, weightMultiplier, hardFloorHeight, hardFloorWeight);
            meshGenerator = new VolTerrainMeshGenerator(threadGroupSize, autoUpdateInEditor, numPointsPerAxis, isoLevel, chunkSize, marchingCube, numChunks, mat);
            chunks = meshGenerator.RequestMeshUpdate();
            settingsUpdated = false;
        }
        colourGenerator = new ColourGenerator(mat, gradient, normalOffsetWeight);
        colourGenerator.UpdateColor(chunkSize, numChunks);
    }

    void OnValidate() {
        settingsUpdated = true;
    }

    void OnDrawGizmos() {
        if (showBoundsGizmo) {
            Gizmos.color = boundsGizmoCol;

            List<Chunk> chunks = (this.chunks == null) ? new List<Chunk>(FindObjectsOfType<Chunk>()) : this.chunks;
            foreach (var chunk in chunks) {
                //Bounds bounds = new Bounds(Chunk.CentreFromId(chunk.id, numChunks, chunkSize), Vector3.one * chunkSize);
                Gizmos.color = boundsGizmoCol;
                Gizmos.DrawWireCube(Chunk.CentreFromId(chunk.id, numChunks, chunkSize), Vector3.one * chunkSize);
            }
        }
    }
}
