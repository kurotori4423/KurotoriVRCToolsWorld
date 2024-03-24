using System.Collections;
using System.Text;
using UnityEditor;
using UnityEngine;


/// <summary>
/// https://esprog.hatenablog.com/entry/2016/09/30/165057
/// 
/// </summary>
public class UVChecker : EditorWindow
{
	private GameObject targetGameObject;
	private MeshFilter targetMeshFilter;
	private Texture2D tex;

    enum UVNUM
    {
        UV,
        UV2,
        UV3,
    };

    private UVNUM displayUV = UVNUM.UV;

    private int textureSize = 512;


	[MenuItem("KurotoriTools/UVChecker")]
	private static void Open()
	{
		GetWindow<UVChecker>();
	}

	private void OnGUI()
	{
		targetGameObject = EditorGUILayout.ObjectField("TargetMesh", targetGameObject, typeof(GameObject), true) as GameObject;

        displayUV = (UVNUM)EditorGUILayout.EnumPopup("UVチャンネル", displayUV, GUILayout.Width(256));

        textureSize = EditorGUILayout.IntField("出力サイズ", textureSize);

		if(GUILayout.Button("Execute"))
		{
			targetMeshFilter = targetGameObject.GetComponent<MeshFilter>();
			tex = new Texture2D(textureSize, textureSize);
			var mesh = targetMeshFilter.sharedMesh;
			DrawUV(mesh);
		}
		if(tex != null)
		{
			EditorGUI.DrawPreviewTexture(new Rect(10, 80, tex.width, tex.height), tex);
		}
	}

	private void DrawUV(Mesh mesh)
	{
        Vector2[] uvs = new Vector2[0];

        switch(displayUV)
        {
            case UVNUM.UV:
                if(mesh.uv.Length > 0)
                {
                    uvs = mesh.uv;
                }
                else
                {
                    return;
                }
                break;
            case UVNUM.UV2:
                if (mesh.uv2.Length > 0)
                {
                    uvs = mesh.uv2;
                }
                else
                {
                    return;
                }
                break;
            case UVNUM.UV3:
                if (mesh.uv3.Length > 0)
                {
                    uvs = mesh.uv3;
                }
                else
                {
                    return;
                }
                break;
        }
		var tri = mesh.triangles;

		for(int i_base = 0; i_base < tri.Length; i_base += 3)
		{
			int i_1 = i_base;
			int i_2 = i_base + 1;
			int i_3 = i_base + 2;

			Vector2 uv1 = uvs[tri[i_1]];
			Vector2 uv2 = uvs[tri[i_2]];
			Vector2 uv3 = uvs[tri[i_3]];

			DrawLine(uv1, uv2);
			DrawLine(uv2, uv3);
			DrawLine(uv3, uv1);
		}

		tex.Apply(false);

		UVLog(uvs);
	}

	private void UVLog(Vector2[] uvs)
	{
		StringBuilder sb = new StringBuilder();
		foreach(var uv in uvs)
		{
			sb.AppendLine(uv.ToString());
		}

		Debug.Log(sb.ToString());
	}

	private void DrawLine(Vector2 from, Vector2 to)
	{
		int x0 = Mathf.RoundToInt(from.x * tex.width);
		int y0 = Mathf.RoundToInt(from.y * tex.height);
		int x1 = Mathf.RoundToInt(to.x * tex.width);
		int y1 = Mathf.RoundToInt(to.y * tex.height);

		DrawLine(x0, y0, x1, y1, Color.red);
	}

	private void DrawLine(int x0, int y0, int x1, int y1, Color col)
	{
		int dy = y1 - y0;
		int dx = x1 - x0;
		int stepx, stepy;

		if(dy < 0)
		{ dy = -dy; stepy = -1; }
		else
		{ stepy = 1; }
		if(dx < 0)
		{ dx = -dx; stepx = -1; }
		else
		{ stepx = 1; }
		dy <<= 1;
		dx <<= 1;

		float fraction = 0;

		tex.SetPixel(x0, y0, col);
		if(dx > dy)
		{
			fraction = dy - (dx >> 1);
			while(Mathf.Abs(x0 - x1) > 1)
			{
				if(fraction >= 0)
				{
					y0 += stepy;
					fraction -= dx;
				}
				x0 += stepx;
				fraction += dy;
				tex.SetPixel(x0, y0, col);
			}
		}
		else
		{
			fraction = dx - (dy >> 1);
			while(Mathf.Abs(y0 - y1) > 1)
			{
				if(fraction >= 0)
				{
					x0 += stepx;
					fraction -= dy;
				}
				y0 += stepy;
				fraction += dx;
				tex.SetPixel(x0, y0, col);
			}
		}
	}
}