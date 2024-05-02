using UnityEngine;

public class Gem : MonoBehaviour
{
    void OnMouseDown()
    {
        GetComponent<GemInfo>().Game.onGemMouseDown(GetComponent<GemInfo>());
    }

    void OnMouseUp()
    {
        GetComponent<GemInfo>().Game.onGemMouseUp(GetComponent<GemInfo>());
    }
}