using System;
using System.Collections.Generic;
using System.Linq;
using Structs;
using UnityEngine;
using Collision = Structs.Collision;

namespace Utils
{
    public static class LinAlg
    { 
        public static Collision GetCollision(RaycastHit2D[] hits, Vector2 moveDirection, Vector2 transformPosition)
        {
            var edgeColliders = GetAllEdgeColliders(hits);
            var collisions = GetAllCollisions(edgeColliders, moveDirection, transformPosition);
            
            return collisions.Count == 0 ? Collision.DefaultCollision 
                : ChooseClosestCollision(collisions, transformPosition);
        }

        private static HashSet<EdgeCollider2D> GetAllEdgeColliders(RaycastHit2D[] hits)
        {
            var edgeColliders = new HashSet<EdgeCollider2D>();
            foreach (var hit in hits)
            {
                if (hit.distance == 0) continue;
                var colliders = hit.collider.gameObject.GetComponents<EdgeCollider2D>();
                foreach (var collider in colliders) edgeColliders.Add(collider);
            }

            return edgeColliders;
        }
        
        private static List<Collision> GetAllCollisions(HashSet<EdgeCollider2D> colliders, Vector2 moveDirection, Vector2 transformPosition)
        {
            var collisions = new List<Collision>();
            foreach (var collider in colliders)
            {
                var lineEnds = collider.points;
                lineEnds[0] += (Vector2) collider.gameObject.transform.position;
                lineEnds[1] += (Vector2) collider.gameObject.transform.position;
                var colliderLine = new Line
                {
                    Start = new Vector2(lineEnds[0].x, lineEnds[0].y),
                    End = new Vector2(lineEnds[1].x, lineEnds[1].y)
                };
                var collision = CalculateCollision(colliderLine, moveDirection, transformPosition, collider.gameObject);
                if (!collision.Equals(Collision.DefaultCollision)) collisions.Add(collision);
            }

            return collisions;
        }
        
        private static Collision ChooseClosestCollision(List<Collision> collisions, Vector2 transformPosition)
        {
            var closestCollision = collisions[0];
            var closestSqrDistance = Vector2.SqrMagnitude(transformPosition - collisions[0].BallPosition);
            foreach (var collision in collisions.Skip(1))
            {
                var sqrDistance = Vector2.SqrMagnitude(transformPosition - collision.BallPosition);
                if (sqrDistance > closestSqrDistance) continue;
                closestCollision = collision;
                closestSqrDistance = sqrDistance;
            }

            return closestCollision;
        }
        
        private static Collision CalculateCollision(Line line, Vector2 moveDirection, Vector2 transformPosition, GameObject go)
        {
            // So many variables to avoid repeated calculations
            
            var x3 = transformPosition.x;
            var y3 = transformPosition.y;
            var forwardPoint = transformPosition + moveDirection; // some point on ball trajectory
            var x4 = forwardPoint.x;
            var y4 = forwardPoint.y;

            var a = line.End.y - line.Start.y; // coefficients of the straight line of collider
            var b = line.Start.x - line.End.x;
            var c = line.Start.y * line.End.x - line.Start.x * line.End.y;

            var q = a * a + b * b;
            var s = y3 - y4;
            var u = x3 - x4;
            var w = x3 * y4 - x4 * y3;
            
            var denominator = u * a + s * b;
            var h = a * w;
            var e = Config.BallRadius * Mathf.Sqrt(q);

            var y5_1 = (h + (e - c) * s) / denominator; // ball position in first collision
            var x5_1 = u * (y5_1 - y4) / s + x4;
            var ballPosition1 = new Vector2(x5_1, y5_1);

            var y5_2 = (-h + (e + c) * s) / -denominator; // ball position in second collision
            var x5_2 = u * (y5_2 - y4) / s + x4;
            var ballPosition2 = new Vector2(x5_2, y5_2);

            var collisionPoint1 = CalculateCollisionPoint(ballPosition1, a, b, c, q);
            var collisionPoint2 = CalculateCollisionPoint(ballPosition2, a, b, c, q);

            var collision = Vector2.Dot(moveDirection, collisionPoint1 - ballPosition1) > 0 ? 
                new Collision {BallPosition = ballPosition1, Point = collisionPoint1, GameObject = go} : 
                new Collision {BallPosition = ballPosition2, Point = collisionPoint2, GameObject = go};
            
            var correctedCollisionPoint = CheckLineEnds(collision.Point, line.Start, line.End); 
            if (correctedCollisionPoint == collision.Point) return collision;
            
            collision.Point = correctedCollisionPoint;
            // Coefficients of the straight line of ball trajectory:
            // a = -s; b = u; c = -w;
            collision.BallPosition = CorrectBallPosition(-s, u, -w, correctedCollisionPoint, transformPosition);
            
            if (collision.BallPosition == Vector2.negativeInfinity) 
                return Collision.DefaultCollision;
            if (Vector2.Dot(moveDirection, collision.Point - collision.BallPosition) < 0) 
                return Collision.DefaultCollision;
            return collision;
        }

