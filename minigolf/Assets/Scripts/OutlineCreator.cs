using System.Collections.Generic;
using UnityEngine;

public class OutlineCreator : MonoBehaviour
{
    public float lineWidth;
    public Material material;
    LineRenderer line;
    Vector3[] vertices;
    Vector3 bringFoward;
    void Start()
    {

    }


    public void Initialize(int len)
    {
        // Stop if no mesh filter exists
        if (GetComponent<MeshFilter>() == null)
        {
            return;
        }

        vertices = GetComponent<DynamicWater>().vertices;


        // Create line prefab
        LineRenderer linePrefab = new GameObject().AddComponent<LineRenderer>();
        linePrefab.transform.name = "Line";
        linePrefab.positionCount = 0;
        linePrefab.material = material;
        linePrefab.startWidth = linePrefab.endWidth = lineWidth;

        // Create first line
        line = Instantiate(linePrefab.gameObject).GetComponent<LineRenderer>();
        line.transform.parent = transform;

        // This vector3 gets added to each line position, so it sits in front of the mesh
        // Change the -0.1f to a positive number and it will sit behind the mesh
        bringFoward = new Vector3(0f, 0f, -0.1f);

        while (len != 1)
        {
            // Add to line
            line.positionCount++;
            line.SetPosition(line.positionCount-1, vertices[len]);
            len--;
        }
        line.positionCount = 48;

        line.startWidth = 0.2f; line.endWidth = 0.2f;
        line.startColor = new Color(1, 1, 1, 0.5f);
        line.endColor = new Color(1,1,1,0.5f);
    }

    public void UpdateLine(int len)
    {
        for (int i = 0; i <line.positionCount; i++)
        {
            line.SetPosition(i, vertices[i]);
        }
    }
}