using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    SpriteRenderer spr;
    public LayerMask playerLayer;
    public float radius;
    public bool inRange = false;
    private bool grabbed = false;
    public Vector2 target;


    [Header("ANIMATION CONTROL")]
    public Sprite[] sprites = new Sprite[3];
    Timer animTimer;
    public float animationTime;
    int animDir = 1;
    int i = 0;
    int id = 0;

    // Start is called before the first frame update
    void Start()
    {
        spr = GetComponent<SpriteRenderer>();
        target = transform.position;

        animTimer = gameObject.AddComponent<Timer>();
        animTimer.Duration = animationTime;
        animTimer.AddTimerFinishedListener(() =>
        {
            if (i == sprites.Length-1 && animDir != -1)
                animDir = -1;
            else if (i == 0)
                animDir = 1;

            i = i + animDir;
            spr.sprite = sprites[i];
        });
    }

    // Update is called once per frame
    void Update()
    {
        inRange = Physics2D.OverlapCircle(transform.position, radius, playerLayer);
        if((inRange && target == (Vector2)transform.position) || grabbed)
        {
            target = GameManager.ins.hole.transform.position;
            grabbed = true;
            
        }

        if (grabbed && id == 0)
        {
            //transform.position = Vector2.Lerp(transform.position, target + new Vector2(spr.bounds.size.x/2, spr.bounds.size.y / 2), 5 * Time.deltaTime); old following player bit
            id = transform.LeanMove(target, 1).setEaseInQuart().setIgnoreTimeScale(true).id;
            GameManager.ins.paused = true;
        }

        if (GameManager.ins.paused)
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, 0.3f, 15 * Time.deltaTime);
        }

        if ((Vector2)transform.position == target && id != 0)
        {
            //Time.timeScale = 1;
            GameManager.ins.hole.Unlock();
            gameObject.SetActive(false);
            GameManager.ins.paused = false;
        }

        if (!animTimer.Running)
            animTimer.Run();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
