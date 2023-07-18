using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractalManager : MonoBehaviour
{
    [SerializeField]
    GameObject refObj;

    
    // Start is called before the first frame update
    void Start()
    {
        refObj.GetComponent<CreateMesh>().SetAvatarMesh();
        GetComponent<AllDIrectionFractal>().SetMesh();

        GetComponent<Fractal>().EnableFractalCreation();
        GetComponent<Fractal>().DoFractalJob();
        GetComponent<AllDIrectionFractal>().CreateMeshFractal();
    }

    // Update is called once per frame
    void Update()
    {
        // GetComponent<Fractal>().DoFractalJob();
        refObj.GetComponent<CreateMesh>().SetAvatarMesh();
        GetComponent<AllDIrectionFractal>().SetMesh();

        GetComponent<AllDIrectionFractal>().EnableJobMeshFractal();

    }
}
