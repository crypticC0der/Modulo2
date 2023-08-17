using UnityEngine;
using System.Collections.Generic;

namespace Modulo{
	public static class Crafting{

		static Vector2 Normalize(int refined, int sharpened, int neutral){
			float sum = refined+sharpened+neutral;
			float pinkr = sharpened/sum;
			float yellowr = refined/sum;
			float min = Mathf.Min(pinkr,yellowr);
			pinkr += min;
			yellowr += min;
			return new Vector2(pinkr,yellowr);
		}

		static ItemRatio closestRatio(Vector2 r){
			float distance=float.PositiveInfinity;
			ItemRatio best = null;
			foreach(ItemRatio ir in ItemRatio.table){
				float newdist = (ir.ratio - r).sqrMagnitude;
				if(newdist < distance){
					best = ir;
					distance = newdist;
				}
			}
			return best;
		}

		public static Item Obtain(int refined, int sharpened, int neutral) {
			Vector2 ratio = Normalize(refined,sharpened,neutral);
			ItemRatio cr = closestRatio(ratio);
			float power = refined+sharpened+neutral;
			float stability = (ratio-cr.ratio).magnitude;
			Debug.Log("created " + cr.item.name + " with (power,stability): " +
					  (new Vector2(power,stability)).ToString());
			return cr.item.FromTemplate(power,stability);
		}
	}
}
