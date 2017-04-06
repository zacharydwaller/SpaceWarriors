using UnityEngine;
using System.Collections;

public class ShipHuman : ShipBase
{
    new public void Start()
    {
        base.Start();

        primaryFireNext = 0f;
    }

    // Spread shot
    public override void FirePrimary()
    {
        GameObject laserObj;
        LaserController laserScript;
        Vector3 rotation;

        if(Time.time < primaryFireNext) return;
        _audioController.Play(_audioController.primarySound);

        rotation = ScatterProjectile(transform.rotation);

        laserObj = (GameObject) GameObject.Instantiate(
            primaryObjReference,
            transform.position,
            Quaternion.Euler(rotation));

        laserScript = laserObj.GetComponent<LaserController>();
        laserScript.Fire(enemyTag, primaryFireDamage);

        _rigidbody.AddForce(-transform.forward * primaryFireRecoil, ForceMode.Impulse);

        primaryFireNext = Time.time + primaryFireDelay;
    }

    public override void FireSecondary()
    {
        if(Time.time < secondaryFireNext) return;

        GameObject missileObj;
        MissileController missileScript;

        _audioController.Play(_audioController.secondarySound);

        missileObj = (GameObject) GameObject.Instantiate(
            secondaryObjectReference,
            transform.position,
            transform.rotation);

        missileScript = missileObj.GetComponent<MissileController>();
        missileScript.Fire(enemyTag, secondaryFireDamage);

        _rigidbody.AddForce(-transform.forward * secondaryFireRecoil, ForceMode.Impulse);

        secondaryFireNext = Time.time + secondaryFireDelay;
    }
}
