using TMPro;
using UnityEngine;



public class TokensContainerText : MonoBehaviour
{
    [SerializeField] GameObject activeOnOver;
    [SerializeField] TMP_Text tmpText;
    [SerializeField] LayerMask layer;

    TokensContainer over = null;

    void SetOver(TokensContainer over)
    {
        if (this.over == over)
            return;

        if (this.over)
        {
            this.over.OnValueChanged.RemoveListener(OnValueChanged);
        }

        this.over = over;

        activeOnOver.SetActive(false); // reset anim
        activeOnOver.SetActive(this.over && this.over.Value > 0);

        if (over)
        {
            transform.position = Camera.main.WorldToScreenPoint(over.transform.position);

            tmpText.text = over.Value.ToString();

            over.OnValueChanged.AddListener(OnValueChanged);
        }
    }

    void Awake()
    {
        activeOnOver.SetActive(false);
    }

    void Update()
    {
        TokensContainer over = null;

#if UNITY_EDITOR

        if (Physics.Raycast(
            Camera.main.ScreenPointToRay(Input.mousePosition),
            out RaycastHit hit,
            100,
            layer))
            over = hit.transform.GetComponentInParent<TokensContainer>();

#elif UNITY_ANDROID

        if (Input.touchCount > 0 && 
            Physics.Raycast(
            Camera.main.ScreenPointToRay(Input.GetTouch(0).position),
            out RaycastHit hit,
            100,
            layer))
            over = hit.transform.GetComponentInParent<TokensContainer>();

#endif


        SetOver(over);
    }

    void OnValueChanged(int value)
    {
        tmpText.text = over.Value.ToString();
        activeOnOver.SetActive(value > 0);
    }
}
