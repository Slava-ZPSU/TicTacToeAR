using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
public class PlaceBoard : MonoBehaviour
{
    [SerializeField] private GameObject boardPrefab;
    [SerializeField] private BoardMode currentMode = BoardMode.None;

    public BoardMode CurrentMode => currentMode;
    private List<GameObject> placedBoards = new List<GameObject>();

    private ARRaycastManager aRRaycastManager;
    private ARPlaneManager aRPlaneManager;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private GameObject selectedBoard = null;
    public bool IsBoardSelected => selectedBoard != null;
    private GameObject selectedHighlight = null;

    private float initialPinchDistance;
    private Vector3 initialScale;

    public void SetNoneMode() {
        currentMode = BoardMode.None;
        DeselectBoard();
    }

    public void SetAddMode() {
        currentMode = BoardMode.AddBoard;
        DeselectBoard();
    }

    public void SetEditMode() => currentMode = BoardMode.EditBoard;

    private void Awake() {
        aRRaycastManager = GetComponent<ARRaycastManager>();
        aRPlaneManager = GetComponent<ARPlaneManager>();
    }

    private void Update() {
        EditBoard();
    }

    private void OnEnable() {
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();

        EnhancedTouch.Touch.onFingerDown += FingerDown;
    }

    private void OnDisable() {
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();

        EnhancedTouch.Touch.onFingerDown -= FingerDown;
    }

    private void FingerDown(EnhancedTouch.Finger finger) {
        if (IsPointerOverUI(finger.currentTouch.screenPosition)) return;
        if (finger.index != 0) return;
        if (!aRRaycastManager.Raycast(finger.currentTouch.screenPosition, hits, TrackableType.PlaneWithinPolygon)) return;

        Pose pose = hits[0].pose;

        if (currentMode == BoardMode.AddBoard) {
            AddBoards(pose);
        }

        if (currentMode == BoardMode.EditBoard) {
            TrySelectBoard(finger.currentTouch.screenPosition);
        }
    }

    private void AddBoards(Pose pose) {
        foreach (var board in placedBoards) {
            float distance = Vector3.Distance(board.transform.position, pose.position);
            if (distance < 0.5f) {
                pose.position += (pose.position - board.transform.position).normalized * 0.5f;
            }
        }

        GameObject obj = Instantiate(boardPrefab, pose.position, pose.rotation);
        placedBoards.Add(obj);
        AlignBoardToCamera(obj, hits[0].trackableId);
    }

    private void AlignBoardToCamera(GameObject obj, TrackableId planeId) {
        if (aRPlaneManager.GetPlane(planeId).alignment != PlaneAlignment.HorizontalUp) return;

        Vector3 pos = obj.transform.position;
        Vector3 camPos = Camera.main.transform.position;
        Vector3 direction = camPos - pos;

        Vector3 targetRotationEuler = Quaternion.LookRotation(direction).eulerAngles;
        Vector3 scaledEuler = Vector3.Scale(targetRotationEuler, obj.transform.up.normalized);
        Quaternion targetRotation = Quaternion.Euler(scaledEuler);
        obj.transform.rotation = obj.transform.rotation * targetRotation;
    }

    private void TrySelectBoard(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (placedBoards.Contains(hit.collider.gameObject))
            {
                DeselectBoard(); // зняти попередню обводку

                selectedBoard = hit.collider.gameObject;

                Transform highlight = selectedBoard.transform.Find("Outline");
                if (highlight != null)
                {
                    highlight.gameObject.SetActive(true);
                    selectedHighlight = highlight.gameObject;
                }

                return;
            }
        }

        // Якщо клік не по дошці — скинути вибір
        DeselectBoard();
    }

    private void EditBoard() {
        if (currentMode != BoardMode.EditBoard || selectedBoard == null) return;

        var touches = EnhancedTouch.Touch.activeTouches;

        if (touches.Count == 1) {
            Vector2 screenPos = touches[0].screenPosition;
            if (aRRaycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon)) {
                selectedBoard.transform.position = hits[0].pose.position;
            }
        }
        else if (touches.Count == 2) {
            var touch1 = touches[0];
            var touch2 = touches[1];

            float currentDistance = Vector2.Distance(touch1.screenPosition, touch2.screenPosition);

            if (initialPinchDistance == 0f) {
                initialPinchDistance = currentDistance;
                initialScale = selectedBoard.transform.localScale;
            } else {
                float scaleFactor = currentDistance / initialPinchDistance;
                selectedBoard.transform.localScale = initialScale * scaleFactor;
            }
        } else {
            initialPinchDistance = 0f;
        }
    }

    public void DeselectBoard()
    {
        if (selectedHighlight != null) {
            selectedHighlight.SetActive(false);
            selectedHighlight = null;
        }
        selectedBoard = null;
    }

    private bool IsPointerOverUI(Vector2 screenPos)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPos
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    public void DeleteSelectBoard() {
        if (selectedBoard == null) return;

        placedBoards.Remove(selectedBoard);
        Destroy(selectedBoard);
        selectedBoard = null;
        selectedHighlight = null;
    }

}

public enum BoardMode {
    None,
    AddBoard,
    EditBoard
}
