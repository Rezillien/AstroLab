using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class FogController : MonoBehaviour {

    public Texture2D texture;
    public Sprite sprite;
    public Transform playerTransform;
    public Map map;
    public int textureSize;
    
	// Use this for initialization
	void Start () {
        playerTransform = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>().transform;
        map = GameManager.instance.GetMap();

        textureSize = Mathf.Max(map.width, map.height);
        texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBAFloat, false, true);
        texture.wrapMode = TextureWrapMode.Repeat;
        //texture.filterMode = FilterMode.Point;

        for (int x = 0; x < textureSize; ++x)
        {
            for (int y = 0; y < textureSize; ++y)
            {
                GameObject tile = map.GetWallTile(new Coords2(x, y));
                SetSolid(new Coords2(x, y), tile != null);
            }
        }

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
        texture.Apply();

        float playerX = playerTransform.position.x;
        float playerY = playerTransform.position.y;
        float texX = (playerX + 0.5f) / textureSize;
        float texY = (playerY + 0.5f) / textureSize;
        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        Material material = spriteRenderer.material;
        material.SetVector(Shader.PropertyToID("_PlayerPos"), new Vector4(texX, texY, 0, 0));
    }

    void SetSolid(Coords2 coords, bool solid)
    {
        Color color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        if (solid) color.a = 1.0f;
        texture.SetPixel(coords.x, coords.y, color);
    }

    void OnWallTileChanged(Coords2 coords)
    {
        SetSolid(coords, map.IsBlockingLight(coords));
    }
}
