using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class Ball_Physics : MonoBehaviour
{
    private Camera mainCamera;
    Rigidbody2D rb;
    [HideInInspector] public SpriteRenderer spr;
    [Header("Raycasting")]
    public LayerMask collidables;
    public ContactFilter2D contactFilter = new ContactFilter2D();
    //public float raycastOffset;
    public bool usingPhysics;
    public bool wasInWater = false;
    [Range(0f,1f)]public float rayLength;

    [Range(0f, 1f)] public float circleRad;
    public LayerMask Hazards;
    [Header("Unity Col Adjustments")]
    CircleCollider2D col;
    public bool onGround = false;
    public bool nearingGround = false, nearingCeiling = false;
    [Header("DuckTape")]
    public float maxVelocity;
    Vector2 prevPos;
    private float minimumExtent;
    private float partialExtent;
    public float skinWidth = 0.1f; //probably doesn't need to be changed 
    [Header("Input system")]
    public InputActionReference mousePos;
    public InputActionReference touchAction, mouseClick;
    [Header("Drag and click mechanics")]
    public Vector2 startPos;
    public Vector2 dir;
    [Header("Forces")]
    public float swingforce;
    Vector3 brakeVel;
    public float brakeVelMod = 1;
    [Header("Effects")]
    [HideInInspector] public LineRenderer dirLn;
    [HideInInspector] public ParticleSystem dustPart;
    [HideInInspector] ParticleSystem deathPart;
    bool slow = false;
    [Header("GO's")]
    public GameObject radiusPref;
    PulsatingCircle activeCircle;
    public PulsatingCircle[] activeCirlcesInScene;
    [Header("PAUSING / LEVEL FINISH STUFF")]
    public bool canMove = false;
    Transform holeTrans;
    Vector2 storedRB;
    [Header("cols")]
    public bool dead = false;

    private void Awake()
    {
        canMove = false;
        activeCirlcesInScene = new PulsatingCircle[2];
        activeCircle = Instantiate(radiusPref, startPos, Quaternion.identity).GetComponent<PulsatingCircle>();
        activeCircle.gameObject.SetActive(false);
        activeCirlcesInScene[0] = activeCircle;
        activeCircle = Instantiate(radiusPref, startPos, Quaternion.identity).GetComponent<PulsatingCircle>();
        activeCircle.gameObject.SetActive(false);
        activeCirlcesInScene[1] = activeCircle;
        activeCircle = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        dirLn = transform.GetChild(0).GetComponent<LineRenderer>();
        dustPart = transform.GetChild(1).GetComponent<ParticleSystem>();
        spr = transform.GetChild(3).GetComponent<SpriteRenderer>();
        deathPart = transform.GetChild(4).GetComponent<ParticleSystem>();
        usingPhysics = true;
        mainCamera = Camera.main;
        Application.targetFrameRate = 60;
        rb = GetComponent<Rigidbody2D>();
        prevPos = rb.position;
        col = GetComponent<CircleCollider2D>();
        Physics2D.IgnoreLayerCollision(6,7);
        Physics2D.IgnoreLayerCollision(10, 7);

        minimumExtent = Mathf.Min(col.bounds.extents.x, col.bounds.extents.y);
        partialExtent = minimumExtent * (1.0f - skinWidth);
        dirLn.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        HazardCols();
        if (canMove)
        {
            GetMousePos();
            SquishAndStretch();
        }
        else if (holeTrans)
        {
            TriggerHoleEnter(holeTrans);
            slow = false;
        }
        else
        {
            BackUpInput();
        }


        if (slow && Time.timeScale != 0.3f && !GameManager.ins.paused) { Time.timeScale = .3f; }
        if (!slow && Time.timeScale != 1 && !GameManager.ins.paused)
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, 1, Time.deltaTime * 5);
        }
        Time.fixedDeltaTime = 0.02F * Time.timeScale;
    }

    private void FixedUpdate()
    {
        Vector2 movementThisStep = (Vector2)transform.position - prevPos;

        var speed = Vector3.Magnitude(rb.velocity);
        var v = rb.velocity;
        if (speed > maxVelocity)
        {
            float brakeSpeed = speed - maxVelocity;
            Vector3 normalizedVel = v.normalized;
            brakeVel = normalizedVel * brakeSpeed * brakeVelMod;
            rb.AddForce(-brakeVel);
        }

        if (speed > 22)
        {
            rb.velocity = rb.velocity.normalized * 22;
        }
        /*
                rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, .1f, Time.deltaTime * .01f);

                if (rb.velocity.magnitude < 0.1f)
                {
                    rb.angularVelocity = 0;
                }*/


        if (usingPhysics && canMove)
        {
            CollisionScanForward();
        }


        float movementSqrMag = movementThisStep.sqrMagnitude;
        var temp = Physics2D.Raycast(prevPos, movementThisStep, movementSqrMag * 2, collidables);
        if (temp)
        {

            rb.velocity = Vector2.Reflect(rb.velocity, temp.normal);
            rb.position = temp.point - (movementThisStep / movementSqrMag) * partialExtent;

            Debug.Log("Fix position");
        }


        prevPos = rb.position;
        
    }

    private void LateUpdate()
    {

    }
    #region bounce collision
    void CollisionScanForward() // can sometimes go through walls (if moving pretty fast), look into it
    {
        var v = rb.velocity;
        var direction2 = Quaternion.Euler(0, 0, 45) * rb.velocity;
        var direction1 = Quaternion.Euler(0, 0, -45) * rb.velocity;
        var prevVel = new Vector2(rb.velocity.x, 0).normalized; 
        Debug.DrawLine(transform.position, (Vector2)transform.position + rb.velocity.normalized * rayLength, UnityEngine.Color.red);
        Debug.DrawLine(transform.position, transform.position + direction1.normalized * rayLength, UnityEngine.Color.green);
        Debug.DrawLine(transform.position, transform.position + direction2.normalized * rayLength, UnityEngine.Color.blue);

        // ground/water detection
        if (Physics2D.Raycast(transform.position, -transform.up, 0.6f, collidables) && Mathf.Abs(rb.velocity.y) < 1f) { onGround = true; }
        else if (!GameManager.ins.dynWater.playerInWater) { onGround = false; rb.gravityScale = Mathf.Lerp(rb.gravityScale, 3, Time.deltaTime); }
        else { rb.gravityScale = 0; rb.AddForce(Vector2.up/25, ForceMode2D.Impulse);  if (!wasInWater) wasInWater = true; }

        if (wasInWater && !GameManager.ins.dynWater.playerInWater)
        {
            brakeVelMod = 2f;
            wasInWater = false;
        }/*else if (GameManager.ins.dynWater.playerInWater && rb.velocity.y < 0)
        {
            brakeVelMod = 1.5f;
        }else if (GameManager.ins.dynWater.playerInWater)
        {
            brakeVelMod = 1.25f;
        }*/

        if (brakeVelMod != 1 && !GameManager.ins.dynWater.playerInWater)
        {
            brakeVelMod = Mathf.Lerp(brakeVelMod, 1, 5 * Time.deltaTime);
        }else if (GameManager.ins.dynWater.playerInWater && rb.velocity.y >= 0)
        {
            brakeVelMod = Mathf.Lerp(brakeVelMod, 3f, 5 * Time.deltaTime);
        }else if (GameManager.ins.dynWater.playerInWater && rb.velocity.y < 0)
        {
            brakeVelMod = Mathf.Lerp(brakeVelMod, 6, 5 * Time.deltaTime);
        }

        //nearing ground for squish
        nearingGround = Physics2D.Raycast(transform.position, -transform.up, 1, collidables);
        nearingCeiling = Physics2D.Raycast(transform.position, transform.up, 1, collidables);





        // if (results[0] != null && rb.gravityScale != 0)
        if (Mathf.Abs(rb.velocity.y) >= 3)
        {
            //for the love of god, fix this shit later

            var hitCol = Physics2D.Raycast(transform.position, v, rayLength, collidables);
            var hitCol1 = Physics2D.Raycast(transform.position, direction1, rayLength/1.1f, collidables);
            var hitCol2 = Physics2D.Raycast(transform.position, direction2, rayLength / 1.1f, collidables);
            // add 2 more raycasts with an offset to account for any deviations

            //Time.timeScale = 0;
            //Debug.DrawLine(transform.position, rb.velocity * 10000, UnityEngine.Color.black);
            if (hitCol)
            {
                //print(hitCol.normal);
                rb.velocity = Vector2.Reflect(v, hitCol.normal);
                if (rb.velocity.y < 0)
                    rb.gravityScale = 1;
                //Debug.Log("hit 0 ran");
            }
            else if (hitCol1)
            {
                rb.velocity = Vector2.Reflect(v, hitCol1.normal);
                if (rb.velocity.y < 0)
                    rb.gravityScale = 1;
                //Debug.Log("hit 1 ran");
            }
            else if (hitCol2)
            {
                rb.velocity = Vector2.Reflect(v, hitCol2.normal);
                if (rb.velocity.y < 0)
                    rb.gravityScale = 1;
                //Debug.Log("hit 2 ran");
            }
        }else if (Mathf.Abs(rb.velocity.y) < 3 && nearingGround)   //else if (results[0] != null && rb.gravityScale == 0)
        {
            //for the love of god, fix this shit later
            if (!GameManager.ins.dynWater.playerInWater)
                rb.gravityScale = 1;
            var hitCol = Physics2D.Raycast(transform.position, v, rayLength, collidables);
            var hitCol1 = Physics2D.Raycast(transform.position, direction1 / 1.1f, rayLength, collidables);
            var hitCol2 = Physics2D.Raycast(transform.position, direction2 / 1.1f, rayLength, collidables);
            // add 2 more raycasts with an offset to account for any deviations
            //Time.timeScale = 0;
            //Debug.DrawLine(transform.position, rb.velocity * 10000, UnityEngine.Color.black);
            if (hitCol)
            {
                //print(hitCol.normal);
                rb.velocity = Vector2.Reflect(new Vector2(rb.velocity.x, 3), hitCol.normal);
                if (rb.velocity.y < 0)
                    rb.gravityScale = 1;
                //Debug.Log("hit 0 ran");
            }
            else if (hitCol1)
            {
                rb.velocity = Vector2.Reflect(v, hitCol1.normal);
                if (rb.velocity.y < 0)
                    rb.gravityScale = 1;
                //Debug.Log("hit 1 ran");
            }
            else if (hitCol2)
            {
                rb.velocity = Vector2.Reflect(v, hitCol2.normal);
                if (rb.velocity.y < 0)
                    rb.gravityScale = 1;
                //Debug.Log("hit 2 ran");
            }
        } 

    }

    #endregion

    #region input 

    void BackUpInput()
    {

        Vector2 mousePosition = mousePos.action.ReadValue<Vector2>();
        mousePosition = mainCamera.ScreenToWorldPoint(mousePosition);
        if (mouseClick.action.WasPressedThisFrame())
        {
            startPos = mousePosition;
            Debug.Log("BACKUP");
            if (activeCirlcesInScene[0] == null)
                activeCircle = Instantiate(radiusPref, startPos, Quaternion.identity).GetComponent<PulsatingCircle>();
            else
            {
                activeCircle = activeCirlcesInScene[0];
            }
            activeCircle.target = startPos;
            activeCircle.gameObject.transform.position = startPos;
            activeCircle = null;
        }
    }


    void GetMousePos()
    {
        Vector2 mousePosition = mousePos.action.ReadValue<Vector2>();
        mousePosition = mainCamera.ScreenToWorldPoint(mousePosition);
        if (mouseClick.action.WasPressedThisFrame())
        {
            slow = true;
            Time.timeScale = .3f;
            startPos = mousePosition;
            if (activeCirlcesInScene[0] == null)
                activeCircle = Instantiate(radiusPref, startPos, Quaternion.identity).GetComponent<PulsatingCircle>();
            else
            {
                activeCircle = activeCirlcesInScene[0];
            }
            activeCircle.target = startPos;
            activeCircle.gameObject.transform.position = startPos;
            activeCircle.gameObject.SetActive(true);
            activeCirlcesInScene[0] = activeCircle;
            activeCircle = null;
        }

        if (mouseClick.action.IsPressed())
        {
            Vector2 t_dir = mousePosition - startPos;
            if (t_dir.magnitude != 0)
            {
                if (activeCircle != null)
                    activeCircle.target = mousePosition;

                float mag = t_dir.magnitude;
                t_dir = new Vector2(t_dir.x / mag, t_dir.y / mag);
                mag = Mathf.Clamp(mag, 0, 2);
                //Vector2 force = -dir * mag * swingforce;

                Vector3[] trajectory = new Vector3[2];
                trajectory[0] = transform.position;
                trajectory[1] = transform.position + (Vector3)(-t_dir * ((mag*3) / 2.5f));
                dirLn.SetPositions(trajectory);
                if (!dirLn.enabled)
                    dirLn.enabled = true;
                Vector2 undir = mousePosition - startPos;

                if (activeCircle == null)
                {
                    if (activeCirlcesInScene[1] == null)
                        activeCircle = Instantiate(radiusPref, mousePosition, Quaternion.identity).GetComponent<PulsatingCircle>();
                    else
                    {
                        activeCircle = activeCirlcesInScene[1];
                    }
                    activeCircle.target = mousePosition;
                    //activeCircle.gameObject.transform.position = mousePosition;
                    activeCircle.gameObject.SetActive(true);
                    if (activeCirlcesInScene[1] == null)
                        activeCirlcesInScene[1] = activeCircle;
                    if (!activeCirlcesInScene[0].gameObject.activeInHierarchy)
                        activeCirlcesInScene[0].gameObject.SetActive(true);
                    //activeCircle = null;
                }
             
                if (undir.magnitude < 0.2f && activeCirlcesInScene[0].GetComponent<SpriteRenderer>().color != Color.red)
                {
                    for (int i = 0; i < activeCirlcesInScene.Length; i++)
                        activeCirlcesInScene[i].GetComponent<SpriteRenderer>().color = Color.red;
                }
                else if (undir.magnitude > 0.2f && activeCirlcesInScene[0].GetComponent<SpriteRenderer>().color != Color.green) 
                {
                    for (int i = 0; i < activeCirlcesInScene.Length; i++)
                        activeCirlcesInScene[i].GetComponent<SpriteRenderer>().color = Color.green;
                }
            }
        }

        if (mouseClick.action.WasReleasedThisFrame())
        {
            slow = false;
            Debug.Log("released this frame");
            activeCircle = null;
            for (int i = 0; i < activeCirlcesInScene.Length; i++)
                activeCirlcesInScene[i].gameObject.SetActive(false);

            dirLn.enabled = false;
            dir = mousePosition - startPos;
            if (dir.magnitude > 0.2f)
            {
                rb.velocity = Vector2.zero;
                if (!GameManager.ins.dynWater.playerInWater)
                    rb.gravityScale = 1;
                float mag = dir.magnitude;
                dir = new Vector2(dir.x / mag, dir.y / mag);
                mag = Mathf.Clamp(mag, 0, 3);
                //Debug.Log(mag);
                rb.AddForce(-dir * mag * 2 * swingforce, ForceMode2D.Impulse);
                dustPart.Play();
                //rb.AddTorque(mag*5, ForceMode2D.Force);
            }
        }
    }
    #endregion

    #region Hole Trigger
    public void TriggerHoleEnter(Transform holeTransform)
    {
        canMove = false;
        if (rb.velocity != Vector2.zero)
        {
            storedRB = rb.velocity;
            rb.velocity = Vector2.zero;
        }
        if (holeTrans == null)
            holeTrans = holeTransform;
        if (rb.gravityScale != 0)
            rb.gravityScale = 0;

        Debug.Log(holeTransform.position);

        transform.position = Vector3.Lerp(transform.position, holeTransform.position, Time.deltaTime * storedRB.magnitude);
        if (spr.transform.localScale != new Vector3(0, 0, 0))
        {
            spr.transform.localScale = Vector3.Lerp(spr.transform.localScale, new Vector2(0, 0), Time.deltaTime);
            float degreesPerSecond = 60;

            transform.Rotate(new Vector3(0, 0, degreesPerSecond) * Time.deltaTime);
        }
    }
    #endregion

    #region EFFECTS
    public void SquishAndStretch()
    {
        if ((!nearingGround|| rb.velocity.y > 0) && (!nearingCeiling || rb.velocity.y < 0) && rb.velocity.magnitude > 3)
        {
            spr.transform.localScale = Vector3.Lerp(spr.transform.localScale, new Vector3(1 - Mathf.Abs(rb.velocity.normalized.y) / 10, 1 + Mathf.Abs(rb.velocity.normalized.y)/10, 1) , Time.deltaTime);
        }
        else if (Mathf.Abs(rb.velocity.y) > 3 && rb.velocity.magnitude > 3)
        {
            spr.transform.localScale = Vector3.Lerp(spr.transform.localScale, new Vector3(1 + Mathf.Abs(rb.velocity.normalized.y) / 10, 1 - Mathf.Abs(rb.velocity.normalized.y) / 10, 1), Time.deltaTime * 5);
        }
        else
        {
            spr.transform.localScale = Vector3.Lerp(spr.transform.localScale, new Vector3(1, 1, 1), Time.deltaTime * 5);
        }
    }
    #endregion

    #region GeneralCollisions
    public void HazardCols()
    {
        bool hitHazard = Physics2D.OverlapCircle(transform.position, circleRad, Hazards);
        if (hitHazard && !dead)
        {
            //all things needed for a hard reset thus far
            dead = !dead;
            slow = false;
            GameManager.ins.paused = false;
            spr.enabled = false;
            rb.velocity = Vector2.zero;
            for (int i = 0; i < activeCirlcesInScene.Length; i++)
                activeCirlcesInScene[i].gameObject.SetActive(false);
            transform.GetChild(2).GetComponent<TrailRenderer>().enabled = false;
            dirLn.enabled = false;
            canMove = false;
            rb.gravityScale = 0;
            deathPart.Play();
            GameManager.ins.ReloadScene();
        }
    }
    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
       // Gizmos.DrawWireSphere(transform.position, transform.localScale.x/ raycastOffset);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position - Vector2.up/1.6f);
        Gizmos.DrawLine(transform.position, transform.position - Vector3.up * 0.6f);
        Gizmos.DrawWireSphere(transform.position, circleRad);
        //Gizmos.DrawLine(transform.position, (Vector2)transform.position + (rb.velocity.normalized * 0.6f));
        //Gizmos.DrawLine(transform.position, (Vector3)transform.position + (Vector3.Cross(rb.velocity.normalized, Vector2.up) * 0.6f));
        //Gizmos.DrawLine(transform.position, (Vector3)transform.position + (Vector3.Cross(rb.velocity.normalized, Vector2.down) * 0.6f));

    }

}
