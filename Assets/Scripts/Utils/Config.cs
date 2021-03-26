using UnityEngine;

namespace Utils
{
    public static class Config
    {
        private static float startAngle = Mathf.PI * 0.75f;
        public static Vector2 startDirection = new Vector2(Mathf.Cos(startAngle), Mathf.Sin(startAngle));
    
        public static float ballSpeed = 7f;
        public static float ballRadius = 0.18f;
        public static float sqrBallRadius = ballRadius * ballRadius + 0.008f;
    
        public static float rayLength = 15f;
    
        public static float floatTolerance = 0.0001f;
    }
}