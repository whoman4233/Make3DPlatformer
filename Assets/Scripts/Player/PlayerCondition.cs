using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void TakePhysicalDamage(int damage);
}

public class PlayerCondition : MonoBehaviour, IDamageable
{
    public UICondition uiCondition;

    Condition health { get { return uiCondition.health; } }
    Condition stemina { get { return uiCondition.stamina; } }

    public float noHungerHealthDecay;

    public event Action onTakeDamage;

    // Update is called once per frame
    void Update()
    {
        health.Add(health.passiveValue * Time.deltaTime);

        stemina.Add(stemina.passiveValue * Time.deltaTime);

        if (health.curValue <= 0f)
        {
            Die();
        }
    }


    public void Heal(float amount)
    {
        health.Add(amount);
    }

    public void Die()
    {
        Debug.Log("»ç¸Á");
    }

    public void TakePhysicalDamage(int damage)
    {
        health.Subtract(damage);
        onTakeDamage?.Invoke();
    }

    public bool UseStamina(float amount)
    {
        if(stemina.curValue - amount < 0f)
        {
            return false;
        }

        stemina.Subtract(amount);
        return true;
    }
}
