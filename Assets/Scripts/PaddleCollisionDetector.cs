using UnityEngine;
using UnityEngine.Events;
using Utils;
using Event = Utils.Event;

public class PaddleCollisionDetector : MonoBehaviour
{
    private Transform ballTransform;
    private PaddleMover paddleMover;
    private BoxCollider2D boxCollider;
    private Vector2 ballDirection;
    
    private bool ballTouchPaddle;

    private Vector3 previousBallPosition;
    private Vector3 previousClosestPoint;
    
    private UnityEvent<Vector2> paddleCollision = new UnityEvent<Vector2>();

    private void Awake()
    {
        EventEmitter.AddEvent(Event.PaddleCollision, paddleCollision);
        EventEmitter.SubscribeOnEvent(Event.MoveDirectionChanged, OnMoveDirectionChanged);
    }

    private void Start()
    {
        paddleMover = gameObject.GetComponentInChildren<PaddleMover>();
        var ballMover = FindObjectOfType<BallMover>();
        ballTransform = ballMover.transform;
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void OnMoveDirectionChanged(Vector2 newDirection)
    {
        ballDirection = newDirection;
    }
    
    private void FixedUpdate()
    {
        CheckCollision();
    }
    
    private void CheckCollision()
    {
        var closestPoint = boxCollider.ClosestPoint(ballTransform.position);
        var sqrDistance = Vector2.SqrMagnitude((Vector2) ballTransform.position - closestPoint);
        if (sqrDistance > Config.sqrBallRadius)
        {
            previousBallPosition = ballTransform.position;
            previousClosestPoint = closestPoint;
            ballTouchPaddle = false;
            return;
        }
        
        if (ballTouchPaddle) return;

        Collision collision = GetCollision(closestPoint);
        var newDirection = LinAlg.CalculateNewDirection(collision, ballDirection);
        newDirection = (newDirection + paddleMover.PaddleDirection).normalized;
        ballTouchPaddle = true;

        paddleCollision.Invoke(newDirection);
    }

    private Collision GetCollision(Vector2 closestPoint)
    {
        Collision collision;
        var ballInsidePaddle = boxCollider.bounds.Contains(ballTransform.position);
        if (ballInsidePaddle)
        {
            collision = new Collision
            {
                ballPosition = previousBallPosition,
                point = previousClosestPoint,
                gameObject = gameObject
            };
        }
        else
        {
            collision = new Collision
            {
                ballPosition = ballTransform.position,
                point = closestPoint,
                gameObject = gameObject
            };
        }

        return collision;
    }
}
