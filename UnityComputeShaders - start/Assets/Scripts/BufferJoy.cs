﻿using UnityEngine;
using System.Collections;

public class BufferJoy : MonoBehaviour
{

    public ComputeShader shader;
    public int texResolution = 1024;

    Renderer rend;
    RenderTexture outputTexture;

    int circlesHandle;
    int clearHandle;

    struct Circle
    {
        public Vector2 origin;
        public Vector2 velocity;
        public float radius;
    }

    public Color clearColor = new Color();
    public Color circleColor = new Color();

    int count = 10;

    Circle[] circleData;
    ComputeBuffer buffer;

    // Use this for initialization
    void Start()
    {
        outputTexture = new RenderTexture(texResolution, texResolution, 0);
        outputTexture.enableRandomWrite = true;
        outputTexture.Create();

        rend = GetComponent<Renderer>();
        rend.enabled = true;

        InitData();

        InitShader();
    }

    private void InitData()
    {
        circlesHandle = shader.FindKernel("Circles");

        uint threadGroupSizeX;
        shader.GetKernelThreadGroupSizes(circlesHandle, out threadGroupSizeX, out _, out _);
        int total = (int) threadGroupSizeX * count;
        circleData = new Circle[total];

        float speed = 100;
        float halfSpeed = speed * 0.5f;
        float minRaidus = 10.0f;
        float maxRadius = 30.0f;
        float raidusRange = maxRadius - minRaidus;

        for (int i = 0; i < total; i++)
        {
            Circle circle = circleData[i];
            circle.origin.x = Random.value * texResolution;
            circle.origin.y = Random.value * texResolution;
            circle.velocity.x = Random.value * speed - halfSpeed;
            circle.velocity.y = Random.value * speed - halfSpeed;
            circle.radius = Random.value * raidusRange + minRaidus;
            circleData[i] = circle;
        }
    }

    private void InitShader()
    {
    	clearHandle = shader.FindKernel("Clear");
    	
        shader.SetVector( "clearColor", clearColor );
        shader.SetVector( "circleColor", circleColor );
        shader.SetInt( "texResolution", texResolution );
		
		shader.SetTexture( clearHandle, "Result", outputTexture );
        shader.SetTexture( circlesHandle, "Result", outputTexture );

        int stride = (2 + 2 + 1) * sizeof(float); // float2 + float2 + float * size(float)
        buffer = new ComputeBuffer(circleData.Length, stride);
        buffer.SetData(circleData);
        shader.SetBuffer(circlesHandle, "circleBuffer", buffer);

        rend.material.SetTexture("_MainTex", outputTexture);
    }
 
    private void DispatchKernels(int count)
    {
    	shader.Dispatch(clearHandle, texResolution/8, texResolution/8, 1);
        shader.SetFloat("time", Time.time);
        shader.Dispatch(circlesHandle, count, 1, 1);
    }

    void Update()
    {
        DispatchKernels(count);
    }
}

