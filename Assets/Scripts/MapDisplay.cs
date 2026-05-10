using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;

        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawSmoothTexture(float[,] noiseMap, TerrainType[] regions, int textureResolution, float blendAmount)
    {
        Texture2D biomeTexture = TextureGenerator.GenerateBiomeTexture(noiseMap, regions, textureResolution, blendAmount);

        meshRenderer.sharedMaterial.mainTexture = biomeTexture;
    }

    public void DrawMesh(MeshData meshData, Texture2D texture)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
        meshRenderer.transform.gameObject.AddComponent<MeshCollider>();
    }
}
