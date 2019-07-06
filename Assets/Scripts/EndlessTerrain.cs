using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {

    public const float maxViewDist = 450;
    public Transform viewer;
    public Material mapMaterial;
    
    public static Vector2 viewerPosition;
    static MapGenerator mapGenerator;
    int chunkSize;
    int chunksVisibleInDist;

    Dictionary<Vector2, TerrainChunk> terrainChunkGen = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start() {
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInDist = Mathf.RoundToInt(maxViewDist / chunkSize);
    }

    void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks() {

        for(int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);
        for(int yOffset = -chunksVisibleInDist; yOffset <= chunksVisibleInDist; yOffset++) {
            for (int xOffset = -chunksVisibleInDist; xOffset <= chunksVisibleInDist; xOffset++) {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkGen.ContainsKey(viewedChunkCoord)) {
                    terrainChunkGen[viewedChunkCoord].UpdateTerrainChunk();
                    if (terrainChunkGen[viewedChunkCoord].isVisible()) {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkGen[viewedChunkCoord]);
                    }
                } else {
                    terrainChunkGen.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, mapMaterial));
                }
            }
        }
    }

    public class TerrainChunk {

        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MapData mapData;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        public TerrainChunk(Vector2 coord, int size, Transform parent, Material mat) {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = mat;

            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;
            SetVisible(false);

            mapGenerator.RequestMapData(OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData) {
            mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
        }

        void OnMeshDataReceived(MeshData meshData) {
            meshFilter.mesh = meshData.CreateMesh();
        }

        public void UpdateTerrainChunk() {
            float viewerDistFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDistFromNearestEdge <= maxViewDist;
            SetVisible(visible);
        }

        public void SetVisible(bool visible) {
            meshObject.SetActive(visible);
        }

        public bool isVisible() {
            return meshObject.activeSelf;
        }
    }
}
