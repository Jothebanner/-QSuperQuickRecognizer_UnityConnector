using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDollarGestureRecognizer;
using QDollarGestureRecognizer;
using System.Linq;

public class GestureManager : MonoBehaviour
{

	#region Singleton
	private static GestureManager _instance;

	public static GestureManager Instance => _instance;

	private void Awake()
	{
		//TODO: I don't think this will work for multiplayer
		if (_instance != null)
		{
			Destroy(gameObject);
		}
		// this bit ^
		else
		{
			_instance = this;
		}
	}
	#endregion
	[SerializeField] GameObject cube;
	[SerializeField] CharacterInputs characterInputs;

	[SerializeField] float checkDistance = 5;

	[SerializeField] Material _meshMaterial;
	[SerializeField] Material activatedMeshMaterial;

	[SerializeField] public List<MeshPlusPlus> meshPlusPlusList = new List<MeshPlusPlus>();

	public List<Vector3[]> strokeList = new List<Vector3[]>();

	MaterialPropertyBlock _materialPropertyBlock;

    private void Update()
    {
        
    }

    private void LateUpdate()
    {
		foreach (MeshPlusPlus meshplusplus in meshPlusPlusList)
		{
			Material mat;
			if (meshplusplus.material != null)
				mat = meshplusplus.material;
			else
				mat = _meshMaterial;
			//Graphics.DrawMesh(meshplusplus.mesh, Vector3.zero, Quaternion.identity, _meshMaterial, 0);
			Graphics.DrawMesh(meshplusplus.mesh, Vector3.zero, Quaternion.identity, mat, 0, null, 0, _materialPropertyBlock, true, true);
		}
	}

    //public void TestRecognizer()
    //   {
    //	Debug.Log(meshPlusPlusList[0].strokePoints.Length);
    //	CombineStrokes(meshPlusPlusList.ToArray());

    //	// TODO combine meshes if they're close enough
    //   }

    public void CombineCloseStrokes(MeshPlusPlus activeMesh)
    {
		List<MeshPlusPlus> combinedStrokes =  new List<MeshPlusPlus>();

		combinedStrokes.Add(activeMesh);

		foreach (Vector3 point in activeMesh.strokePoints)
        {
			foreach (MeshPlusPlus otherMesh in meshPlusPlusList)
            {
				if (otherMesh != activeMesh && !combinedStrokes.Contains(otherMesh))
				{
					foreach (Vector3 otherPoint in otherMesh.strokePoints)
					{
						if (Vector3.Distance(point, otherPoint) < checkDistance)
						{
							combinedStrokes.Add(otherMesh);
							break;
						}
					}
				}
            }
        }

		CombineStrokes(combinedStrokes.ToArray());
    }


    void CombineStrokes(MeshPlusPlus[] meshStrokes)
    {
		foreach (MeshPlusPlus meshStroke in meshStrokes)
			strokeList.Add(meshStroke.strokePoints);

		Point[] points = GetComponent<GestureConnector>().PointExtractor(strokeList);
		Debug.Log(GetComponent<GestureConnector>().CheckCandidate(points));

		if (GetComponent<GestureConnector>().CheckCandidate(points) == "Pentagram")
		{
			foreach (MeshPlusPlus mesh in meshStrokes)
			{
				meshPlusPlusList.Find(strokeMesh => strokeMesh == mesh).material = activatedMeshMaterial;
			}

			MagicCircle magicCircle = new MagicCircle(meshStrokes);

			characterInputs.spawnPoint = magicCircle.CirlceCenter;

			//MakeCube(magicCircle.MCBoundaries.Upper, "top");
			//MakeCube(magicCircle.MCBoundaries.Lower);
			//MakeCube(magicCircle.MCBoundaries.Left);
			//MakeCube(magicCircle.MCBoundaries.Right);
			//MakeCube(magicCircle.CirlceCenter);
			//Instantiate(cube, magicCircle.MCBoundaries.UpperLeft, Quaternion.identity);


		}
		strokeList.Clear();
    }

	public GameObject MakeCube(Vector3 position, string name = "Cube", Color color = default(Color))
    {
		GameObject Cube = Instantiate(cube, position, Quaternion.identity);
		Cube.name = name;
		if (color != default(Color))
        {
			Cube.transform.position += new Vector3(0, 1, 0);
        }
		return Cube;
	}

	public MeshPlusPlus FindClosestStroke(Vector3 position)
    {
		MeshPlusPlus closestMesh = null;
		float closestPointDistance = Mathf.Infinity;

		foreach (MeshPlusPlus stroke in meshPlusPlusList)
        {
			foreach (Vector3 point in stroke.strokePoints)
            {
				if (Vector3.Distance(point, position) < closestPointDistance)
                {
					closestPointDistance = Vector3.Distance(point, position);
					closestMesh = stroke;
                }
            }
        }

		if (closestMesh != null)
			return closestMesh;
		else
			return null;
    }
}

