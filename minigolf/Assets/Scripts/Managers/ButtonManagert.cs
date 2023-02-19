using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
public class ButtonManagert : MonoBehaviour
{
    //Timer transitionTimer;
    //public float transitionTime;
    [Header("Buttons")]
    public Button SettingsButton;

    [Header("TransitionShader")]
    [SerializeField]
    private Material screenTransMaterial;

    [SerializeField]
    private float transitionTime;

    [SerializeField]
    private string propertyName = "_Progress"; // this is the float value within the shader... how tf does this fucking work

    public UnityEvent OnOpenTransitionFinished, OnCloseTransitionFinished; // assigned in inspector (Do in script once you google how tf to assign this at runtime)

    // Start is called before the first frame update
    void Start()
    {
/*        transitionTimer = gameObject.AddComponent<Timer>();
        transitionTimer.Duration = transitionTime;
        transitionTimer.AddTimerFinishedListener(() =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        });*/

        StartCoroutine(TransitionTimerOpen());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TriggerLeaveTransition()
    {
        StartCoroutine(TransitionTimerClose());
        //StartCoroutine(AnimateVertexColors());
    }

    public void SceneTrigger()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void TransitionToNewScene(Button playButton)
    {
        //transitionTimer.Run();
        playButton.interactable = false;
        SettingsButton.interactable = false;
        ButtonOverrides butOverride = playButton.GetComponent<ButtonOverrides>();
        LeanTween.move(butOverride.parent, butOverride.startPos + new Vector3(10f, 0.2f, 0), 0.5f).setEaseInOutQuart();
        LeanTween.move(SettingsButton.gameObject.transform.parent.gameObject, SettingsButton.gameObject.transform.parent.gameObject.transform.position - new Vector3(5f, 0.2f, 0), 0.5f).setEaseInOutQuart();
        TriggerLeaveTransition();
    }



    IEnumerator TransitionTimerOpen()
    {
        float currentTime = 0;
        while (currentTime < transitionTime)
        {
            currentTime += Time.deltaTime;
            screenTransMaterial.SetFloat(propertyName, Mathf.Clamp01(currentTime / transitionTime));
            yield return null;
        }
        OnOpenTransitionFinished?.Invoke();
    }

    IEnumerator TransitionTimerClose() // called by gamemanager when hole is entered
    {
        float currentTime = 0;
        while (currentTime < transitionTime)
        {
            currentTime += Time.deltaTime;
            screenTransMaterial.SetFloat(propertyName, 1 - Mathf.Clamp01(currentTime / transitionTime));
            yield return null;
        }
        OnCloseTransitionFinished?.Invoke();
    }
}
