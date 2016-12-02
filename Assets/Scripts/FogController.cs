using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
using System.Collections.Generic;

public class FogController : MonoBehaviour
{

    private class Camera
    {
        public Vector2 origin;
        public bool isOn;
        public int shaderPropertyId;
    }

    private const int maxNumberOfCameras = 16;

    private Texture2D texture;
    private Sprite sprite;
    private Transform playerTransform;
    private Map map;
    private int textureSize;
    private bool textureChanged;
    private Dictionary<Coords2, Camera> cameras;
    private Stack<int> unusedShaderPropertyIds;
    private Material material;

    // Use this for initialization
    void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>().transform;
        map = GameManager.instance.GetMap();

        textureSize = Mathf.Max(map.width, map.height);
        texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBAFloat, false, true);
        texture.wrapMode = TextureWrapMode.Repeat;

        Color[] colors = new Color[textureSize * textureSize];
        for (int y = 0, i = 0; y < textureSize; ++y)
        {
            for (int x = 0; x < textureSize; ++x, ++i)
            {
                colors[i] = ChooseColor(new Coords2(x, y));
            }
        }
        texture.SetPixels(colors);
        texture.Apply();

        sprite = Sprite.Create(texture, Rect.MinMaxRect(0, 0, textureSize, textureSize), new Vector2(0, 0));

        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        Transform transform = gameObject.GetComponent<Transform>();
        material = spriteRenderer.material;
        material.renderQueue = 3001;
        material.mainTexture = texture;
        material.SetInt(Shader.PropertyToID("_TextureSize"), textureSize);

        cameras = new Dictionary<Coords2, Camera>();
        unusedShaderPropertyIds = new Stack<int>();
        for (int i = 0; i < maxNumberOfCameras; ++i)
        {
            int currentId = Shader.PropertyToID("_CamerasPos" + i.ToString());
            unusedShaderPropertyIds.Push(currentId);
            material.SetVector(currentId, new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
        }

        spriteRenderer.sprite = sprite;
        transform.position = new Vector3(-0.5f, -0.5f, 0);
        transform.localScale = new Vector3(100, 100, 1.0f);

        map.OnWallTileChanged += new Map.WallTileChangedEventHandler(OnWallTileChanged);
        CameraObjectController.OnCameraStateChanged += new CameraObjectController.CameraStateChangedEventHandler(OnCameraStateChanged);
    }

    // Update is called once per frame
    void Update()
    {
        if (textureChanged) texture.Apply();
        textureChanged = false;

        float playerX = playerTransform.position.x;
        float playerY = playerTransform.position.y;
        float texX = (playerX + 0.5f) / textureSize;
        float texY = (playerY + 0.5f) / textureSize;
        material.SetVector(Shader.PropertyToID("_PlayerPos"), new Vector4(texX, texY, 0, 0));
    }

    void UpdateColor(Coords2 coords)
    {
        Color color = ChooseColor(coords);
        SetColor(coords, color);
    }

    void SetColor(Coords2 coords, Color color)
    {
        texture.SetPixel(coords.x, coords.y, color);
        textureChanged = true;
    }

    Color ChooseColor(Coords2 coords)
    {
        Color color = new Color(0.0f, 0.0f, 0.0f, 0.0f);

        float opacity = map.Opacity(coords);
        color.a = opacity;
        return color;
    }

    void OnWallTileChanged(Coords2 coords)
    {
        UpdateColor(coords);
    }

    void UpdateShaderProperty(Camera camera)
    {
        Vector4 newProperty = new Vector4(camera.origin.x / textureSize, camera.origin.y / textureSize, (camera.isOn ? 1.0f : 0.0f), 0.0f);
        material.SetVector(camera.shaderPropertyId, newProperty);
    }

    void OnCameraStateChanged(Coords2 coords, CameraObjectController cameraController)
    {
        Camera camera;
        if (cameras.TryGetValue(coords, out camera))
        {
            camera.isOn = cameraController.IsOn();
            camera.origin = cameraController.GetOrigin();
        }
        else
        {
            camera = new Camera();
            camera.origin = cameraController.GetOrigin();
            camera.isOn = cameraController.IsOn();
            camera.shaderPropertyId = unusedShaderPropertyIds.Pop();
            cameras.Add(coords, camera);
        }
        UpdateShaderProperty(camera);
    }
}
