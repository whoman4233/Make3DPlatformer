using UnityEngine;

[DefaultExecutionOrder(100)]   // PlayerController(FixedUpdate) 이후에 실행되게
public class PlatformRider : MonoBehaviour
{
    Rigidbody rb;
    MovingPlatform currentPlatform;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // 플레이어가 현재 어떤 발판 위에 올라가 있다면
        if (currentPlatform != null)
        {
            // 발판이 움직인 만큼 같이 이동
            rb.position += currentPlatform.DeltaPosition;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        var platform = col.collider.GetComponent<MovingPlatform>()
                       ?? col.collider.GetComponentInParent<MovingPlatform>();

        if (platform != null)
            currentPlatform = platform;
    }

    void OnCollisionExit(Collision col)
    {
        var platform = col.collider.GetComponent<MovingPlatform>()
                       ?? col.collider.GetComponentInParent<MovingPlatform>();

        if (platform != null && platform == currentPlatform)
            currentPlatform = null;
    }
}
