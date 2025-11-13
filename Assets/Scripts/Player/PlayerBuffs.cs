using System.Collections;
using UnityEngine;

public class PlayerBuffs : MonoBehaviour
{
    [SerializeField] private PlayerController controller;

    private Coroutine speedCoro;
    private Coroutine jumpCoro;

    private float baseMoveSpeed;
    private float baseJumpPower;

    void Awake()
    {
        if (controller == null) controller = GetComponent<PlayerController>();
        baseMoveSpeed = controller.moveSpeed;
        baseJumpPower = controller.jumpPower;
    }

    // 장비 등으로 기본치가 바뀌는 게임이면 필요할 때 호출해 갱신
    public void RebindBase(float? moveSpeed = null, float? jumpPower = null)
    {
        if (moveSpeed.HasValue) baseMoveSpeed = moveSpeed.Value;
        if (jumpPower.HasValue) baseJumpPower = jumpPower.Value;
    }

    public void ApplySpeedBoost(float multiplier, float duration)
    {
        if (speedCoro != null) StopCoroutine(speedCoro);
        speedCoro = StartCoroutine(SpeedBoostRoutine(multiplier, duration));
    }

    public void ApplyJumpBoost(float multiplier, float duration)
    {
        if (jumpCoro != null) StopCoroutine(jumpCoro);
        jumpCoro = StartCoroutine(JumpBoostRoutine(multiplier, duration));
    }

    private IEnumerator SpeedBoostRoutine(float mult, float duration)
    {
        controller.moveSpeed = baseMoveSpeed * mult;
        yield return WaitSeconds(duration);
        controller.moveSpeed = baseMoveSpeed;
        speedCoro = null;
    }

    private IEnumerator JumpBoostRoutine(float mult, float duration)
    {
        controller.jumpPower = baseJumpPower * mult;
        yield return WaitSeconds(duration);
        controller.jumpPower = baseJumpPower;
        jumpCoro = null;
    }

    private static IEnumerator WaitSeconds(float s)
    {
        float t = 0f;
        while (t < s) { t += Time.deltaTime; yield return null; }
    }
}
