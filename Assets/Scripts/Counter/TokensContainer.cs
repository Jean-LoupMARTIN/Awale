using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TokensContainer : Counter
{
    [SerializeField] GameObject tokenPrefab;
    [SerializeField] Transform tokenSpawn;
    [SerializeField] float tokenSpawnRadius = 0.02f;
    [SerializeField] float dtSpawnToken = 0.2f;
    Queue<GameObject> tokens = new Queue<GameObject>();

    void OnDrawGizmosSelected()
    {
        DrawTokenRadius();
    }

    void DrawTokenRadius()
    {
        if (!tokenSpawn)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(tokenSpawn.position, tokenSpawnRadius);
    }

    void Awake()
    {
        TokensMatchValue();
        OnValueChanged.AddListener((value) => { TokensMatchValue(); });
    }


    void TokensMatchValue()
    {
        if (tokens.Count == value)
        {
            if (addTokensCoroutine != null)
            {
                StopCoroutine(addTokensCoroutine);
                addTokensCoroutine = null;
            }
        }

        else if (tokens.Count > value)
        {
            if (addTokensCoroutine != null)
            {
                StopCoroutine(addTokensCoroutine);
                addTokensCoroutine = null;
            }

            while (tokens.Count > value)
                Destroy(tokens.Dequeue());
        }

        else if (tokens.Count < value)
            if (addTokensCoroutine == null)
                addTokensCoroutine = StartCoroutine(AddTokens());

}

    Coroutine addTokensCoroutine = null;

    IEnumerator AddTokens()
    {
        while (tokens.Count < value)
        {
            GameObject tokenGO = Instantiate(
                tokenPrefab,
                tokenSpawn.position + RandomExtension.RandomPointInSphere(tokenSpawnRadius),
                Quaternion.identity, //RandomExtension.RandomQuaternion(),
                transform);

            tokens.Enqueue(tokenGO);

            yield return new WaitForSeconds(dtSpawnToken);
        }

        addTokensCoroutine = null;
    }
}
