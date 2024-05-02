using UnityEngine;

public class Gem : MonoBehaviour
{
    void OnMouseDown()
    {
        GetComponent<GemInfo>().Game.onGemClick(GetComponent<GemInfo>());
    }
}