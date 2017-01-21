using UnityEngine;
using System.Collections;

public class DoorController : WallTileController
{
    public AudioClip[] openSounds;
    public AudioClip[] closeSounds;
    public float soundVolume = 0.4f;

    public Sprite closedSprite;
    public Sprite openedSprite;
    public bool isOpen;

    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;

    // Use this for initialization
    void Start()
    {
        isOpen = false;
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = closedSprite;
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Toggles between open/closed, returns true indicating that some action was performed
    public override bool Interact(Coords2 coords, Player player)
    {
        isOpen = !isOpen;

        Sprite spriteToUse;
        if (isOpen)
        {
            audioSource.PlayOneShot(openSounds[Random.Range(0, openSounds.Length)], soundVolume);
            spriteToUse = openedSprite;
        }
        else
        {
            audioSource.PlayOneShot(closeSounds[Random.Range(0, closeSounds.Length)], soundVolume);
            spriteToUse = closedSprite;
        }

        spriteRenderer.sprite = spriteToUse;

        return true;
    }

    //returns if the player is unable to stand on it
    public override bool HasCollider()
    {
        return !isOpen;
    }

    //returns 0.0f when open, 1.0f when closed
    public override float Opacity()
    {
         return isOpen ? 0.0f : 1.0f;
    }
}
