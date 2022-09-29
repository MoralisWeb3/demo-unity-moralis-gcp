using UnityEngine;
using System;

[RequireComponent(typeof(TextMesh))]
public class PlayerWalletAddress : MonoBehaviour
{
    private TextMesh textMesh;

    private void Awake()
    {
        textMesh = GetComponent<TextMesh>();
        Hide();
    }

    private void LateUpdate()
    {
        if (Camera.main == null) return;
        if (textMesh.text == string.Empty) return;

        textMesh.transform.rotation =
            Quaternion.LookRotation(textMesh.transform.position - Camera.main.transform.position);
    }

    public void Show(string address)
    {
        //TODO formatting
        address = address.Remove(6,30).Insert(6, "...");
        textMesh.text = address;
    }

    public void Hide()
    {
        textMesh.text = string.Empty;
    }
}
