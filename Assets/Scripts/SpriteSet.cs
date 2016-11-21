using UnityEngine;
using System.Collections;

public class SpriteSet : MonoBehaviour {

    public Sprite[] sprites;

    public Sprite Get(int i)
    {
        return sprites[i];
    }

}
