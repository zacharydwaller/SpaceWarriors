using UnityEngine;
using System.Collections;

public class MenuCoolBackground : MonoBehaviour
{
    public float speed;

    public Material mat;

    public void Update()
    {
        mat.mainTextureOffset = new Vector2((mat.mainTextureOffset.x + (speed * Time.deltaTime)) % 1, mat.mainTextureOffset.y);
    }
}