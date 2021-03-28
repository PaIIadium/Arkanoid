using UnityEngine;
using UnityEngine.Events;
using Utils;
using Collision = Structs.Collision;
using Event = Utils.Event;

public class PaddleCollisionDetector : MonoBehaviour
{
    private Transform ballTransform;
    private PaddleMover paddleMover;
    private BoxCollider2D boxCollider;
    private Vector2 ballDirection;
    
    private bool ballTouchingPaddle;

    private Vector3 previousBallPosition;
    private Vector3 previousClosestPoint;
    
    private readonly UnityEvent<Vector2> paddleCollision = new UnityEvent<Vector2>();

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
        if (sqrDistance > Config.SqrBallRadius)
        {
            previousBallPosition = ballTransform.position;
            previousClosestPoint = closestPoint;
            ballTouchingPaddle = false;
            return;
        }
        
        if (ballTouchingPaddle) return;

        var collision = GetCollision(closestPoint);
        var newDirection = LinAlg.CalculateNewDirection(collision, ballDirection);
        newDirection = (newDirection + paddleMover.PaddleDirection).normalized;
        ballTouchingPaddle = true;

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
                BallPosition = previousBallPosition,
                Point = previousClosestPoint,
                GameObject = gameObject
            };
        }
        else
        {
            collision = new Collision
            {
                BallPosition = ballTransform.position,
                Point = closestPoint,
                GameObject = gameObject
            };
        }

        return collision;
    }
}
