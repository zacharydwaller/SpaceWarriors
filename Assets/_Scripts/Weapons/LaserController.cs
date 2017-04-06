using UnityEngine;
using System.Collections;

public class LaserController : MonoBehaviour
{
    public float damage;
    public float speed = 30f;
    public float impactForce = 2.5f;

    public AudioClip hitSound;

    private string targetTag;
    private float lifetime = 2f;

    private AudioSource _audioSource;

    public void Update()
    {
        // Destroy after a certain period of time
        lifetime -= Time.deltaTime;

        if(lifetime <= 0f)
        {
            GameObject.Destroy(gameObject);
        }

        // Lock Y position to 0
        transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
    }

    // To be called by the firing ship
    public void Fire(string newTargetTag, float newDamage)
    {
        _audioSource = GetComponentInChildren<AudioSource>();

        targetTag = newTargetTag;
        damage = newDamage;

        GetComponent<Rigidbody>().velocity = transform.forward * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag.Equals(targetTag))
        {
            GameObject particleObj = transform.GetChild(0).gameObject;
            GameObject audioObj = transform.GetChild(1).gameObject;
            ParticleSystem particleSystem = particleObj.GetComponent<ParticleSystem>();

            _audioSource.PlayOneShot(hitSound);

            other.SendMessage("TakeDamage", damage);
            other.GetComponent<Rigidbody>().AddForce(transform.forward * impactForce, ForceMode.Impulse);

            transform.DetachChildren();
            particleSystem.Play();

            GameObject.Destroy(gameObject);
            GameObject.Destroy(particleObj, particleSystem.main.duration);
            GameObject.Destroy(audioObj, hitSound.length);
        }
        if(other.tag.Equals("BlackHole"))
        {
            GameObject.Destroy(gameObject);
        }
    }
}
