using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Icosphere : MonoBehaviour
{

    [SerializeField] private int scale;
    [Range(0, 6)]
    [SerializeField] private int numIterations;

    const float GOLDENRATIO = 1.61803f;


    List<Vector3> vertList = new List<Vector3>();
    List<Vector3> neighbours = new List<Vector3>();
    Color[] colors;
    List<int> triList = new List<int>();
    Mesh mesh;
    List<int> tempTriList;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        mesh.name = "PlanetMesh";

        GetComponent<MeshFilter>().mesh = mesh;
        generateSphere();
        //Create collider
        GetComponent<MeshCollider>().sharedMesh = mesh;
        //generateTerrain();

        colors = new Color[vertList.Count];
        for (int i = 0; i < vertList.Count; i++)
        {
            colors[i] = Color.Lerp(Color.blue, Color.red, vertList[i].y);
        }

        generateMesh();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void generateSphere()
    {

        //create base veticies for isocahedron https://en.wikipedia.org/wiki/Icosahedron
        vertList.Add(new Vector3(-1, GOLDENRATIO, 0).normalized);
        vertList.Add(new Vector3(1, GOLDENRATIO, 0).normalized);
        vertList.Add(new Vector3(-1, -GOLDENRATIO, 0).normalized);
        vertList.Add(new Vector3(1, -GOLDENRATIO, 0).normalized);
        vertList.Add(new Vector3(0, -1, GOLDENRATIO).normalized);
        vertList.Add(new Vector3(0, 1, GOLDENRATIO).normalized);
        vertList.Add(new Vector3(0, -1, -GOLDENRATIO).normalized);
        vertList.Add(new Vector3(0, 1, -GOLDENRATIO).normalized);
        vertList.Add(new Vector3(GOLDENRATIO, 0, -1).normalized);
        vertList.Add(new Vector3(GOLDENRATIO, 0, 1).normalized);
        vertList.Add(new Vector3(-GOLDENRATIO, 0, -1).normalized);
        vertList.Add(new Vector3(-GOLDENRATIO, 0, 1).normalized);


        //create base triangles array
        int[] newTriangles = new int[] {
            //# 5 faces around point 0
            0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11, 
            //Adjacent faces 
            1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8, 
            //5 faces around 3
            3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
            //Adjacent faces
            4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1 };

        //convert to list
        for (int i = 0; i < newTriangles.Length; i++)
        {
            triList.Add(newTriangles[i]);
        }

        //list to store the triangles created at each recursive step
        tempTriList = new List<int>();


        int numberOfTriangles = triList.Count;
        for (int i = 0; i < numIterations; i++)
        {
            //remove outertriangles from 
            tempTriList.Clear();
            //Get each triangle from list of triangles
            for (int j = 0; j < numberOfTriangles; j += 3)
            {

                Vector3 vert1 = vertList[triList[j]];
                Vector3 vert2 = vertList[triList[j + 1]];
                Vector3 vert3 = vertList[triList[j + 2]];

                //calculate mid point of each edge of the trianle for the new verticies to be created .
                Vector3 midPointA = Vector3.Lerp(vert1, vert2, 0.5f).normalized;
                Vector3 midPointB = Vector3.Lerp(vert1, vert3, 0.5f).normalized;
                Vector3 midPointC = Vector3.Lerp(vert2, vert3, 0.5f).normalized;

                int indexOfMidPointA = 0;
                int indexOfMidPointB = 0;
                int indexOfMidPointC = 0;

                //check if the midpoint of new triangle has already been created for previous triangles.
                //only calls index of where neccesary to avoid O(n) operation.
                //in future creating a hashed list to avoid calling .contains can be used if slow.
                if (!vertList.Contains(midPointA))
                {
                    vertList.Add(midPointA);
                    indexOfMidPointA = vertList.Count - 1;
                }
                else
                {
                    indexOfMidPointA = vertList.IndexOf(midPointA);
                }

                if (!vertList.Contains(midPointB))
                {
                    vertList.Add(midPointB);
                    indexOfMidPointB = vertList.Count - 1;
                }
                else
                {
                    indexOfMidPointB = vertList.IndexOf(midPointB);
                }

                if (!vertList.Contains(midPointC))
                {
                    vertList.Add(midPointC);
                    indexOfMidPointC = vertList.Count - 1;
                }
                else
                {
                    indexOfMidPointC = vertList.IndexOf(midPointC);
                }



                //Creates the four new triangles clockwise


                //index of vert 1
                tempTriList.Add(triList[j]);
                tempTriList.Add(indexOfMidPointA);
                tempTriList.Add(indexOfMidPointB);

                //index of vert 2
                tempTriList.Add(triList[j + 1]);
                tempTriList.Add(indexOfMidPointC);
                tempTriList.Add(indexOfMidPointA);

                //middle Triangle
                tempTriList.Add(indexOfMidPointA);
                tempTriList.Add(indexOfMidPointC);
                tempTriList.Add(indexOfMidPointB);

                //index of vert 3    
                tempTriList.Add(triList[j + 2]);
                tempTriList.Add(indexOfMidPointB);
                tempTriList.Add(indexOfMidPointC);

            }
            //only iterate on the newly created triangles. 
            triList = tempTriList.ToList();

            numberOfTriangles = triList.Count;

        }


        Debug.Log("Number of verticies = " + vertList.Count);
        Debug.Log("Number of Triangles = " + tempTriList.Count / 3);


        generateMesh();
    }

    private void generateMesh()
    {
        mesh.vertices = vertList.ToArray();
        mesh.normals = vertList.ToArray();
        mesh.triangles = tempTriList.ToArray();
        mesh.colors = colors;
        GetComponent<MeshCollider>().sharedMesh = mesh;

    }

    private void scaleSphere()
    {
        for (int i = 0; i < vertList.Count; i++)
        {
            vertList[i] *= scale;
        }
    }

}
