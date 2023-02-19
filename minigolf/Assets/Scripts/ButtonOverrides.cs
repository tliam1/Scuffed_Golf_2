using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonOverrides : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //Attach this script to the GameObject you would like to have mouse hovering detected on
    //This script outputs a message to the Console when the mouse pointer is currently detected hovering over the GameObject and also when the pointer leaves.
    //Detect if the Cursor starts to pass over the GameObject

    [HideInInspector] public GameObject parent;
    [HideInInspector] public Vector3 startPos;
    int id = 0;
    Button thisBut;

    private void Start()
    {
        parent = transform.parent.gameObject;
        startPos = parent.transform.position;
        thisBut = GetComponent<Button>();
    }

    private void Update()
    {
        if (!thisBut.interactable && id != -1)
        {
            LeanTween.cancel(id);
            id = -1;
        }
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        //Output to console the GameObject's name and the following message
        //Debug.Log("Cursor Entering " + name + " GameObject");
        if (thisBut.interactable)
        {
            LeanTween.cancel(id);
            id = LeanTween.move(parent, startPos + new Vector3(1f, 0.2f, 0), .2f).setEaseInOutQuart().id;
        }
        //LeanTween.cancel(id);
    }

    //Detect when Cursor leaves the GameObject
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        //Output the following message with the GameObject's name
        //Debug.Log("Cursor Exiting " + name + " GameObject");
        if (thisBut.interactable)
        {
            LeanTween.cancel(id);
            id = LeanTween.move(parent, startPos, .2f).setEaseInOutQuart().id;
        }
    }
}
