using UnityEngine;
using System.Collections;

public class PersistentFogController : MonoBehaviour
{

    enum Visibility
    {
        Undiscovered,
        Discovered,
        Visible
    }
    private Texture2D texture;
    private Visibility[,] oldTileVisibility;
    private Visibility[,] tileVisibility;
    private Sprite sprite;
    private PlayerMovement player;
    private Map map;
    private int textureSize;
    private bool textureChanged;
    
    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
        map = GameManager.instance.GetMap();

        textureSize = Mathf.Max(map.width, map.height);
        texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBAFloat, false, true);
        texture.wrapMode = TextureWrapMode.Repeat;
        oldTileVisibility = new Visibility[textureSize, textureSize];
        tileVisibility = new Visibility[textureSize, textureSize];

        Color[] colors= new Color[textureSize * textureSize];
        //fog initialization
        for (int y = 0, i = 0; y < textureSize; ++y)
        {
            for (int x = 0; x < textureSize; ++x, ++i)
            {
                colors[i] = ChooseColor(Visibility.Undiscovered);
                tileVisibility[x, y] = Visibility.Undiscovered;
                oldTileVisibility[x, y] = Visibility.Undiscovered;
            }
        }
        texture.SetPixels(colors);
        texture.Apply();

        //create fog texture and set shader uniforms
        sprite = Sprite.Create(texture, Rect.MinMaxRect(0, 0, textureSize, textureSize), new Vector2(0, 0));

        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;

        Material material = spriteRenderer.material;
        material.mainTexture = texture;

        Transform transform = gameObject.GetComponent<Transform>();
        transform.position = new Vector3(-0.5f, -0.5f, 0);
        transform.localScale = new Vector3(100, 100, 1.0f);
    }

    private void UpdateTileVisibility(Coords2 coords, bool[,] isVisited)
    {
        //in this type of fog a tile can be either visible or not, fractional opacity is not supported
        const float opacityThreshold = 0.5f;

        if (map.Opacity(coords) > opacityThreshold) return;
        if (isVisited[coords.x, coords.y] == true) return;

        isVisited[coords.x, coords.y] = true;
        tileVisibility[coords.x, coords.y] = Visibility.Visible;

        //flood fill room
        if (coords.x > 0) UpdateTileVisibility(new Coords2(coords.x - 1, coords.y), isVisited);
        if (coords.y > 0) UpdateTileVisibility(new Coords2(coords.x, coords.y - 1), isVisited);
        if (coords.x < textureSize - 1) UpdateTileVisibility(new Coords2(coords.x + 1, coords.y), isVisited);
        if (coords.y < textureSize - 1) UpdateTileVisibility(new Coords2(coords.x, coords.y + 1), isVisited);
    }

    private void UpdateTileVisibility()
    {
        bool[,] isVisited = new bool[textureSize, textureSize];
        UpdateTileVisibility(new Coords2(player.x, player.y), isVisited);
    }

    void FixedUpdate()
    {
        for (int x = 0; x < textureSize; ++x)
        {
            for (int y = 0; y < textureSize; ++y)
            {
                oldTileVisibility[x, y] = tileVisibility[x, y];
                //after that flood fill i performed to mark still visible tiles
                if (tileVisibility[x, y] == Visibility.Visible)
                    tileVisibility[x, y] = Visibility.Discovered;
            }
        }

        UpdateTileVisibility();
        
        for (int x = 0; x < textureSize; ++x)
        {
            for (int y = 0; y < textureSize; ++y)
            {
                //update only when necessary
                if (tileVisibility[x, y] != oldTileVisibility[x, y])
                {
                    UpdateColor(new Coords2(x, y));
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //apply is expensive, so do it only when there was a change
        if (textureChanged) texture.Apply();
        textureChanged = false;
    }

    void UpdateColor(Coords2 coords)
    {
        SetColor(coords, ChooseColor(tileVisibility[coords.x, coords.y]));
    }

    void SetColor(Coords2 coords, Color color)
    {
        texture.SetPixel(coords.x, coords.y, color);
        textureChanged = true;
    }

    Color ChooseColor(Visibility v)
    {
        const float undiscoveredOpacity = 1.0f;
        const float discoveredOpacity = 0.75f;
        const float visible = 0.0f;

        float opacity = 0.0f;
        
        if (v == Visibility.Undiscovered) opacity = undiscoveredOpacity;
        else if (v == Visibility.Discovered) opacity = discoveredOpacity;
        else if (v == Visibility.Visible) opacity = visible;

        //black with opacity
        return new Color(0.0f, 0.0f, 0.0f, opacity);
    }
}
