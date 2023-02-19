using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulsatingCircle : MonoBehaviour
{
    public Vector2 target;
    public Vector2 targetScale;
    public float rotationSpeed;
    float randPulseSize;
    // Start is called before the first frame update
    void Start()
    {
        targetScale = transform.localScale;
        transform.position = target;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.forward * (rotationSpeed * Time.deltaTime));
        transform.position = target;
       

        transform.localScale = Vector3.Slerp(transform.localScale, transform.localScale * randPulseSize, Time.deltaTime);

        if (Vector2.Distance(transform.localScale, targetScale) < 0.1f)
        {
            if (randPulseSize > 1)
                randPulseSize = 0.75f;
            else
                randPulseSize = 1.25f;
            targetScale = new Vector2(1, 1) * randPulseSize;
        }

    }

    private void OnEnable()
    {
        transform.position = target;
    }
}
