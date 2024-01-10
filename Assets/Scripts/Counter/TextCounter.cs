using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TextCounter : Counter
{
    TMP_Text tmpText;

    float valueDisplayed;
    [SerializeField] float speed = 8;

    [SerializeField] float textScaleCoef = 0.2f;


    float ValueDisplayed
    {
        set {
            valueDisplayed = value;
            tmpText.text = Mathf.RoundToInt(valueDisplayed).ToString();
            float scale = 1 + Mathf.Abs(this.value - valueDisplayed) * textScaleCoef;
            tmpText.transform.localScale = scale * Vector3.one;
        }
    }

    void Awake()
    {
        tmpText = GetComponent<TMP_Text>();

        valueDisplayed = value;
        tmpText.text = value.ToString();
        tmpText.transform.localScale = Vector3.one;
    }

    void OnEnable()
    {
        ValueDisplayed = value;
    }

    void Update()
    {
        ValueDisplayed = Time.timeScale == 0 ? value : Mathf.Lerp(valueDisplayed, value, speed * Time.deltaTime); // TODO
    }
}
