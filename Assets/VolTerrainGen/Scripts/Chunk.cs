using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {
    public Vector3 id;

    [HideInInspector]
    public Mesh mesh;

    MeshFilter filter_;
    MeshRenderer renderer_;
    MeshCollider collider_;
    bool generateCollider;

    public void Destroy() {
        DestroyImmediate(gameObject, false);
    }

    public static Chunk InitChunks(Vector3 id, GameObject chunkHolder) {
        GameObject chunk = new GameObject($"Chunk ({id.x}, {id.y}, {id.z})");
        chunk.transform.parent = chunkHolder.transform;
        Chunk newChunk = chunk.AddComponent<Chunk>();
        newChunk.id = id;
        return newChunk;
    }

    public static Vector3 CentreFromId(Vector3 id, Vector3Int numChunks, float chunkSize) {
        Vector3 totalBounds = (Vector3)numChunks * chunkSize;
        return -totalBounds / 2 + id * chunkSize + Vector3.one * chunkSize / 2;
    }

    public static GameObject CreateChunkHolder(GameObject chunkHolder, string chunkHolderName) {
        // Create/find mesh holder object for organizing chunks under in the hierarchy
        if (chunkHolder == null) {
            if (GameObject.Find(chunkHolderName)) {
                chunkHolder = GameObject.Find(chunkHolderName);
            } else {
                chunkHolder = new GameObject(chunkHolderName);
            }
        }
        return chunkHolder;
    }

    // Add components/get references in case lost (references can be lost when working in the editor)
    public void SetUp(Material mat, bool generateColliders) {
        generateCollider = generateColliders;

        filter_ = GetComponent<MeshFilter>();
        renderer_ = GetComponent<MeshRenderer>();
        collider_ = GetComponent<MeshCollider>();

        if (filter_ == null) {
            filter_ = gameObject.AddComponent<MeshFilter>();
        }

        if (renderer_ == null) {
            renderer_ = gameObject.AddComponent<MeshRenderer>();
        }

        if (collider_ == null && generateCollider) {
            collider_ = gameObject.AddComponent<MeshCollider>();
        }
        if (collider_ != null && !generateCollider) {
            DestroyImmediate(collider_);
        }

        mesh = filter_.sharedMesh;
        if (mesh == null) {
            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            filter_.sharedMesh = mesh;
        }

        if (generateCollider) {
            if (collider_.sharedMesh == null) {
                collider_.sharedMesh = mesh;
            }
            // force update
            collider_.enabled = false;
            collider_.enabled = true;
        }

        renderer_.material = mat;
    }
}