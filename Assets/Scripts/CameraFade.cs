using UnityEngine;
using System.Collections;

using UsefulThings;

public class CameraFade : _ChangeColor
{
    public Gradient whiteGradient;
    public Material fadeMaterial;

    bool isBlack;
    bool isFadingToWhite;

    void OnPostRender()
    {
        if (isFadingToWhite)
        {
            fadeMaterial.color = whiteGradient.Evaluate(curve.Evaluate(GetComponent<TimeKeeper>()));
        }
        else
        {
            fadeMaterial.color = isBlack ? Color.black : getColor();
        }
        fadeMaterial.SetPass(0);
        GL.PushMatrix();
        GL.LoadOrtho();
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

    public void CutFromBlack()
    {
        isBlack = false;
    }

    public void FadeToWhite() {
        GetComponent<TimeKeeper>().resetTime();
        isFadingToWhite = true;
    }
}
