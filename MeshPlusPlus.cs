using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MeshPlusPlus
{
    public Mesh mesh;
    public Vector3[] strokePoints;
    public Material material;

    public MeshPlusPlus(Mesh _mesh, Vector3[] _strokePoints)
    {
        this.mesh = _mesh;
        this.strokePoints = _strokePoints;
    }
}
