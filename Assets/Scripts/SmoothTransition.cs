using UnityEngine;
using System.Collections;

//Class for representing smooth movement, linearly interpolates position on in time
class SmoothTransition
{
    public float totalDuration;
    public float animationTimeLeft;
    public Coords2 initialCoords;
    public Coords2 finalCoords;

    public SmoothTransition(Coords2 _initialCoords, Coords2 _finalCoords, float _totalDuration)
    {
        totalDuration = _totalDuration;
        animationTimeLeft = _totalDuration;

        initialCoords = _initialCoords;
        finalCoords = _finalCoords;
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

        float x = initialCoords.x + (finalCoords.x - initialCoords.x) * t;
        float y = initialCoords.y + (finalCoords.y - initialCoords.y) * t;
        float z = 0.0f;

        return new Vector3(x, y, z);
    }
}
