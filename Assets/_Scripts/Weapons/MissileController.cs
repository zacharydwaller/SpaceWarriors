using UnityEngine;
using System.Collections;

public class MissileController : MonoBehaviour
{
    public float damage;
    public float speed = 15f;
    public float turnRate = 5f;
    public float impactForce = 50f;

    public AudioClip explosionSound;

    private string targetTag;

    private float lifetime = 5f;

    private Transform targetTransf;

    private Rigidbody _rigidbody;
    private AudioSource _audioSource;

    public void Update()
    {
        lifetime -= Time.deltaTime;

        if(lifetime <= 0f)
        {
            GameObject.Destroy(gameObject);
        }

        TrackTarget();
        transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
    }

    public void Fire(string newTargetTag, float newDamage)
    {
        _audioSource = GetComponentInChildren<AudioSource>();
        _rigidbody = GetComponent<Rigidbody>();

        targetTag = newTargetTag;
        damage = newDamage;
        _rigidbody.velocity = transform.forward * speed;

        targetTransf = GameObject.FindGameObjectWithTag(targetTag).transform;
    }

    public void TrackTarget()
    {
        Quaternion oldRotation;
        Quaternion targetRotation;

        oldRotation = transform.rotation;
        transform.LookAt(targetTransf);
        targetRotation = transform.rotation;
        transform.rotation = oldRotation;

        _rigidbody.rotation = Quaternion.RotateTowards(oldRotation, targetRotation, turnRate);
        _rigidbody.velocity = transform.forward * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag.Equals(targetTag))
        {
            GameObject sparksObj = transform.GetChild(0).gameObject;
            GameObject smokeObj = transform.GetChild(1).gameObject;
            GameObject audioObj = transform.GetChild(2).gameObject;
            ParticleSystem sparksParticles = sparksObj.GetComponent<ParticleSystem>();
            ParticleSystem smokeParticles = smokeObj.GetComponent<ParticleSystem>();

            _audioSource.PlayOneShot(explosionSound);

            other.SendMessage("TakeDamage", damage);
            other.GetComponent<Rigidbody>().AddForce(transform.forward * impactForce, ForceMode.Impulse);

            transform.DetachChildren();
            sparksParticles.Play();
            smokeParticles.Stop();

            GameObject.Destroy(gameObject);
            GameObject.Destroy(sparksObj, sparksParticles.main.duration);
            GameObject.Destroy(smokeObj, smokeParticles.main.duration);
            GameObject.Destroy(audioObj, explosionSound.length);
        }
        if(other.tag.Equals("BlackHole"))
        {
            GameObject.Destroy(gameObject);
        }
    }
}
