using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    //TODO: some method of scaling 

    private Vector3 mousePositionFirst;
    private Vector3 mousePositionSecond;
    private Vector3 transformVector;
    public float moveSpeed = 1/16.0f;

    // Use this for initialization
    void Start()
    {
        mousePositionFirst = mousePositionSecond = Input.mousePosition;

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
            transformVector = new Vector3(mousePositionFirst.x - mousePositionSecond.x, mousePositionFirst.y - mousePositionSecond.y, -10.0f);
            transform.position = new Vector3(transform.position.x + transformVector.x * moveSpeed, transform.position.y + transformVector.y * moveSpeed, transformVector.z);
        }

    }
}

