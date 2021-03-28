using UnityEngine;

namespace Utils
{
    public static class Config
    {
        private const float startAngle = Mathf.PI * 0.75f;
        public static Vector2 StartDirection = new Vector2(Mathf.Cos(startAngle), Mathf.Sin(startAngle));
    
        public const float BallSpeed = 7f;
        public const float BallRadius = 0.18f;
        public const float SqrBallRadius = BallRadius * BallRadius + 0.008f;
    
        public const float RayLength = 15f;
    
        public const float FloatTolerance = 0.0001f;
    }
}