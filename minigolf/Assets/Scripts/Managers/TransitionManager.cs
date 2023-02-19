using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class TransitionManager : MonoBehaviour
{
    [Header("Bools")]
    public bool closeTransitionEnded = false;
    [Header("Floats")]
    public float sinWaveStrength = 10;
    public float sinWiggleSpeed = 1;
    private float totTime;
    [Header("Text")]
    public TMP_Text wigglyText;
    public TMP_Text clickToContinueText;
    [SerializeField]
    Gradient gradientText;
    [SerializeField]
    float gradientSpeed = 1;
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
        StartCoroutine(TransitionTimerOpen());
    }

    // Update is called once per frame
    void Update()
    {
     
    }

    public void TriggerLeaveTransition()
    {
        StartCoroutine(TransitionTimerClose());
        StartCoroutine(AnimateVertexColors());
    }

    public void TriggerDeathLeaveTransition()
    {
        StartCoroutine(TransitionTimerClose());
    }

    #region TEXT

    public void WiggleText()
    {
        //wigglyText.ForceMeshUpdate();
        var textInfo = wigglyText.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible)
            {
                continue;
            }

            var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

            for (int j = 0; j < 4; ++j)
            {
                var orig = verts[charInfo.vertexIndex + j];
                verts[charInfo.vertexIndex + j] = orig + new Vector3(0, Mathf.Sin(Time.time * sinWiggleSpeed + orig.x * 0.01f) * sinWaveStrength, 0);
            }
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            wigglyText.UpdateGeometry(meshInfo.mesh, i);
        }
    }

    IEnumerator AnimateVertexColors()
    {
        wigglyText.ForceMeshUpdate();

        TMP_TextInfo textInfo = wigglyText.textInfo;
        int currentChar = 0;

        Color32[] newVertexColors;
        Color32 c0 = gradientText.Evaluate(0f);
        Color32 c1 = wigglyText.color;

        while (true)
        {
            int charCount = textInfo.characterCount;
            if (charCount == 0)
            {
                yield return new WaitForSeconds(0.25f);
                continue;
            }

            int materialIndex = textInfo.characterInfo[currentChar].materialReferenceIndex;

            newVertexColors = textInfo.meshInfo[materialIndex].colors32;

            int vertexIndex = textInfo.characterInfo[currentChar].vertexIndex;

            if (textInfo.characterInfo[currentChar].isVisible)
            {
                //c0 = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 255);
                float offset = (currentChar / charCount);
                c1 = gradientText.Evaluate((totTime + offset) % 1);
                totTime += Time.deltaTime;

                c0.a = (byte)200f;
                c1.a = (byte)200f;

                newVertexColors[vertexIndex + 0] = c0;
                newVertexColors[vertexIndex + 1] = c0;
                newVertexColors[vertexIndex + 2] = c1;
                newVertexColors[vertexIndex + 3] = c1;

                c0 = c1;

                wigglyText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            }

            currentChar = (currentChar + 1) % charCount;
            WiggleText();



            yield return new WaitForSeconds(gradientSpeed);
        }
    }
    #endregion

    #region Shaders
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
            screenTransMaterial.SetFloat(propertyName, 1-Mathf.Clamp01(currentTime / transitionTime));
            yield return null;
        }
        OnCloseTransitionFinished?.Invoke();
    }


    public void AfterTransitionOpen()
    {
        GameManager.ins.golfBall.canMove = true;
        // have gamemanager Instantiate the player at spawn pos
    }

    public void AfterTransitionClose()
    {
        closeTransitionEnded = true;
    }
    #endregion
}
