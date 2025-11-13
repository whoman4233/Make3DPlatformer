using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    private Vector2 curMovementInput;
    public float jumpPower = 6f;
    public LayerMask groundLayerMask;

    [Header("Look (3rd Person)")]
    public Transform cameraContainer;   // 피벗(=CameraContainer)
    public float minXLook = -45f;
    public float maxXLook = 70f;
    public float lookSensivility = 0.1f;
    public bool canLook = true;

    // 추가: 회전 스무딩
    public float rotateTowardMoveDirSpeed = 12f;

    private float camPitch;             // 위/아래(피치)
    private float yaw;                  // 좌/우(요) — 피벗의 Y 회전
    private Vector2 mouseDelta;

    private Rigidbody rigidBody;
    public Action inventory;

    private PlayerCondition condition;
    public int jumpStamina = 10;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        condition = GetComponent<PlayerCondition>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        // 시작 시 현재 방향을 요로 초기화
        yaw = transform.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (canLook) CameraLook();
    }

    void FixedUpdate()
    {
        Move();
    }

    // === 3인칭용 이동 ===
    void Move()
    {
        // 카메라 기준 평면 방향
        Vector3 fwd = cameraContainer.forward;
        fwd.y = 0f;
        fwd.Normalize();

        Vector3 right = cameraContainer.right;
        right.y = 0f;
        right.Normalize();

        Vector3 moveDir = (fwd * curMovementInput.y + right * curMovementInput.x);
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        Vector3 vel = moveDir * moveSpeed;
        vel.y = rigidBody.velocity.y;
        rigidBody.velocity = vel;

        // 이동 입력이 있을 때 캐릭터를 이동 방향으로 회전
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion target = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, rotateTowardMoveDirSpeed * Time.deltaTime);
        }
    }

    // === 3인칭용 카메라 회전: 피벗만 회전 ===
    void CameraLook()
    {
        // 마우스 누적
        yaw += mouseDelta.x * lookSensivility;            // 좌우
        camPitch += mouseDelta.y * lookSensivility;       // 위아래
        camPitch = Mathf.Clamp(camPitch, minXLook, maxXLook);

        // 피벗 Yaw는 월드 Y, Pitch는 로컬 X
        cameraContainer.rotation = Quaternion.Euler(0f, yaw, 0f);
        cameraContainer.localEulerAngles = new Vector3(-camPitch, cameraContainer.localEulerAngles.y, 0f);
        // ※ 위 두 줄은: 월드 Yaw 적용 후, 로컬 피치로 고정하려는 의도.
        // 씬 상황에 따라 하나로 합쳐도 됩니다:
        // cameraContainer.eulerAngles = new Vector3(-camPitch, yaw, 0f);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            curMovementInput = context.ReadValue<Vector2>();
        else if (context.phase == InputActionPhase.Canceled)
            curMovementInput = Vector2.zero;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && IsGrounded() && condition.UseStamina(jumpStamina))
            rigidBody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
    }

    // === 기존 IsGrounded 그대로 사용 ===
    bool IsGrounded()
    {
        // 살짝 위에서부터 쏘고, 길이는 살짝 짧게
        float rayLength = 0.6f;
        Vector3 startOffset = transform.up * 0.2f; // 0.01f → 0.2f 정도로 높여서 바닥 뚫지 않게

        Ray[] rays = new Ray[4]
        {
        new Ray(transform.position + (transform.forward * 0.2f) + startOffset, Vector3.down),
        new Ray(transform.position + (-transform.forward * 0.2f) + startOffset, Vector3.down),
        new Ray(transform.position + (transform.right * 0.2f) + startOffset, Vector3.down),
        new Ray(transform.position + (-transform.right * 0.2f) + startOffset, Vector3.down)
        };

        for (int i = 0; i < rays.Length; i++)
        {
            Debug.DrawRay(rays[i].origin, rays[i].direction * rayLength, Color.red, 2f);
            if (Physics.Raycast(rays[i], rayLength, groundLayerMask))
                return true;
        }

        return false;
    }


    public void OnInventory(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            inventory?.Invoke();
            ToggleCursor();
        }
    }

    void ToggleCursor()
    {
        bool toggle = Cursor.lockState == CursorLockMode.Locked;
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
