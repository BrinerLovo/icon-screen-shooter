using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ScreenShotUI : MonoBehaviour
{
    public Rect ScreenShotRect = new Rect(0, 0, 100, 100);
    public RenderTexture PreviewRender;
    public Camera TempCamera;

    public bool FromCenter = true;
    private Texture2D whiteTexture;

    private void Awake()
    {
        whiteTexture = Texture2D.whiteTexture;
    }

    private void OnGUI()
    {
        DrawRect();
    }

    void DrawRect()
    {
        if(FromCenter)
        {
            Vector2 Center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Vector2 HalfSize = new Vector2(ScreenShotRect.width * 0.5f, ScreenShotRect.height * 0.5f);
            GUI.DrawTexture(new Rect(Center.x - HalfSize.x, Center.y - HalfSize.y, 2, ScreenShotRect.height), whiteTexture, ScaleMode.StretchToFill);
            GUI.DrawTexture(new Rect(Center.x + HalfSize.x, Center.y - HalfSize.y, 2, ScreenShotRect.height), whiteTexture, ScaleMode.StretchToFill);
            GUI.DrawTexture(new Rect(Center.x - HalfSize.x, Center.y - HalfSize.y, ScreenShotRect.width, 2), whiteTexture, ScaleMode.StretchToFill);
            GUI.DrawTexture(new Rect(Center.x - HalfSize.x, Center.y + HalfSize.y, ScreenShotRect.width + 2, 2), whiteTexture, ScaleMode.StretchToFill);

            GUI.color = new Color(1, 1, 1, 0.3f);
            GUI.DrawTexture(new Rect(Center.x - 1, Center.y - HalfSize.y, 2, ScreenShotRect.height), whiteTexture, ScaleMode.StretchToFill);
            GUI.DrawTexture(new Rect(Center.x - HalfSize.x, Center.y - 1, ScreenShotRect.width, 2), whiteTexture, ScaleMode.StretchToFill);
        }
        GUI.Label(new Rect(0, Screen.height - 40, 200, 25), string.Format("Screen {0} x {1}", Screen.width, Screen.height));
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, transform.forward);
        Gizmos.DrawWireCube(transform.position + transform.forward, Vector3.one * 0.1f);
    }

    private void OnValidate()
    {
        if(PreviewRender != null)
        {

        }
    }

    public Vector2 Position()
    {
        if (FromCenter)
        {
            return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }
        else
        {
            return new Vector2(ScreenShotRect.x, ScreenShotRect.y);
        }
    }
}