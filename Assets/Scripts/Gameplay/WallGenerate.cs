using UnityEngine;

public class CreateXShape : MonoBehaviour
{
    public Vector2 planeSize = new Vector2(200, 100); // Set size of the plane

    void Start()
    {
        CreateDiagonalWall(new Vector2(-planeSize.x / 2, -planeSize.y / 2), new Vector2(planeSize.x / 2, planeSize.y / 2));
        CreateDiagonalWall(new Vector2(-planeSize.x / 2, planeSize.y / 2), new Vector2(planeSize.x / 2, -planeSize.y / 2));
    }

    void CreateDiagonalWall(Vector2 start, Vector2 end)
    {
        GameObject wall = new GameObject("DiagonalWall");
        wall.transform.parent = transform;
        PolygonCollider2D collider = wall.AddComponent<PolygonCollider2D>();
        collider.points = new Vector2[]
        {
            start, new Vector2(end.x, start.y), end, new Vector2(start.x, end.y)
        };
        wall.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Square"); // Load a square sprite
    }
}