        private static Vector2 CalculateCollisionPoint(Vector2 ballPosition, float a, float b, float c, float q)
        {
            var k = b * ballPosition.x;
            var m = a * ballPosition.y;
            var t = k - m;

            var x = (b * t - a * c) / q;
            var y = (a * -t - b * c) / q;
            return new Vector2(x, y);
        }
        
        // Checking if collision point is between line segment ends. Choosing closest end if not
        private static Vector2 CheckLineEnds(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            if (IsFloatGreater(point.x, lineStart.x) && IsFloatGreater(point.x, lineEnd.x))
            {
                return IsFloatGreater(lineStart.x, lineEnd.x) ? lineStart : lineEnd;
            }

            if (IsFloatGreater(lineStart.x, point.x) && IsFloatGreater(lineEnd.x, point.x))
            {
                return IsFloatGreater(lineStart.x, lineEnd.x) ? lineEnd : lineStart;
            }

            if (!AreFloatsEqual(lineStart.x, lineEnd.x)) return point;
        
            if (IsFloatGreater(point.y, lineStart.y) && IsFloatGreater(point.y, lineEnd.y))
            {
                return IsFloatGreater(lineStart.y, lineEnd.y) ? lineStart : lineEnd;
            }

            if (IsFloatGreater(point.y, lineStart.y) || IsFloatGreater(point.y, lineEnd.y)) return point;
            return IsFloatGreater(lineStart.y, lineEnd.y) ? lineEnd : lineStart;
        }

        // Find ball center position in collision point by straight line coefficients (a, b, c) of ball trajectory and 
        // current ball position
        private static Vector2 CorrectBallPosition(float a, float b, float c, Vector2 collisionPoint, Vector2 transformPosition)
        {
            var x3 = collisionPoint.x;
            var y3 = collisionPoint.y;
            var a1 = b * b + a * a; // coefficients of quadratic equation
            var b1 = 2 * b * c + 2 * a * x3 * b - 2 * a * a * y3;
            var c1 = c * c + 2 * a * x3 * c + a * a * x3 * x3 + a * a * y3 * y3 - a * a * Config.SqrBallRadius;
            var (y1, y2) = SolveQuadraticEquation(a1, b1, c1);
            if (y1 is float.NaN) return Vector2.negativeInfinity;
            var x1 = (-b * y1 - c) / a;
            var x2 = (-b * y2 - c) / a;
            var point1 = new Vector2(x1, y1);
            var point2 = new Vector2(x2, y2);
            var sqrDistance1 = (point1 - transformPosition).SqrMagnitude();
            var sqrDistance2 = (point2 - transformPosition).SqrMagnitude();
            return sqrDistance1 < sqrDistance2 ? point1 : point2;
        }

        // Comparing floats considering tolerance in Config
        private static bool IsFloatGreater(float num1, float num2)
        {
            if (AreFloatsEqual(num1, num2)) return false;
            return num1 > num2;
        }

        private static bool AreFloatsEqual(float num1, float num2)
        {
            return Mathf.Abs(num2 - num1) < Config.FloatTolerance;
        }
        
        private static (float, float) SolveQuadraticEquation(float a, float b, float c)
        {
            var discriminant = b * b - 4 * a * c;
            if (discriminant < 0) return (Single.NaN, Single.NaN);
            var sqrtDiscriminant = Mathf.Sqrt(discriminant);
            var doubleA = 2 * a;
            var x1 = (-b + sqrtDiscriminant) / doubleA;
            var x2 = (-b - sqrtDiscriminant) / doubleA;
            return (x1, x2);
        }
        
        public static Vector2 CalculateNewDirection(Collision collision, Vector2 moveDirection)
        {
            var normal = CalculateNormalVector(collision, moveDirection);
            var oppositeMoveDirection = -moveDirection;
            var cosine = Vector2.Dot(normal, oppositeMoveDirection);
            var reducedNormal = normal * cosine;
            var verticalVector = reducedNormal - oppositeMoveDirection;
            var impulse = verticalVector * 2;
            return oppositeMoveDirection + impulse;
        }

        private static Vector2 CalculateNormalVector(Collision collision, Vector2 moveDirection)
        {
            var normalVector = (collision.Point - collision.BallPosition).normalized;
            if (Vector2.Dot(normalVector, moveDirection) < 0) normalVector *= -1;
            Debug.DrawLine(collision.Point - normalVector, collision.Point, Color.blue);
            return normalVector;
        }
    }
}
