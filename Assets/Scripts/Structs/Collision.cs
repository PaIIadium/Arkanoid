using UnityEngine;

namespace Structs
{
    public struct Collision
    {
        public Vector2 BallPosition;
        public Vector2 Point;
        public GameObject GameObject;

        public static Collision DefaultCollision = new Collision
        {
            BallPosition = Vector2.negativeInfinity,
            Point = Vector2.negativeInfinity,
            GameObject = null
        };
    }
}