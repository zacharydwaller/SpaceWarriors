using UnityEngine;
using System.Collections;

// Rotates an object in a sinusoidal fashion
public class MenuRotator : MonoBehaviour
{
    public float rotationSpeed; // degrees / sec (kind of)
    public Vector2 rotateBetween; // two y-values to rotate between
    
    public void Update()
    {
        float amplitude = Mathf.Abs(rotateBetween.x - rotateBetween.y) / 2f * Mathf.Deg2Rad;
        float period = rotationSpeed * Mathf.Deg2Rad;
        float shift = ((rotateBetween.x + rotateBetween.y) / 2f) * Mathf.Deg2Rad;
        
        // f(x) = a*sin(b*x-c)+d
        float newRotation = amplitude * Mathf.Sin((period / 2*Mathf.PI) * Time.time) + shift;

        transform.rotation = Quaternion.Euler(
            transform.rotation.eulerAngles.x,
            newRotation * Mathf.Rad2Deg,
            transform.rotation.eulerAngles.z);
    }
}
