using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateMesh : MonoBehaviour
{
    [SerializeField]
    private GameObject referenceMesh;
    Mesh thisMesh;

    private void Update()
    {
        SetAvatarMesh();
    }



    public virtual void SetAvatarMesh()
    {
        SetMesh();
    }

    void SetMesh()
    {
        SkinnedMeshRenderer skin = referenceMesh.GetComponent<SkinnedMeshRenderer>();

        Mesh bakedMesh = new Mesh();
        skin.BakeMesh(bakedMesh);

        AvatarMeshNow.avatarMesh = bakedMesh;
    }
}
