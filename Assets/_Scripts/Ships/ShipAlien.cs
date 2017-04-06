using UnityEngine;
using System.Collections;

public class ShipAlien : ShipBase
{
    // Primary: Alternating lasers
    public Transform[] laserTransforms;
    private int currentLaser = 0;

    // Secondary: Burst of Missiles
    // Use same transforms as lasers because lazy
    private bool firingMissileBurst = false;
    private int currentMissileTransf = 0;
    private int currentMissile = 0;
    private int numMissiles = 5;
    private float midBurstDelay = 0.25f;
    private float nextMissileTime;

    new public void Start()
    {
        base.Start();

        primaryFireNext = 0f;
    }

    new public void FixedUpdate()
    {
        base.FixedUpdate();

        if(firingMissileBurst)
        {
            FireMissileBurst();
        }
    }

    public override void InitializeStats()
    {
        base.InitializeStats();

        currentLaser = 0;
        firingMissileBurst = false;
        currentMissileTransf = 0;
        currentMissile = 0;
        nextMissileTime = 0f;
    }

    public override void FirePrimary()
    {
        GameObject laserObj;
        LaserController laserScript;
        Vector3 rotation;
        Transform laserTransf = laserTransforms[currentLaser];

        if(Time.time < primaryFireNext) return;
        _audioController.Play(_audioController.primarySound);

        // Alternate between lasers
        currentLaser = (currentLaser + 1) % laserTransforms.Length;

        rotation = ScatterProjectile(laserTransf.rotation);

        // Create laser
        laserObj = (GameObject) GameObject.Instantiate(
            primaryObjReference,
            laserTransf.position,
            Quaternion.Euler(rotation));

        laserScript = laserObj.GetComponent<LaserController>();
        laserScript.Fire(enemyTag, primaryFireDamage);

        // Recoil
        _rigidbody.AddForce(-transform.forward * primaryFireRecoil, ForceMode.Impulse);

        primaryFireNext = Time.time + primaryFireDelay;
    }

    

    public override void FireSecondary()
    {
        if(Time.time < secondaryFireNext) return;
        // Next fire time calculated at end of burst
        secondaryFireNext = Mathf.Infinity;

        firingMissileBurst = true;
        currentMissile = 0;
        nextMissileTime = Time.time;

    }

    public void FireMissileBurst()
    {
        if(Time.time < nextMissileTime) return;
        nextMissileTime = Time.time + midBurstDelay;

        if(currentMissile >= numMissiles)
        {
            firingMissileBurst = false;
            secondaryFireNext = Time.time + secondaryFireDelay;
        }
        else
        {
            FireMissile();
            currentMissile++;
        }
    }

    public void FireMissile()
    {
        GameObject missileObj;
        MissileController missileScript;
        Transform missileTransf = laserTransforms[currentMissileTransf];

        _audioController.Play(_audioController.secondarySound);

        currentMissileTransf = (currentMissileTransf + 1) % laserTransforms.Length;

        missileObj = (GameObject) GameObject.Instantiate(
            secondaryObjectReference,
            missileTransf.position,
            missileTransf.rotation);

        missileScript = missileObj.GetComponent<MissileController>();
        missileScript.Fire(enemyTag, secondaryFireDamage);

        _rigidbody.AddForce(-transform.forward * secondaryFireRecoil, ForceMode.Impulse);
    }
}
