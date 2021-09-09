using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class VolTerrainMeshGenerator
{
    public int threadGroupSize;

    public bool autoUpdateInEditor;
    public ComputeShader shader;

    public int numPointsPerAxis;
    public float isoLevel;
    public bool settingsUpdated;


    public float boundsSize;



    ComputeBuffer triangleBuffer;
    ComputeBuffer triCountBuffer;

    Mesh mesh;


    public VolTerrainMeshGenerator(int threadGroupSize, bool autoUpdateInEditor, int numPointsPerAxis, float isoLevel, bool settingsUpdated, float boundsSize, ComputeShader shader) {
        this.threadGroupSize = threadGroupSize;

        this.autoUpdateInEditor = autoUpdateInEditor;
        this.shader = shader;

        this.numPointsPerAxis = numPointsPerAxis;
        this.isoLevel = isoLevel;
        this.settingsUpdated = settingsUpdated;


        this.boundsSize = boundsSize;

    }

    
    public void Run() {
        DrawMesh();
    }

    
    
    public void RequestMeshUpdate() {
        if (!Application.isPlaying && autoUpdateInEditor) {
            Run();
        }
    }

    
    
    
    public void DrawMesh() {
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / (float)threadGroupSize);

        int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCount = numVoxels * 5;


        triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        triangleBuffer.SetCounterValue(0);
        shader.SetBuffer(0, "triangles", triangleBuffer);
        shader.SetInt("numPointsPerAxis", numPointsPerAxis);
        shader.SetFloat("isoLevel", isoLevel);

        shader.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);



        // Get number of triangles in the triangle buffer
        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        int numTris = triCountArray[0];
        
        // Get triangle data from shader
        Triangle[] tris = new Triangle[numTris];
        triangleBuffer.GetData(tris, 0, 0, numTris);


        // generate mesh
        meshGneerator(numTris, tris);

        triangleBuffer.Dispose();
        triangleBuffer.Release();
        triCountBuffer.Dispose();
        triCountBuffer.Release();
    }

    void meshGneerator(int numTris, Triangle[] tris) {
        if (mesh == null) {
            mesh = new Mesh();
        }

        mesh.Clear();

        var vertices = new Vector3[numTris * 3];
        var meshTriangles = new int[numTris * 3];

        for (int i = 0; i < numTris; i++) {
            for (int j = 0; j < 3; j++) {
                meshTriangles[i * 3 + j] = i * 3 + j;
                vertices[i * 3 + j] = tris[i][j];
            }
        }


        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;

        mesh.RecalculateNormals();

        GameObject.Find("GameObject").GetComponent<MeshFilter>().mesh = mesh;
    }


    struct Triangle {
#pragma warning disable 649 // disable unassigned variable warning
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector3 this[int i] {
            get {
                switch (i) {
                    case 0:
                        return a;
                    case 1:
                        return b;
                    default:
                        return c;
                }
            }
        }
    }


}
