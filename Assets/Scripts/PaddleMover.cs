using UnityEngine;

enum Direction { Left, Right }

public class PaddleMover : MonoBehaviour
{
    [SerializeField]
    private float speed = 15f;
    
    private float xCenter;
    private Vector3 displacement;
    private float paddleHalfWidth;
    private (float left, float right) bounds;
    
    public Vector2 PaddleDirection { get; private set; } = Vector2.zero;
    
    private void Start()
    {
        CalculateConstants();
    }
    
    private void FixedUpdate()
    {
        PaddleDirection = Vector2.zero;
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.touchCount == 0) return;
        var position = Input.GetTouch(0).position;

        if (position.x < xCenter) Move(Direction.Left);
        else Move(Direction.Right);
    }

    private void CalculateConstants()
    {
        var screenWidth = Screen.width;
        xCenter = screenWidth * 0.5f;
        displacement = Vector3.right * speed * Time.fixedDeltaTime;
        var lineEnds = new Vector3[2];
        gameObject.GetComponent<LineRenderer>().GetPositions(lineEnds);
        paddleHalfWidth = Mathf.Abs(lineEnds[1].x - lineEnds[0].x) / 2;
        bounds.left = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
        bounds.right = Camera.main.ScreenToWorldPoint(new Vector3(screenWidth, 0, 0)).x;
    }

    private void Move(Direction direction)
    {
        if (direction == Direction.Left)
        {
            if (transform.position.x - displacement.x - paddleHalfWidth < bounds.left) return;
            transform.position -= displacement;
            PaddleDirection = Vector2.left;
        }
        else
        {
            if (transform.position.x + displacement.x + paddleHalfWidth > bounds.right) return;
            transform.position += displacement;
            PaddleDirection = Vector2.right;
        }
    }
}
