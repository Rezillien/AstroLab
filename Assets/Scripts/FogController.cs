using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class FogController : MonoBehaviour {

    private Texture2D texture;
    private Sprite sprite;
    private Transform playerTransform;
    private Map map;
    private int textureSize;
    private bool textureChanged;
    
	// Use this for initialization
	void Start () {
        playerTransform = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>().transform;
        map = GameManager.instance.GetMap();

        textureSize = Mathf.Max(map.width, map.height);
        texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBAFloat, false, true);
        texture.wrapMode = TextureWrapMode.Repeat;
        //texture.filterMode = FilterMode.Point;
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
        Material material = spriteRenderer.material;
        material.renderQueue = 3001;
        material.mainTexture = texture;
        spriteRenderer.sprite = sprite;
        transform.position = new Vector3(-0.5f, -0.5f, 0);
        transform.localScale = new Vector3(100, 100, 1.0f);

        map.OnWallTileChanged += new Map.WallTileChangedEventHandler(OnWallTileChanged);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(textureChanged) texture.Apply();
        textureChanged = false;

        float playerX = playerTransform.position.x;
        float playerY = playerTransform.position.y;
        float texX = (playerX + 0.5f) / textureSize;
        float texY = (playerY + 0.5f) / textureSize;
        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        Material material = spriteRenderer.material;
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

        bool blocksLight = map.IsBlockingLight(coords);
        if (blocksLight) color.a = 1.0f;
        return color;
    }

    void OnWallTileChanged(Coords2 coords)
    {
        UpdateColor(coords);
    }
}
