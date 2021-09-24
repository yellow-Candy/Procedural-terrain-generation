using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class VolTerrainMeshGenerator {
    public int threadGroupSize;

    public bool autoUpdateInEditor;
    public ComputeShader marchingCube;

    public int numPointsPerAxis;
    public float isoLevel;

    GameObject chunkHolder;
    const string chunkHolderName = "Terrain";
    List<Chunk> chunks;
    public float chunkSize;
    Vector3Int numChunks;
    bool generateColliders = false;
    Material mat;

    ComputeBuffer triangleBuffer;
    ComputeBuffer triCountBuffer;



    public VolTerrainMeshGenerator(int threadGroupSize, bool autoUpdateInEditor, int numPointsPerAxis, float isoLevel, float chunkSize, ComputeShader shader, Vector3Int numChunks, Material mat) {
        this.threadGroupSize = threadGroupSize;

        this.autoUpdateInEditor = autoUpdateInEditor;
        marchingCube = shader;

        this.numPointsPerAxis = numPointsPerAxis;
        this.isoLevel = isoLevel;


        this.chunkSize = chunkSize;
        this.numChunks = numChunks;
        this.mat = mat;
    }

    
    
    public List<Chunk> RequestMeshUpdate() {
        if (!Application.isPlaying && autoUpdateInEditor) {
            chunkHolder = Chunk.CreateChunkHolder(chunkHolder, chunkHolderName);
            CreateChunks();
            UpdateAllChunks(chunks);
        }
        return chunks;
    }

    public void CreateChunks() {
        chunks = new List<Chunk>();
        List<Chunk> oldChunks = new List<Chunk>(Object.FindObjectsOfType<Chunk>());

        for (int x = 0; x < numChunks.x; x++) {
            for (int y = 0; y < numChunks.y; y++) {
                for (int z = 0; z < numChunks.z; z++) {
                    Vector3 chunkId = new Vector3(x, y, z);

                    bool chunkAlreadyExists = false;

                    // If chunk already exists, add it to the chunks list, and remove from the old list.
                    for (int i = 0; i < oldChunks.Count; i++) {
                        if (oldChunks[i].id == chunkId) {
                            chunks.Add(oldChunks[i]);
                            oldChunks.RemoveAt(i);
                            chunkAlreadyExists = true;
                            break;
                        }
                    }

                    // Create new chunk
                    if (!chunkAlreadyExists) {
                        var newChunk = Chunk.InitChunks(chunkId, chunkHolder);
                        chunks.Add(newChunk);
                    }

                    chunks[chunks.Count - 1].SetUp(mat, generateColliders);

                }
            }
        }

        for (int i = 0; i < oldChunks.Count; i++) {
            oldChunks[i].Destroy();
        }
    }

    

    public void UpdateAllChunks(List<Chunk> chunks) {
        foreach(Chunk chunk in chunks) {
            InitMarchingCube(chunk);
        }
    }
    
    public void InitMarchingCube(Chunk chunk) {
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / (float)threadGroupSize);

        int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCount = numVoxels * 5;


        triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);


        float voxelSize = chunkSize / (numPointsPerAxis - 1);
        Vector3 center = Chunk.CentreFromId(chunk.id, numChunks, chunkSize);
        //Transform trans = GameObject.Find("GameObject").GetComponent<MeshFilter>().transform;
        //coord = trans.position;

        triangleBuffer.SetCounterValue(0);
        marchingCube.SetBuffer(0, "triangles", triangleBuffer);
        marchingCube.SetInt("numPointsPerAxis", numPointsPerAxis);
        marchingCube.SetFloat("isoLevel", isoLevel);
        marchingCube.SetFloat("voxelSize", voxelSize);
        marchingCube.SetVector("chunkCenter", center);
        marchingCube.SetFloat("chunkSize", chunkSize);


        marchingCube.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);



        // generate mesh
        MeshGenerator(chunk);


        // release buffer.............
        triangleBuffer.Dispose();
        triCountBuffer.Dispose();
        //triangleBuffer.Release();
        //triCountBuffer.Release();
        //System.GC.SuppressFinalize(triangleBuffer);
        //System.GC.SuppressFinalize(triCountBuffer);
    }


    // Generate mesh and set it in meshfilter
    void MeshGenerator(Chunk chunk) {
        // Get number of triangles in the triangle buffer
        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        int numTris = triCountArray[0];

        // Get triangle data from shader buffer
        Triangle[] tris = new Triangle[numTris];
        triangleBuffer.GetData(tris, 0, 0, numTris);


        Mesh mesh = chunk.mesh;

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

        //GameObject.Find("GameObject").GetComponent<MeshFilter>().mesh = mesh;
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
