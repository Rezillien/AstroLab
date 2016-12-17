using UnityEngine;
using System.Collections;

public class Mover : MonoBehaviour
    //CZA ZMIENIć NAZWE KUHWA
{
    public GameObject dirt;
    public float speed;

    private GameObject diirt;
    private SmoothTransition moveAnimation;
    private Map map;
    private int dx, dy;

    void Start()
    {
        moveAnimation = null;
        map = GameManager.instance.GetMap();
        Vector3 bulletRotation = transform.rotation.eulerAngles;
        float z = bulletRotation.z;
        if (z >= 0 && z < 45 || z >315 && z <=360) { dx = 1; dy = 0; }
        else if (z >= 45 && z < 135) { dx = 0; dy = 1; }
        else if (z > 135 && z < 225) { dx = -1; dy = 0; }
        else if (z >= 225 && z < 315) { dx = 0; dy = -1; }
    }

    void Update()
    {
        if (!map.HasCollider(new Coords2((int)(transform.position.x + dx), (int)(transform.position.y + dy))))
        {
            moveAnimation = new SmoothTransition(new Vector3(transform.position.x, transform.position.y, 0.0f), new Vector3(transform.position.x + dx, transform.position.y + dy, 0.0f), 0.25f);
            moveAnimation.Update();
            transform.position = moveAnimation.GetInterpolatedPosition();
        }
        else
        {
            map.CreateWallTile(new Coords2((int)(transform.position.x), (int)(transform.position.y)), dirt);
            Destroy(gameObject);
        }
    }
}