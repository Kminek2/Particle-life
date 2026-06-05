using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControlls : MonoBehaviour
{
    [SerializeField] ControllsSettings _settings;
    [SerializeField] InputActionReference _movement;
    [SerializeField] InputActionReference _zoomAction;

    float _zoom = 10;

    void Update()
    {
        if (_movement.action.IsInProgress())
            Move();
        if (_zoomAction.action.IsInProgress())
            Zoom();
    }

    private void Move()
    {
        Vector3 movementInput = _movement.action.ReadValue<Vector2>();
        float cameraSpeedSetting = _settings.cameraSpeed;
        float speedMult = _zoom * Time.deltaTime;
        transform.position += movementInput * cameraSpeedSetting * speedMult;
    }

    private void Zoom()
    {
        float zoom = _zoom - _zoomAction.action.ReadValue<Vector2>().y;
        _zoom = Mathf.Clamp(zoom, 1, 200);
        transform.localScale = Vector3.one * _zoom / 10;
        Camera.main.orthographicSize = _zoom * 10;
    }
}
