
using UnityEngine;
using System.Collections.Generic;

namespace Modulo {
public static class MouseChange{

	public static Dictionary<string,Vector2> handPoints = new Dictionary<string,Vector2>(){
		{"arrow",new Vector2(16,20)},
		{"fist",new Vector2(48,48)},
		{"hand",new Vector2(32,20)},
		{"no",new Vector2(48,48)}
	};

	public static Color outline=Color.blue;
	public static Color inline=Color.blue;
	public static string name=null;
	public static int scale=-1;

	const int size=96;
	static bool mouseChanged=false;
	static bool mouseUpdated=false;

	static Color defaultOutline=Color.white;
	static Color defaultInline=Color.black;
	static string defaultName="arrow";
	static int defaultScale=4;


	public static void SetDefaults(){
		mouseUpdated |= outline!=defaultOutline
			|| inline!=defaultInline
			|| scale!=defaultScale
			|| name!=defaultName;

		outline=defaultOutline;
		inline=defaultInline;
		scale=defaultScale;
		name=defaultName;
	}


	public static void Update(){
		if(!mouseChanged){
			SetDefaults();
		}
		if(mouseUpdated){
			UpdateMouse();
		}
		mouseChanged=false;
		mouseUpdated=false;
	}

	public static void ChangeOutline(Color outline){
		mouseChanged=true;
		mouseUpdated |= MouseChange.outline!=outline;
		MouseChange.outline=outline;
	}


	public static void ChangeInline(Color inline){
		mouseChanged=true;
		mouseUpdated |= MouseChange.inline!=inline;
		MouseChange.inline=inline;
	}

	public static void ChangeColor(Color outline,Color inline){
		ChangeOutline(outline);
		ChangeInline(inline);
	}

	public static void ChangeScale(int scale){
		mouseChanged=true;
		mouseUpdated |= scale != MouseChange.scale;
		MouseChange.scale = scale;
	}

	public static void ChangeName(string name){
		mouseChanged=true;
		mouseUpdated |= name != MouseChange.name;
		MouseChange.name = name;
	}

	public static void UpdateMouse(){
		int newsize = size/scale;

		Texture2D t = Resources.Load<Texture2D>("curseor/" + name);
		Texture2D newt = new Texture2D(newsize,newsize);
		Color32[] ins = t.GetPixels32();
		Color32[] outs= new Color32[newsize*newsize];
		for(int i =0;i<outs.Length;i++){
			Color32 c = new Color32(0,0,0,0);
			int j = scale*(i % newsize);
			int k = scale*(i / newsize);

			for(int m=0;m<scale;m++){
				for(int n=0;n<scale;n++){
					Color32 p = ins[(k+m)*size + (j+n)];
					int scale2 = scale*scale;
					for(int idx=0;idx<4;idx++){
						c[idx] += (byte)(p[idx]/scale2);
					}
				}
			}
			float g = (c.r + c.g + c.b)/(3f*255f);
			outs[i] = Color32.Lerp(inline,outline,g);
			outs[i].a = c.a;
		}
		newt.SetPixels32(outs);
		newt.Apply();
		Cursor.SetCursor(newt,handPoints[name]/size,CursorMode.Auto);
	}
}
}
