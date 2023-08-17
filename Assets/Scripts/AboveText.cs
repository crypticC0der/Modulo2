using UnityEngine;
using System.Collections.Generic;
using TMPro;

namespace Modulo{
	public class AboveText{

		static TMP_FontAsset _baseFont=null;
		static private TMP_FontAsset baseFont{
			get{
				if(_baseFont!=null){
					return _baseFont;
				}else{
					_baseFont= TMP_FontAsset.CreateFontAsset(
						Resources.Load<Font>("FFFFORWA"));
					return _baseFont;
				}
			}
			set{}
		}


		TextMeshPro text;
		public AboveText(Transform above){
			GameObject me = new GameObject("text");
			text = me.AddComponent<TextMeshPro>();
			me.transform.SetParent(above);
			me.transform.localPosition= new Vector3(0,.72f,0);
			// text.text="123";
			text.alignment=TextAlignmentOptions.Center;
			text.font=baseFont;
			text.fontSize=28;
			me.transform.localScale=new Vector3(.1f,.1f,0);
		}

		public void SetText(string s) => text.text=s;
		public void SetText(object s) => text.text=s.ToString();

		public void SetColour(Color c) => text.color=c;

		public void Show() => text.enabled=true;
		public void Hide() => text.enabled=false;
	}
}
