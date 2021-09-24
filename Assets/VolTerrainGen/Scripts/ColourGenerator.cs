using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ColourGenerator {
    public Material mat;
    public Gradient gradient;
    public float normalOffsetWeight;

    Texture2D texture;
    const int textureResolution = 50;

    public ColourGenerator(Material mat, Gradient grad, float normalOffsetWeight) {
        this.mat = mat;
        gradient = grad;
        this.normalOffsetWeight = normalOffsetWeight;
    }

    void Init() {
        if (texture == null || texture.width != textureResolution) {
            texture = new Texture2D(textureResolution, 1, TextureFormat.RGBA32, false);
        }
    }

    public void UpdateColor(float chunkSize, Vector3Int numChunks) {
        Init();
        UpdateTexture();

        float boundsY = chunkSize * numChunks.y;

        mat.SetFloat("boundsY", boundsY);
        mat.SetFloat("normalOffsetWeight", normalOffsetWeight);

        mat.SetTexture("ramp", texture);
    }

    void UpdateTexture() {
        if (gradient != null) {
            Color[] colours = new Color[texture.width];
            for (int i = 0; i < textureResolution; i++) {
                Color gradientCol = gradient.Evaluate(i / (textureResolution - 1f));
                colours[i] = gradientCol;
            }

            texture.SetPixels(colours);
            texture.Apply();
        }
    }
}