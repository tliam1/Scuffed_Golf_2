using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hole : MonoBehaviour
{
    SpriteRenderer spr;
    public bool ballEntered = false;
    public bool locked = false;
    public float holeDetectionRange;
    public GameObject key;
    private GameObject lockGO, unlockPartGO;

    private void Awake()
    {
        spr = GetComponent<SpriteRenderer>();
        lockGO = transform.GetChild(0).gameObject;
        lockGO.SetActive(false);
        unlockPartGO = transform.GetChild(1).gameObject;
    }

    // Start is called before the first frame update
    void Start()
    {
        if ((key=GameObject.FindGameObjectWithTag("Key")) != null)
        {
            locked = true;
            lockGO.SetActive(true);
            var a = spr.color;
            a.a = 0.5f;
            spr.color = a;
            a = lockGO.GetComponent<SpriteRenderer>().color;
            a.a = 0.5f;
            lockGO.GetComponent<SpriteRenderer>().color = a;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!ballEntered && Vector2.Distance(GameManager.ins.golfBall.gameObject.transform.position, transform.position) < holeDetectionRange && !locked)
        {
            ballEntered = true;
            GameManager.ins.golfBall.TriggerHoleEnter(this.transform);
        }
    }

    public void Unlock()
    {
        if (locked)
        {
            lockGO.SetActive(false);
            var a = spr.color;
            a.a = 1;
            spr.color = a;
            locked = false;
            unlockPartGO.GetComponent<ParticleSystem>().Play();
/*            a = lockGO.GetComponent<SpriteRenderer>().color;
            a.a = 0.5f;
            lockGO.GetComponent<SpriteRenderer>().color = a;*/
        }
    }
}
