using UnityEngine;
using System.Collections;

public class MenuBobber : MonoBehaviour
{
    public float bobSpeed;
    public Vector2 bobBetween; // Two y-positions to bob between

    // Bobs the object up and down in a sinusoidal fashion
    public void Update()
    {
        float amplitude = Mathf.Abs(bobBetween.x - bobBetween.y) / 2f;
        float period = bobSpeed * Mathf.Deg2Rad;
        float shift = (bobBetween.x + bobBetween.y) / 2f;

        // f(x) = a*sin(b*x-c)+d
        float newPosition = amplitude * Mathf.Sin((period / 2 * Mathf.PI) * Time.time) + shift;

        transform.position = new Vector3(
            transform.position.x,
            newPosition,
            transform.position.z);
    }
}
