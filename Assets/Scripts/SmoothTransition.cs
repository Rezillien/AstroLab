using UnityEngine;
using System.Collections;

//Class for representing smooth movement, linearly interpolates position on in time
class SmoothTransition
{
    public float totalDuration;
    public float animationTimeLeft;
    public int initialX;
    public int initialY;
    public int finalX;
    public int finalY;

    public SmoothTransition(int ix, int iy, int fx, int fy, float total)
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
