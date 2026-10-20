using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class DungeonHudPointer : MonoBehaviour
{
    [SerializeField] private GraphicRaycaster raycaster;
    [SerializeField] private EventSystem eventSystem;
    private readonly List<RaycastResult> hits = new List<RaycastResult>();

    public void Configure(GraphicRaycaster uiRaycaster, EventSystem uiEventSystem)
    {
        raycaster = uiRaycaster;
        eventSystem = uiEventSystem;
    }

    private void Update()
    {
        if (Mouse.current == null || eventSystem == null || raycaster == null || Cursor.lockState != CursorLockMode.None ||
            !Mouse.current.leftButton.wasPressedThisFrame) return;
        var pointer = new PointerEventData(eventSystem) { position = Mouse.current.position.ReadValue() };
        hits.Clear();
        raycaster.Raycast(pointer, hits);
        foreach (var hit in hits)
        {
            var button = hit.gameObject.GetComponentInParent<Button>();
            if (button != null && button.interactable)
            {
                button.onClick.Invoke();
                break;
            }
        }
    }
}
