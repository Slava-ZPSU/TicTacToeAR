using UnityEngine;
using System.Collections.Generic;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

public class Board : MonoBehaviour
{
    [SerializeField] private GameObject xPrefab;
    [SerializeField] private GameObject oPrefab;
    [SerializeField] private float cellSize = 0.1f; // Розмір однієї клітинки
    [SerializeField] private float placementYOffset = 0.01f; // Щоб фігурки не злипалися з поверхнею

    private GameObject[,] grid = new GameObject[3, 3];
    private bool isXTurn = true;

    private void OnEnable()
    {
        EnhancedTouch.EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerDown += OnFingerDown;
    }

    private void OnDisable()
    {
        EnhancedTouch.Touch.onFingerDown -= OnFingerDown;
        EnhancedTouch.EnhancedTouchSupport.Disable();
    }

    private void OnFingerDown(EnhancedTouch.Finger finger)
    {
        Vector2 screenPos = finger.currentTouch.screenPosition;
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                Vector3 localHit = transform.InverseTransformPoint(hit.point);
                int x = Mathf.FloorToInt((localHit.x + 1.5f * cellSize) / cellSize);
                int y = Mathf.FloorToInt((localHit.z + 1.5f * cellSize) / cellSize);

                if (IsValidCell(x, y))
                {
                    PlaceMark(x, y);
                }
            }
        }
    }

    private bool IsValidCell(int x, int y)
    {
        return x >= 0 && x < 3 && y >= 0 && y < 3 && grid[x, y] == null;
    }

    private void PlaceMark(int x, int y)
    {
        GameObject markPrefab = isXTurn ? xPrefab : oPrefab;
        Vector3 localPos = new Vector3((x - 1) * cellSize, placementYOffset, (y - 1) * cellSize);
        Vector3 worldPos = transform.TransformPoint(localPos);

        GameObject mark = Instantiate(markPrefab, worldPos, Quaternion.identity, transform);
        mark.transform.localScale = Vector3.one;
        grid[x, y] = mark;

        isXTurn = !isXTurn;
    }
}
