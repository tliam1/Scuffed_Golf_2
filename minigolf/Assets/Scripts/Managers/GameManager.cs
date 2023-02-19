using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager ins;
    private void Awake()
    {
        if (ins)
            Destroy(this);
        ins = this;
    }

    public Ball_Physics golfBall;
    public Hole hole;
    public DynamicWater dynWater;
    TransitionManager transManager;
    private bool levelfinished = false;
    public bool paused = false;

    [Header("Timers")]
    public float reloadSceneTime;
    Timer reloadSceneTimer;


    // Start is called before the first frame update
    void Start()
    {
        transManager = GetComponent<TransitionManager>();

        reloadSceneTimer = gameObject.AddComponent<Timer>();
        reloadSceneTimer.Duration = reloadSceneTime;
        reloadSceneTimer.AddTimerFinishedListener(() =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (hole.ballEntered && golfBall.spr.transform.localScale.magnitude < 0.5f && !levelfinished)
        {
            levelfinished = true;
            transManager.TriggerLeaveTransition();
        }

        if(transManager.closeTransitionEnded && transManager.wigglyText.transform.localScale != Vector3.one && !golfBall.dead)
        {
            transManager.wigglyText.transform.localScale = Vector3.Lerp(transManager.wigglyText.transform.localScale, Vector3.one, Time.deltaTime * 2);
            transManager.clickToContinueText.transform.position = Vector3.Lerp(transManager.clickToContinueText.transform.position, Camera.main.transform.position - new Vector3(0, 3f, -10), Time.deltaTime * 2);
        }


        if (golfBall.mouseClick.action.WasPressedThisFrame() && transManager.closeTransitionEnded && transManager.wigglyText.transform.localScale.magnitude >= 1.5f)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }


    public void ReloadScene()
    {
        Time.timeScale = 1f;
        StartCoroutine(Stall(0.5f));
    }

    IEnumerator Stall(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        transManager.TriggerDeathLeaveTransition();
        if (!reloadSceneTimer.Running)
            reloadSceneTimer.Run();
    }
}
