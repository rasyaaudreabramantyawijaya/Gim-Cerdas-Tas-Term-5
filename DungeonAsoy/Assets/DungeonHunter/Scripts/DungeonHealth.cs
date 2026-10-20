using System;
using UnityEngine;

public sealed class DungeonHealth : MonoBehaviour
{
    [SerializeField] private float maximum = 100f;
    [SerializeField] private float current = 100f;
    public float Maximum => maximum;
    public float Current => current;
    public bool IsDead => current <= 0f;
    public event Action<float, float> Changed;
    public event Action Died;
    private bool deathSent;

    public void Configure(float value)
    {
        maximum = Mathf.Max(1f, value);
        current = maximum;
        deathSent = false;
        Changed?.Invoke(current, maximum);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f) return;
        current = Mathf.Max(0f, current - amount);
        Changed?.Invoke(current, maximum);
        if (current <= 0f && !deathSent)
        {
            deathSent = true;
            Died?.Invoke();
        }
    }
}
