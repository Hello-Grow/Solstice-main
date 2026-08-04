using System.Collections.Generic;
using UnityEngine;

public class LightManager : Singleton<LightManager>
{
    public List<Transform> godRayLightsTransforms = new List<Transform>();
    public List<float> godRayLightsRadiuses = new List<float>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
