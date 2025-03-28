using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenEdgeAligner : MonoBehaviour
{
    [SerializeField] private BoxCollider2D topCollider;
    [SerializeField] private BoxCollider2D botCollider;
    [SerializeField] private BoxCollider2D leftCollider;
    [SerializeField] private BoxCollider2D rightCollider;

    private void Start()
    {
        AlignWithScreen();
    }

    private void AlignWithScreen()
    {
        // Get the screen edges in world coordinates
        Vector3 topLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0));
        Vector3 bottomRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0));

        // Position the top collider
        topCollider.transform.position = new Vector3(0, topLeft.y, 0);
        topCollider.size = new Vector2(bottomRight.x * 2, topCollider.size.y); // Adjust width to match screen width

        // Position the bottom collider
        botCollider.transform.position = new Vector3(0, bottomRight.y, 0);
        botCollider.size = new Vector2(bottomRight.x * 2, botCollider.size.y); // Adjust width to match screen width

        // Position the left collider
        leftCollider.transform.position = new Vector3(topLeft.x, 0, 0);
        leftCollider.size = new Vector2(leftCollider.size.x, topLeft.y * 2); // Adjust height to match screen height

        // Position the right collider
        rightCollider.transform.position = new Vector3(bottomRight.x, 0, 0);
        rightCollider.size = new Vector2(rightCollider.size.x, topLeft.y * 2); // Adjust height to match screen height
    }
}
