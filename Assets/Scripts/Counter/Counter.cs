using UnityEngine;
using UnityEngine.Events;

public class Counter : MonoBehaviour
{
    protected int value;

    protected UnityEvent<int> onValueChanged = new UnityEvent<int>();

    public int Value
    {
        get => value;

        set {
            value = Mathf.Max(value, 0);

            if (this.value == value)
                return;

            this.value = value;

            onValueChanged.Invoke(this.value);
        }
    }

    public UnityEvent<int> OnValueChanged { get => onValueChanged; }
}
