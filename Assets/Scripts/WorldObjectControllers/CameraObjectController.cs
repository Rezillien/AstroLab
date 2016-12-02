using UnityEngine;
using System.Collections;

public class CameraObjectController : WorldObjectController {

    public delegate void CameraStateChangedEventHandler(Coords2 coords, CameraObjectController cameraController);
    static public event CameraStateChangedEventHandler OnCameraStateChanged;

    private SpriteRenderer sprite;
    private Vector2 origin;
    private bool isOn;

    // Use this for initialization
    void Start () {
        sprite = GetComponent<SpriteRenderer>();
        isOn = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetOrigin(Coords2 coords, Vector2 newOrigin)
    {
        origin = newOrigin;
        if (OnCameraStateChanged != null) OnCameraStateChanged(coords, this);
    }

    public void SetEnabled(Coords2 coords, bool newIsOn)
    {
        isOn = newIsOn;
        if (OnCameraStateChanged != null) OnCameraStateChanged(coords, this);
    }

    public void SetAnchor(Vector2 newAnchor)
    {
        transform.position = new Vector3(newAnchor.x, newAnchor.y, 0.0f);
    }

    public void RotateAroundOrigin(float rotation)
    {
        transform.Rotate(new Vector3(0.0f, 0.0f, 1.0f), rotation, Space.Self);
    }

    public override bool Interact(Coords2 coords, GameObject player)
    {
        SetEnabled(coords, !isOn);

        return true;
    }

    public bool IsOn()
    {
        return isOn;
    }
    public Vector2 GetOrigin()
    {
        return origin;
    }
}
