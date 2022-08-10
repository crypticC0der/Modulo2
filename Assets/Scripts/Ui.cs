using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ui{
	public class Frame{
		Vector2 size;
		Vector2 position;

		public void Render(object o, string title,Vector2 pos){}
		public void Render(Image o, string title,Vector2 pos){}
		public void Render(Frameable f, string title,Vector2 pos){}

		public Vector2 Position(Vector2 v){
			Vector2 p = position;
			p.x+=size.x*v.x;
			p.y+=size.y*v.y;
			return p;
		}
	}

	public interface Frameable{
		public void Draw(Frame f);
	}
}
