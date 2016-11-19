using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    // Use this for initialization
    StartBoardManager board;
    int x;
    int y;
    int framesToNextMove;
    int speed = 15;

    void Start()
    {
        x = 1;
        y = 1;
        framesToNextMove = speed;
        Setup();
    }

    public void Setup()
    {
        board = GameManager.instance.getBoard();
    }



    // Update is called once per frame
    void Update()
    {
        --framesToNextMove;

        if (framesToNextMove > 0)
        {
            return;
        }

        int dx = 0;
        int dy = 0;
        float ha = Input.GetAxis("Horizontal");
        float va = Input.GetAxis("Vertical");
        if (ha > 0.0f) dx = 1;
        else if (ha < 0.0f) dx = -1;
        else if (va > 0.0f) dy = 1;
        else if (va < 0.0f) dy = -1;
        if (board.canMove(x + dx, y + dy))
        {
            x += dx;
            y += dy;
        }

        transform.position = new Vector3(x, y, 0);

        if(dx != 0 || dy != 0)
        {
            framesToNextMove = speed;
        }
    }
}
