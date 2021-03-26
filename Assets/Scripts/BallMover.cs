using UnityEngine;
using UnityEngine.Events;
using Utils;
using Event = Utils.Event;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class BallMover : MonoBehaviour
{
    private Vector2 direction;
    private Vector2 nextDirection;
    private Vector2 nextCollisionPoint;
    private GameObject nextHitGameObject;
    public Vector2 Displacement { get; private set; }
    private UnityEvent<Vector2> moveDirectionChanged = new UnityEvent<Vector2>();
    
    private Vector2 Direction
    {
        set
        {
            direction = value;
            Displacement = CalculateDisplacement(Direction, Config.ballSpeed);
            moveDirectionChanged.Invoke(value);
        }
        get => direction;
    }
    
    private void Start()
    {
        EventEmitter.AddEvent(Event.MoveDirectionChanged, moveDirectionChanged);
        EventEmitter.SubscribeOnEvent(Event.PaddleCollision, OnPaddleCollision);
        Direction = Config.startDirection;
    }
    
    private void FixedUpdate()
    {
        MoveBall();
        CheckCollision();
    }

    private Vector2 CalculateDisplacement(Vector2 dir, float magnitude)
    {
        return dir * (magnitude * Time.fixedDeltaTime);
    }
    
    private void OnPaddleCollision(Vector2 newDirection)
    {
        nextDirection = newDirection;
        BounceOff(true);
    }
    
    private void BounceOff(bool isPaddle)
    {
        if (!isPaddle && nextHitGameObject.CompareTag("Block")) Destroy(nextHitGameObject);
        nextHitGameObject = null;
        nextCollisionPoint = Vector2.negativeInfinity;
        Direction = nextDirection;
    }

    private void MoveBall()
    {
        var position = transform.position;
        var newPosition = new Vector3(Displacement.x + position.x,
            Displacement.y + position.y, position.z);
        transform.position = newPosition;
    }
    
    private void CheckCollision()
    {
        if (!IsCollide(nextCollisionPoint)) return;
        BounceOff(false);
    }

    public void OnHit(Vector2 newDirection, Vector2 collisionPoint, GameObject hitGameObject)
    {
        nextDirection = newDirection;
        nextHitGameObject = hitGameObject;
        nextCollisionPoint = collisionPoint;

        if (IsCollide(collisionPoint)) BounceOff(false);
    }
    
    private bool IsCollide(Vector2 collisionPoint)
    {
        var sqrDistance = ((Vector2) transform.position - collisionPoint).sqrMagnitude;
        return sqrDistance < Config.sqrBallRadius;
    }
}
