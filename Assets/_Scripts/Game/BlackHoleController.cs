using UnityEngine;
using System.Collections;

public class BlackHoleController : MonoBehaviour
{
    public GameObject[] affectedObjects; // Player ships
    public float pullStrength;

    public float rotationSpeed;

    // Pull in affected game objects and all projectiles
    public void FixedUpdate()
    {
        Rotate();
        PullGravity();
    }

    public void Rotate()
    {
        transform.Rotate(new Vector3(0f, rotationSpeed * Time.deltaTime, 0f));
    }

    public void PullGravity()
    {
        Vector3 difference;
        float magnitudeSqr;
        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");

        foreach(GameObject projectile in projectiles)
        {
            difference = projectile.transform.position - transform.position;
            magnitudeSqr = difference.sqrMagnitude;
            difference.Normalize();

            // Gravitational force F=G*m1*m2/r^2
            // Pull Strength simulates G*m1
            // m2 is taken into consideration with Unity's ForceMode.Force
            projectile.GetComponent<Rigidbody>().AddForce(-difference * (pullStrength / magnitudeSqr), ForceMode.Force);
        }

        foreach(GameObject obj in affectedObjects)
        {
            difference = obj.transform.position - transform.position;
            difference.Normalize();

            // Ignore radius^2 with player ships because it's not fun
            obj.GetComponent<Rigidbody>().AddForce(-difference * pullStrength, ForceMode.Force);
        }
    }
}
