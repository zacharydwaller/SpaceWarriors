using UnityEngine;
using System.Collections;

public class ShipBase : MonoBehaviour
{
    [Header("Input")]
    public string verticalAxis;
    public string horizontalAxis;
    public string verticalAimAxis;
    public string horizontalAimAxis;
    public string firePrimaryAxis;
    public string fireSecondaryAxis;
    public string teleportAxis;

    public bool useMouse;

    [Header("Stats")]
    public float maxHP;
    public float currentHP;

    public float maxVelocity;
    public float acceleration;
    public float turnSpeed;

    [Header("Teleport")]
    public int teleportsRemaining;
    public float teleportDelay;
    [HideInInspector]
    private float teleportNext;

    [Header("Weapons")]
    // Primary: Lasers
    public GameObject primaryObjReference;
    public float primaryFireDamage;
    public float primaryFireDelay;
    public float primaryFireRecoil;

    private float primaryFireStartTime;
    public float primaryFireScatterIncrease;
    public float primaryFireScatterMax;

    [HideInInspector]
    public float primaryFireNext;

    // Secondary: Tracking Missiles
    public GameObject secondaryObjectReference;
    public float secondaryFireDamage;
    public float secondaryFireDelay;
    public float secondaryFireRecoil;
    [HideInInspector]
    public float secondaryFireNext;

    // Screen Wraparound variables
    [HideInInspector]
    public Vector3 _bottomLeftWorldPoint;
    [HideInInspector]
    public Vector3 _topRightWorldPoint;
    [HideInInspector]
    public Vector2 _screenDimensions;

    // Black Hole varibles
    // Black Hole does (maxHP * blackHoleDamageMultiplier) damage
    [HideInInspector]
    public float blackHoleDamageDelay = 1f;
    [HideInInspector]
    public float blackHoleNextDamage;
    [HideInInspector]
    public float blackHoleDamageMultiplier = 1.0f;

    // Collision Variables
    // Collision damage is (maxHP * collisionDamageMultiplier) * (other.velocity * other.maxVelocity)
    // Whoever is going faster will do more damage to the other person but both will take damage
    [HideInInspector]
    public float collisionDamageDelay = 0.5f;
    [HideInInspector]
    public float collisionNextDamage;
    [HideInInspector]
    public float collisionDamageMultiplier = 0.25f;

    // Game Variables
    [HideInInspector]
    public bool canBeControlled = true; // false when round countdown is happening

    // Components
    [HideInInspector]
    public string enemyTag;
    [HideInInspector]
    public Rigidbody _rigidbody;
    public ShipAudioController _audioController;
    public GameObject _explosionRef;

    public void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();

        FindCorners();

        InitializeStats();

