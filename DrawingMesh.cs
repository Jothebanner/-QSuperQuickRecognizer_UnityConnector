using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DrawingMesh : MonoBehaviour
{
    [SerializeField] float newSegmentDistance = 0.05f;
    [SerializeField] float lineWidth = 0.05f;

    public Mesh currentMesh;
    public Material material;

    const int meshHeight = 2;

    int[] triangles;

    [SerializeField] List<Vector3> vertices = new List<Vector3> ();

    List<Vector3> meshPoints = new List<Vector3>();

    [SerializeField] Vector3 lastPosition;

    bool generateInitalEdges = false;


    private void Start()
    {
        CreateNewMeshStroke();
    }

    void Update()
    {
        //for (int i = 0; i < meshPoints.Count; i++)
        //{
        //    if (i - 1 >= 0)
        //        Debug.DrawRay(meshPoints[i - 1], meshPoints[i] - meshPoints[i - 1], Color.red);
        //}

        RaycastHit hit;
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);

        Vector3 hitPoint = new Vector3(hit.point.x, 1e-17f, hit.point.z);

        Vector3[] sides = ProcessVerticesFromPoint(hitPoint);

        var leftPoint = sides[0];

        var rightPoint = sides[1];

        Debug.DrawLine(leftPoint, rightPoint);

        //Debug.DrawRay(lastPosition, hitPoint - lastPosition, Color.red);

        // will make the mesh appear in the Scene at origin position
        Graphics.DrawMesh(currentMesh, Vector3.zero, Quaternion.identity, material, 0);
    }

    public void AddToMeshList()
    {
        GestureManager.Instance.meshPlusPlusList.Add(new MeshPlusPlus(currentMesh, meshPoints.ToArray()));
        CreateNewMeshStroke();
        ClearData();
    }
    public void CreateNewMeshStroke()
    {
        currentMesh = new Mesh();
    }
    void ClearData()
    {
        meshPoints = new List<Vector3>();
        vertices = new List<Vector3>();
        lastPosition = Vector3.zero;
        generateInitalEdges = false;
    }

    public void AddPointToMesh(RaycastHit hit)
    {

        Vector3 hitPoint = new Vector3(hit.point.x, 0.0001f, hit.point.z);

        // if there is a last postion
        if (lastPosition != Vector3.zero)
        {
            //Debug.Log((hit.point - lastPosition).magnitude);
            // then check distance
            if ((hit.point - lastPosition).magnitude > newSegmentDistance)
            {
                meshPoints.Add(hit.point);

                var newVertices = ProcessVerticesFromPoint(hitPoint);

                var leftPoint = newVertices[0];
                var rightPoint = newVertices[1];

                vertices.Add(leftPoint);
                vertices.Add(rightPoint);

                currentMesh.vertices = vertices.ToArray();

                if (generateInitalEdges)
                {
                    var initalLeftPoint = leftPoint - (hitPoint - lastPosition);
                    var initalRightPoint = rightPoint - (hitPoint - lastPosition);
                    vertices.Insert(0, initalLeftPoint);
                    vertices.Insert(1, initalRightPoint);
                    generateInitalEdges = false;
                }

                lastPosition = meshPoints.Last(); // this 100% needs to be after proccessvertiesfrompoint
                if (vertices.Count > 4)
                    GenerateTriangles();
            }
        }
        else // add the initial point
        {
            Debug.Log("hitting else");
            meshPoints.Add(hitPoint);
            lastPosition = meshPoints.Last();
            generateInitalEdges = true;
        }
    }

    Vector3[] ProcessVerticesFromPoint(Vector3 point)
    {
        var newVec = point - lastPosition;
        var newVector = Vector3.Cross(newVec, Vector3.up);
        newVector.Normalize();

        var leftPoint = lineWidth * newVector + point;
        var rightPoint = -lineWidth * newVector + point;

        return new Vector3[]{leftPoint, rightPoint};
    }

    //TODO: add function to just generate the added triangles instead of everything
    void GenerateTriangles()
    {
        int meshLength = meshPoints.Count - 1;

        triangles = new int[meshLength * meshHeight * 6]; // mesh height is always 2 idk, im probably just an idiot

        int vert = 0;
        int tris = 0;
        for (int i = 0; i <= meshHeight -1; i++)
        {
            for (int x = i; x < meshLength; x+=2)
            {
                int a = vert;
                int b = vert + 1;
                int c = vert + 3;
                int d = vert + 2;

                triangles[tris + 0] = d;
                triangles[tris + 1] = b;
                triangles[tris + 2] = a;
                triangles[tris + 3] = b;
                triangles[tris + 4] = d;
                triangles[tris + 5] = c;

                vert+=2;
                tris += 6;

            }
        }

        Vector2[] uvs;

        uvs = new Vector2[vertices.Count];

        for (int i = 0, z = 0; z <= meshHeight -1; z++)
        {
            for (int x = 0; x <= meshLength/2; x++)
            {
                uvs[i] = new Vector2(x, z);

                i++;
            }
        }

        Color[] colors = new Color[vertices.Count];
        for (int i = 0, z = 0; z <= meshHeight - 1; z++)
        {
            for (int x = z; x <= meshLength; x += 2)
            {
                colors[i] = Color.white;

                i++;
            }
        }

        currentMesh.triangles = triangles;
        currentMesh.uv = uvs;
        currentMesh.colors = colors;

        currentMesh.RecalculateNormals();
    }


    
}
