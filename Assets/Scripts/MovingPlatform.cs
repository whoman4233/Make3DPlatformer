using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Path")]
    public Transform[] points;          // 이동할 웨이포인트들(월드 좌표)
    public float speed = 3f;            // m/s
    public bool pingPong = true;        // 끝에서 되돌아오기
    public float waitAtPoint = 0.2f;    // 포인트 도착 시 대기

    [Header("Ride Settings")]
    public string playerTag = "Player"; // 플레이어 태그
    [Range(0f, 1f)] public float minAttachNormalY = 0.4f; // 위에서 밟은 접촉만 붙이기

    Rigidbody rb;
    int index = 0;
    int dir = 1;
    bool waiting;

    // velocity/델타는 필요 시 참고용
    public Vector3 Velocity { get; private set; }
    public Vector3 DeltaPosition { get; private set; }   // ✨ 추가
    Vector3 lastPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;          // kinematic으로 MovePosition 사용
        if (points == null || points.Length < 2)
            Debug.LogWarning("[MovingPlatform] 최소 2개 웨이포인트 필요");
    }

    void Start()
    {
        lastPos = rb.position;
    }

    void FixedUpdate()
    {
        if (waiting || points.Length < 2) { UpdateVelocity(); return; }

        Vector3 target = points[index].position;
        Vector3 newPos = Vector3.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        if ((newPos - target).sqrMagnitude < 0.0001f)
            StartCoroutine(NextPointAfter(waitAtPoint));

        UpdateVelocity();
    }

    IEnumerator NextPointAfter(float sec)
    {
        waiting = true;
        yield return new WaitForSeconds(sec);

        // 다음 인덱스
        if (pingPong)
        {
            if (index == points.Length - 1) dir = -1;
            else if (index == 0) dir = 1;
            index += dir;
        }
        else
        {
            index = (index + 1) % points.Length;
        }
        waiting = false;
    }

    void UpdateVelocity()
    {
        Vector3 p = rb.position;
        DeltaPosition = p - lastPos;                             // ✨ 이번 프레임에 얼마나 이동했는지
        Velocity = DeltaPosition / Mathf.Max(Time.fixedDeltaTime, 1e-6f);
        lastPos = p;
    }

    // ===== 플레이어를 위에서 밟으면 부모로 붙이기 =====
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag(playerTag)) return;

        // 접촉면이 '위에서 밟은' 경우만
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y >= minAttachNormalY)
            {
                Attach(collision.transform);
                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag(playerTag))
            Detach(collision.transform);
    }

    // 트리거(발판 위에 얇은 트리거 콜라이더를 추가한 경우)도 지원
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag)) Attach(other.transform);
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag)) Detach(other.transform);
    }

    // 원래 부모 저장 후 붙였다가, 내려오면 복구
    void Attach(Transform t)
    {
        if (t == null) return;
        if (t.GetComponent<_OriginalParent>() == null)
            t.gameObject.AddComponent<_OriginalParent>().value = t.parent;

        t.SetParent(transform, worldPositionStays: true);
    }

    void Detach(Transform t)
    {
        var op = t ? t.GetComponent<_OriginalParent>() : null;
        if (op)
        {
            t.SetParent(op.value, worldPositionStays: true);
            Destroy(op);
        }
    }

    

    // 임시로 원래 부모를 기억하는 내부 컴포넌트
    class _OriginalParent : MonoBehaviour { public Transform value; }
}
