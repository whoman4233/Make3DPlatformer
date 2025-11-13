using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Bounce : MonoBehaviour
{
    [Header("Bounce Settings")]
    [Tooltip("점프 세기 (Impulse force magnitude)")]
    public float force = 15f;

    [Tooltip("Force 방향: 항상 위로 쏠지 여부")]
    public bool alwaysUp = true;

    [Tooltip("Force 적용시 마찰, 튀김에 영향받지 않도록 트리거로 사용할지")]
    public bool useTrigger = true;

    private void Reset()
    {
        // 트리거로 사용할 경우 자동 설정
        GetComponent<Collider>().isTrigger = useTrigger;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!useTrigger) return;
        ApplyImpulse(other.attachedRigidbody, transform.up);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (useTrigger) return;
        Rigidbody rb = collision.rigidbody;
        if (rb == null) return;

        // 충돌면 법선 방향
        Vector3 dir = alwaysUp ? transform.up : collision.contacts[0].normal;
        ApplyImpulse(rb, dir);
    }

    private void ApplyImpulse(Rigidbody rb, Vector3 dir)
    {
        if (rb == null || rb.isKinematic) return;

        dir.Normalize();

        // 수직성 강조 (아래로 내려가던 중이면 Y속도 0 보정)
        Vector3 v = rb.velocity;
        if (Vector3.Dot(v, dir) < 0f)
            rb.velocity = Vector3.zero;

        rb.AddForce(dir * force, ForceMode.Impulse);
    }
}
