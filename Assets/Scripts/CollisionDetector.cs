using UnityEngine;
using Utils;
using Event = Utils.Event;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class CollisionDetector : MonoBehaviour
{
    private Vector2 rayDirection;
    private BallMover ballMover;
    public Vector3 rayPointOffset;

    private void Awake()
    {
        EventEmitter.SubscribeOnEvent(Event.MoveDirectionChanged, OnMoveDirectionChanged);
        ballMover = gameObject.GetComponent<BallMover>();
    }
    
    private void OnMoveDirectionChanged(Vector2 direction)
    {
        rayDirection = direction;
        var normalVector = Vector2.Perpendicular(direction);
        rayPointOffset = normalVector * Config.ballRadius;
        CheckCollision();
    }

    private void CheckCollision()
    {
        var hits = GetHits();
        var collision = LinAlg.GetCollision(hits, rayDirection, transform.position);
        
        if (collision.Equals(Collision.defaultCollision)) return;
        Debug.DrawLine(collision.ballPosition, collision.ballPosition + Vector2.up * 0.1f, Color.cyan);
        var newDirection = LinAlg.CalculateNewDirection(collision, rayDirection);
        Debug.DrawLine(collision.point - rayDirection, collision.point, Color.green);
        Debug.DrawLine(collision.point, collision.point + newDirection, Color.green);
        ballMover.OnHit(newDirection, collision.point, collision.gameObject);
    }
    
    private RaycastHit2D[] GetHits()
    {
        var ray1Origin = transform.position + rayPointOffset + (Vector3) ballMover.Displacement;
        var ray2Origin = transform.position - rayPointOffset + (Vector3) ballMover.Displacement;
        Debug.DrawRay(ray1Origin, rayDirection * Config.rayLength, Color.red);
        Debug.DrawRay(ray2Origin, rayDirection * Config.rayLength, Color.red);
        var hitInfo1 = Physics2D.Raycast(ray1Origin, rayDirection, Config.rayLength, 1 << 8);
        var hitInfo2 = Physics2D.Raycast(ray2Origin, rayDirection, Config.rayLength, 1 << 8);
        return new [] {hitInfo1, hitInfo2};
    }
}
