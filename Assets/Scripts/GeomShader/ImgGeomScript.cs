using UnityEngine;
using System.Collections;

public class ImgGeomScript : MonoBehaviour
{
    public Shader geomShader;
    Material material;

    public Texture2D sprite;
    public Vector2 size = Vector2.one;
    public Color color = new Color(1.0f, 0.6f, 0.3f, 0.03f);

    ComputeBuffer outputBuffer;

    // ComputeShaderOutputKT cso;
    ConfettiShaderOutput cfso;

    [Range(0,2)]
    [Tooltip("Billboard type 0 = static, 1 = Cylindrical, 2 = Sphecrical")]
    public int billboardType = 2;

    data[] points;

    public bool UseComputeData = false;

    struct data
    {
        public Vector3 pos;
    };

    // Use this for initialization
    void Start()
    {
        material = new Material(geomShader);

        /*
        points = new data[]
        {
            new data() { pos = Vector3.left + Vector3.up },
            new data() { pos = Vector3.right + Vector3.up },
            new data() { pos = Vector3.zero},
            new data() { pos = Vector3.left + Vector3.down },
            new data() { pos = Vector3.right + Vector3.down}
        };
        */

        points = new data[] { new data() { pos = Vector3.zero } };

        if (!UseComputeData)
        {
            outputBuffer = new ComputeBuffer(points.Length, 12);
            outputBuffer.SetData(points);
        }

    }

    void OnRenderObject()
    {
        if (UseComputeData)
        {
            //cso = GetComponent<ComputeShaderOutputKT>();
            cfso = GetComponent<ConfettiShaderOutput>();

            /*
            if (outputBuffer == null && cso != null)
                outputBuffer = cso.outputBuffer;
            */
            if (outputBuffer == null && cfso != null)
                outputBuffer = cfso.outputBuffer;

            if (cfso != null) {
                cfso.Dispatch();
            }
        }

        Color temp = Random.ColorHSV();

        material.SetPass(0);
        material.SetColor("_Color", color);
        material.SetBuffer("buf_Points", outputBuffer);
        material.SetTexture("_Sprite", sprite);
        material.SetVector("_Size", size);
        material.SetInt("_StaticCylinderSpherical", billboardType);
        material.SetVector("_worldPos", transform.position);

        Graphics.DrawProcedural(MeshTopology.Points, outputBuffer.count);
    }

    void OnDestroy()
    {
        outputBuffer.Release();
    }
}
