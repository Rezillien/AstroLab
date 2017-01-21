using UnityEngine;
using System.Collections;

public class CameraObjectController : WorldObjectController {

    //events
    public delegate void CameraStateChangedEventHandler(Coords2 coords, CameraObjectController cameraController);
    static public event CameraStateChangedEventHandler OnCameraStateChanged;

    private Vector2 origin;
    private bool isOn;

    // Use this for initialization
    void Start () {
        isOn = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    //every modifying function has to have coords passed because they are needed for emitting an event
    //TODO: consider storing coords as a property
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

    public override bool Interact(Coords2 coords, Player player)
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
