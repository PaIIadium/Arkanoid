using UnityEngine;

public struct Collision
{
    public Vector2 ballPosition;
    public Vector2 point;
    public GameObject gameObject;

    public static Collision defaultCollision = new Collision
    {
        ballPosition = Vector2.negativeInfinity,
        point = Vector2.negativeInfinity,
        gameObject = null
    };
}