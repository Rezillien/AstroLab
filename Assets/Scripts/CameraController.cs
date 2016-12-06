using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour 
{

	private Vector3 mousePositionFirst;
	private Vector3 mousePositionSecond;
	private Vector3 transformVector;
	public float moveSpeed=1f;

	private bool firstFrame=true;

	// Use this for initialization
	void Start () 
	{

	}

	// Update is called once per frame
	void Update () 
	{
		if (Input.GetMouseButton (1)) {
			if (firstFrame) {
				firstFrame = false;
				mousePositionSecond = Input.mousePosition;
				mousePositionSecond = Camera.main.ScreenToWorldPoint (mousePositionSecond);
			} else {
				mousePositionFirst = mousePositionSecond;
				mousePositionSecond = Input.mousePosition;
				mousePositionSecond = Camera.main.ScreenToWorldPoint (mousePositionSecond);
				transformVector = new Vector3 (mousePositionFirst.x - mousePositionSecond.x, mousePositionFirst.y - mousePositionSecond.y, -10f);
				transform.position = new Vector3 (transform.position.x + transformVector.x * moveSpeed, transform.position.y + transformVector.y * moveSpeed, transformVector.z);
			}
		} else
			firstFrame = true;

	}
}

