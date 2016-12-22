using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    //TODO: some method of scaling 

    private GameObject player;
    private Vector3 mousePositionFirst;
    private Vector3 mousePositionSecond;
    private Vector3 transformVector;
    public float moveSpeed = 1;
    public float altitude = 10.0f;

    // Use this for initialization
    void Start()
    {
        mousePositionFirst = mousePositionSecond = Input.mousePosition;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        mousePositionFirst = mousePositionSecond;
        mousePositionSecond = Input.mousePosition;
        if (Input.GetMouseButton(1))
        {
            Vector3 worldPosFirst = Camera.main.ScreenToWorldPoint(mousePositionFirst);
            Vector3 worldPosSecond = Camera.main.ScreenToWorldPoint(mousePositionSecond);
            transformVector = new Vector3(worldPosFirst.x - worldPosSecond.x, worldPosFirst.y - worldPosSecond.y, -altitude);
            transform.position = new Vector3(transform.position.x + transformVector.x * moveSpeed, transform.position.y + transformVector.y * moveSpeed, transformVector.z);
        }
        else
        {
            transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -altitude);
        }

    }
}