        if(tag == "Human") enemyTag = "Alien";
        else enemyTag = "Human";
    }

    public void FixedUpdate()
    {
        // Screen wraparound works even if player resizes window
        if(HasScreenResized())
        {
            FindCorners();
        }

        PerformInput();
        CheckWraparound();
    }

    public virtual void InitializeStats()
    {
        currentHP = maxHP;

        blackHoleNextDamage = 0f;
        primaryFireNext = 0f;
        secondaryFireNext = 0f;

        teleportsRemaining = 5;

        // Have to zero out velocity or else hilarious things happen
        _rigidbody.velocity = Vector3.zero;
    }

    /* Input */

    public virtual void PerformInput()
    {
        float horz, vert;

        if(canBeControlled == false) return;

        // Use GetAxisRaw because Unity filters inputs in a very strange way
        horz = Input.GetAxisRaw(horizontalAxis);
        vert = Input.GetAxisRaw(verticalAxis);

        Move(horz, vert);

        if(useMouse)
        {
            LookAtMouse();
        }
        else
        {
            float aimHorz, aimVert;

            aimHorz = Input.GetAxisRaw(horizontalAimAxis);
            aimVert = Input.GetAxisRaw(verticalAimAxis);

            Aim(aimHorz, aimVert);
        }
        
        FixRotation(); // used to lock X and Z rotation but allow Y rotation
        
        if(Input.GetButtonDown(firePrimaryAxis))
        {
            StartFirePrimary();
        }
        if(Input.GetAxisRaw(firePrimaryAxis) > 0f)
        {
            FirePrimary();
        }

        if(Input.GetAxisRaw(fireSecondaryAxis) > 0f)
        {
            FireSecondary();
        }

        if(Input.GetAxisRaw(teleportAxis) > 0f)
        {
            DoTeleport();
        }
    }

    public virtual void Move(float horz, float vert)
    {
        _rigidbody.AddForce(new Vector3(horz, 0f, vert) * acceleration, ForceMode.Acceleration);
        _rigidbody.velocity = Vector3.ClampMagnitude(_rigidbody.velocity, maxVelocity);
    }

    public virtual void LookAtMouse()
    {
        Ray mouseRay;
        float rayLength;
        int floorMask;
        RaycastHit floorHit;

        rayLength = 100f;
        floorMask = LayerMask.GetMask("Floor");

        mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(mouseRay, out floorHit, rayLength, floorMask);
        transform.LookAt(floorHit.point);
    }

    public virtual void Aim(float horz, float vert)
    {
        transform.LookAt(transform.position + new Vector3(horz, 0f, vert));
    }

    public virtual void FixRotation()
    {
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
    }

    /* Weapons */

    public void StartFirePrimary()
    {
        primaryFireStartTime = Time.time;
    }

    // ShipHuman and ShipAlien will extend these
    public virtual void FirePrimary()
    {
    }

    public virtual void FireSecondary()
    {
    }

    public virtual void DoTeleport()
    {
        GameObject blackHole, enemyShip;
        Vector3 newWorldPoint;
        float blackHoleSafeRange, enemySafeRange;
        float minTeleportDistance;
        float distToBlackHole, distToEnemyShip, distToTeleportTarget;

        if(Time.time < teleportNext) return;

        // Destroy ship if no more hyperdrives remaining
        if(teleportsRemaining <= 0)
        {
            TakeDamage(maxHP);
            return;
        }

        // Find the black hole and enemy ship
        blackHole = GameObject.FindGameObjectWithTag("BlackHole");
        if(blackHole)
        {
            blackHoleSafeRange = blackHole.transform.localScale.x + 1f;
        }
        else
        {
            blackHoleSafeRange = 0f;
        }

        enemyShip = GameObject.FindGameObjectWithTag(enemyTag);
        enemySafeRange = 4f;

        minTeleportDistance = 15f;

        // Make sure teleport destination is good
        // Good teleport destinations:
        //      NOT outside of the screen
        //      NOT near or inside black hole
        //      NOT near or inside enemy ship
        //      Far enough away from original location for teleport to be useful
        do
        {
            // Not outside of screen
            newWorldPoint = new Vector3(
                Random.Range(_bottomLeftWorldPoint.x, _topRightWorldPoint.x),
                0f,
                Random.Range(_topRightWorldPoint.z, _bottomLeftWorldPoint.z));

            // Not near black hole
            if(blackHole)
                distToBlackHole = Vector3.Distance(newWorldPoint, blackHole.transform.position);
            else
                distToBlackHole = Mathf.Infinity;

            // Not near enemy ship
            distToEnemyShip = Vector3.Distance(newWorldPoint, enemyShip.transform.position);

            // Far enough away from original location
            distToTeleportTarget = Vector3.Distance(newWorldPoint, transform.position);
        } while(
            distToBlackHole < blackHoleSafeRange ||
            distToEnemyShip < enemySafeRange ||
            distToTeleportTarget < minTeleportDistance);

        _audioController.Play(_audioController.teleportSound);
        teleportsRemaining--;
        teleportNext = Time.time + teleportDelay;
        transform.position = newWorldPoint;

        // Zero out velocity
        _rigidbody.velocity = Vector3.zero;
    }

    public Vector3 ScatterProjectile(Quaternion startRotation)
    {
        Vector3 rotation;
        float currentScatter = Mathf.Min(
            (Time.time - primaryFireStartTime) * primaryFireScatterIncrease,
            primaryFireScatterMax);

        rotation = new Vector3(
            startRotation.eulerAngles.x,
            startRotation.eulerAngles.y,
            startRotation.eulerAngles.z);

        rotation.y += Random.Range(-currentScatter / 2, currentScatter / 2);

        return rotation;
    }

    /* Health */
    public void TakeDamage(float amount)
    {
        currentHP -= amount;

        if(currentHP <= 0f)
        {
            GameObject explosionObject = (GameObject) Instantiate(_explosionRef, transform.position, Quaternion.identity);
            GameObject.Destroy(explosionObject, explosionObject.GetComponent<ParticleSystem>().main.duration);

            _audioController.Play(_audioController.deathSound);

            currentHP = 0;

            // Tell game manager ship was destroyed
            GameObject.FindGameObjectWithTag("GameController").SendMessage("PlayerDefeated", gameObject.tag);
        }
    }

    /* Other */

    public void Freeze()
    {
        canBeControlled = false;

        // Sometimes GameManager calls Freeze() before this object's Start() causing _rigidbody to be null
        if(!_rigidbody)
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        // Zero out all velocity or else hilarity will ensue
        _rigidbody.isKinematic = true;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }

    public void UnFreeze()
    {
        canBeControlled = true;

        _rigidbody.isKinematic = false;
    }

    // Check if player is outside screen and do the Asteroids thing
    public void CheckWraparound()
    {
        Vector3 oldPos;

        oldPos = transform.position;
        if(oldPos.x < _bottomLeftWorldPoint.x)
        {
            transform.position = new Vector3(_topRightWorldPoint.x, oldPos.y, oldPos.z);
        }
        else if(oldPos.x > _topRightWorldPoint.x)
        {
            transform.position = new Vector3(_bottomLeftWorldPoint.x, oldPos.y, oldPos.z);
        }

        if(oldPos.z < _bottomLeftWorldPoint.z)
        {
            transform.position = new Vector3(oldPos.x, oldPos.y, _topRightWorldPoint.z);
        }
        else if(oldPos.z > _topRightWorldPoint.z)
        {
            transform.position = new Vector3(oldPos.x, oldPos.y, _bottomLeftWorldPoint.z);
        }

    }

    public bool HasScreenResized()
    {
        if( _screenDimensions.x != Camera.main.pixelWidth ||
            _screenDimensions.y != Camera.main.pixelHeight)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Find the in-game locations where the screen corners are
    public void FindCorners()
    {
        Ray camRay;
        float rayLength;
        int floorMask;
        RaycastHit floorHit;

        rayLength = 100f;
        floorMask = LayerMask.GetMask("Floor");

        // Find bottom left corner
        camRay = Camera.main.ScreenPointToRay(new Vector3(0f, 0f));
        Physics.Raycast(camRay, out floorHit, rayLength, floorMask);
        _bottomLeftWorldPoint = floorHit.point;

        // Find bottom right corner
        camRay = Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight));
        Physics.Raycast(camRay, out floorHit, rayLength, floorMask);
        _topRightWorldPoint = floorHit.point;
    }

    public void OnCollisionEnter(Collision other)
    {
        // Collide with enemy ship
        if(other.transform.tag.Equals(enemyTag))
        {
            if(Time.time < collisionNextDamage) return;
            collisionNextDamage = Time.time + collisionDamageDelay;

            // Collision sound will play from whichever ship is currently going faster
            if(_rigidbody.velocity.sqrMagnitude > other.rigidbody.velocity.sqrMagnitude)
            {
                _audioController.Play(_audioController.collisionSound, _rigidbody.velocity.magnitude / maxVelocity);
            }

            // Whoever is going slower in the collision will recieve more damage
            // (maxHP * collisionMultiplier) * (other.velocity / other.maxVelocity)
            TakeDamage(
                (maxHP * collisionDamageMultiplier) *
                (other.rigidbody.velocity.magnitude / other.gameObject.GetComponent<ShipBase>().maxVelocity));
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if(other.transform.tag.Equals("BlackHole"))
        {
            if(Time.time < blackHoleNextDamage) return;
            blackHoleNextDamage = Time.time + blackHoleDamageDelay;

            TakeDamage(maxHP * blackHoleDamageMultiplier);
        }
    }
}
