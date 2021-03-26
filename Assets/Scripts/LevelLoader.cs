using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour 
{
    void Start()
    {
        var lines = LinesReader.GetLines();
        var linePrefab = LoadBoundLinePrefab();
        InstantiateLines(lines, linePrefab);
    }

    private GameObject LoadBoundLinePrefab()
    {
        var boundLine = Resources.Load<GameObject>("BoundLine");
        return boundLine;
    }

    private void InstantiateLines(List<Line> lines, GameObject linePrefab)
    {
        foreach (var line in lines)
        {
            var lineRenderer = Instantiate(linePrefab, Vector3.zero, Quaternion.identity).GetComponent<LineRenderer>();
            var coordinates = new[]
            {
                new Vector3(line.start.x, line.start.y, 0), 
                new Vector3(line.end.x, line.end.y, 0)
            };
            lineRenderer.SetPositions(coordinates);
            SetEdgeColliders(lineRenderer, coordinates);
        }
    }

    private void SetEdgeColliders(LineRenderer lineRenderer, Vector3[] coordinates)
    {
        var halfLineWidth = lineRenderer.startWidth * 0.5f;
        Vector2 point1 = coordinates[0];
        Vector2 point2 = coordinates[1];
        var offset = Vector2.Perpendicular(point2 - point1).normalized * halfLineWidth;
        
        var colliderPoint1 = point1 + offset;
        var colliderPoint2 = point2 + offset;
        AddCollider(lineRenderer.gameObject, colliderPoint1, colliderPoint2);

        var colliderPoint3 = point1 - offset;
        var colliderPoint4 = point2 - offset;
        AddCollider(lineRenderer.gameObject, colliderPoint3, colliderPoint4);

        AddCollider(lineRenderer.gameObject, colliderPoint1, colliderPoint3);
        AddCollider(lineRenderer.gameObject, colliderPoint2, colliderPoint4);

    }

    private void AddCollider(GameObject line, Vector2 point1, Vector2 point2)
    {
        var edgeCollider = line.AddComponent<EdgeCollider2D>();
        edgeCollider.SetPoints(new List<Vector2> {point1, point2});
    }
}
