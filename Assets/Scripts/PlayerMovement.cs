using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour
{
    //Class for representing animation, linearly interpolates position on request
    class Animation
    {
        public float totalDuration;
        public float animationTimeLeft;
        public int initialX;
        public int initialY;
        public int finalX;
        public int finalY;

        public Animation(float total, int ix, int iy, int fx, int fy)
        {
            totalDuration = total;
            animationTimeLeft = total;

            initialX = ix;
            initialY = iy;
            finalX = fx;
            finalY = fy;
        }

        public void Update()
        {
            animationTimeLeft -= Time.deltaTime;
        }

        public bool IsFinished()
        {
            return animationTimeLeft < 0.0f;
        }

        //lerp
        public Vector3 GetInterpolatedPosition()
        {
            float animationTime = totalDuration - animationTimeLeft;
            if (animationTime > totalDuration) animationTime = totalDuration;

            float t = animationTime / totalDuration;

            float x = initialX + (finalX - initialX) * t;
            float y = initialY + (finalY - initialY) * t;
            float z = 0.0f;

            return new Vector3(x, y, z);
        }
    }

    // position
    int x;
    int y;

    Animation moveAnimation;

    void Start()
    {
        x = 1;
        y = 1;
        transform.position = new Vector3(x, y, 0);

        //no animation to start with
        moveAnimation = null;
    }

    //inform GameManager that player ended turn
    private void EndTurn()
    {
        GameManager.instance.EndPlayerTurn();
    }

    //returns true on success, starts animation
    private bool TryMove()
    {
        Map map = GameManager.instance.GetMap();

        int dx = 0;
        int dy = 0;

        if (Input.GetKeyDown("up")) dy = 1;
        if (Input.GetKeyDown("left")) dx = -1;
        if (Input.GetKeyDown("down")) dy = -1;
        if (Input.GetKeyDown("right")) dx = 1;

        if ((dx != 0 || dy != 0) && !map.HasCollider(x + dx, y + dy))
        {
            moveAnimation = new Animation(0.5f, x, y, x + dx, y + dy);
            x += dx;
            y += dy;

            return true;
        }

        return false;
    }

    //returns true on success
    private bool TryInteract()
    {
        Map map = GameManager.instance.GetMap();

        int dx = 0;
        int dy = 0;

        if (Input.GetKeyDown("w")) dy = 1;
        if (Input.GetKeyDown("a")) dx = -1;
        if (Input.GetKeyDown("s")) dy = -1;
        if (Input.GetKeyDown("d")) dx = 1;

        if (dx != 0 || dy != 0)
        {
            if (map.Interact(x + dx, y + dy, gameObject))
            {
                return true;
            }
        }

        return false;
    }

    // Update is called once per frame
    // player logic
    void Update()
    {
        GameManager gm = GameManager.instance;
        if (!gm.IsPlayerTurn()) return;

        Map map = gm.GetMap();

        if (moveAnimation == null)
        {
            if (!TryMove())
            {
                if (TryInteract())
                {
                    //if interaction performed some action, end turn
                    EndTurn();
                }
            }
        }
        else
        {
            moveAnimation.Update();

            transform.position = moveAnimation.GetInterpolatedPosition();

            if (moveAnimation.IsFinished())
            {
                moveAnimation = null;
                //end turn only after animation has finished
                EndTurn();
            }
        }
    }
}
