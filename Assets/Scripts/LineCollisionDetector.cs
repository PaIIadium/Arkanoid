using UnityEngine;
using Utils;
using Collision = Structs.Collision;
using Event = Utils.Event;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class LineCollisionDetector : MonoBehaviour
{
    private Vector2 rayDirection;
    private Vector3 rayPointOffset;
    private Vector2 rayPointDisplacement;

    private void Awake()
    {
        EventEmitter.SubscribeOnEvent(Event.MoveDirectionChanged, OnMoveDirectionChanged);
    }
    
    private void OnMoveDirectionChanged(Vector2 direction)
    {
        rayDirection = direction;
        var normalVector = Vector2.Perpendicular(direction);
        rayPointOffset = normalVector * Config.BallRadius;
        rayPointDisplacement = direction * (Config.BallSpeed * Time.fixedDeltaTime);
        CheckCollision();
    }

    private void CheckCollision()
    {
        var hits = GetHits();
        var collision = LinAlg.GetCollision(hits, rayDirection, transform.position);
        
        if (collision.Equals(Collision.DefaultCollision)) return;
        Debug.DrawLine(collision.BallPosition, collision.BallPosition + Vector2.up * 0.1f, Color.cyan);
        var newDirection = LinAlg.CalculateNewDirection(collision, rayDirection);
        Debug.DrawLine(collision.Point - rayDirection, collision.Point, Color.green);
        Debug.DrawLine(collision.Point, collision.Point + newDirection, Color.green);
        EventEmitter.LineCollision.Invoke(newDirection, collision.Point, collision.GameObject);
    }
    
    private RaycastHit2D[] GetHits()
    {
        var ray1Origin = transform.position + rayPointOffset + (Vector3) rayPointDisplacement;
        var ray2Origin = transform.position - rayPointOffset + (Vector3) rayPointDisplacement;
        Debug.DrawRay(ray1Origin, rayDirection * Config.RayLength, Color.red);
        Debug.DrawRay(ray2Origin, rayDirection * Config.RayLength, Color.red);
        var hitInfo1 = Physics2D.Raycast(ray1Origin, rayDirection, Config.RayLength, 1 << 8);
        var hitInfo2 = Physics2D.Raycast(ray2Origin, rayDirection, Config.RayLength, 1 << 8);
        return new [] {hitInfo1, hitInfo2};
    }
}
