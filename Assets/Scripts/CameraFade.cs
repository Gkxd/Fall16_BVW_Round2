using UnityEngine;
using System.Collections;

using UsefulThings;

public class CameraFade : _ChangeColor
{

    public Material fadeMaterial;

    bool isBlack;

    void OnPostRender()
    {
        fadeMaterial.color = isBlack ? Color.black : getColor();
        fadeMaterial.SetPass(0);
        GL.PushMatrix();
        GL.LoadOrtho();
        //GL.Color(getColor());
        GL.Begin(GL.QUADS);
        GL.Vertex3(0f, 0f, -12f);
        GL.Vertex3(0f, 1f, -12f);
        GL.Vertex3(1f, 1f, -12f);
        GL.Vertex3(1f, 0f, -12f);
        GL.End();
        GL.PopMatrix();
    }

    public void CutToBlack()
    {
        isBlack = true;
    }
}
