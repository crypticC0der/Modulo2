using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Modulo {
public class HexCoord {
    public int q, r;
    public static Vector2 Q =
        new Vector2(Mathf.Sin(Mathf.PI / 2), Mathf.Cos(Mathf.PI / 2));
    public static Vector2 R = new Vector2(-Mathf.Sin(Mathf.PI * 5f / 6),
                                          -Mathf.Cos(Mathf.PI * 5f / 6));
    const int mask = (~(World.optimizationCubeSize - 1));
    public HexCoord(int q, int r) {
        this.q = q;
        this.r = r;
    }

    public Vector3 position() { return q * Q + r * R; }

    public bool IsRectCenter() {
        return (q & (~mask)) == 0 && (r & (~mask)) == 0;
    }

    public HexCoord RectCenter() { return new HexCoord(q & mask, r & mask); }

    public override bool Equals(object obj) { return Equals(obj as HexCoord); }

    public bool Equals(HexCoord hc) { return hc.q == q && hc.r == r; }

    public override int GetHashCode() {
        return (new Vector2(q, r)).GetHashCode();
    }

    public static HexCoord NearestHex(Vector3 v) {
        float q = (v.x + 1 / Mathf.Sqrt(3) * v.y);
        float r = -(2f / 3 * v.y) * Mathf.Sqrt(3);
        float s = -q - r;

        float rq = Mathf.Round(q);
        float rr = Mathf.Round(r);
        float rs = Mathf.Round(s);

        q = Mathf.Abs(rq - q);
        r = Mathf.Abs(rr - r);
        s = Mathf.Abs(rs - s);

        if (q > r && q > s) {
            rq = -rr - rs;
        } else if (r > s) {
            rr = -rq - rs;
        } else {
            rs = -rq - rr;
        }

        return new HexCoord((int)rq, -(int)rr);
    }

    public static HexCoord operator +(HexCoord a) => a;
    public static HexCoord
    operator +(HexCoord a, HexCoord b) => new HexCoord(a.q + b.q, a.r + b.r);
    public static HexCoord operator -(HexCoord a) => new HexCoord(-a.q, -a.r);
    public static HexCoord operator -(HexCoord a, HexCoord b) => a + (-b);
    public static HexCoord
    operator *(int f, HexCoord a) => new HexCoord(f * a.q, f *a.r);
    public static HexCoord operator *(HexCoord a,
                                      int f) => new HexCoord(f * a.q, f *a.r);
    public static bool operator ==(HexCoord a, HexCoord b) => a.Equals(b);
    public static bool operator !=(HexCoord a, HexCoord b) => !a.Equals(b);

    public static float Distance(HexCoord a, HexCoord b) {
        return (Mathf.Abs(a.q - b.q) + Mathf.Abs(a.q + a.r - b.q - b.r) +
                Mathf.Abs(a.r - b.r)) /
               2;
    }

    // public List<HexCoord> Range(int range){
    //     List<HexCoord> ret = new List<HexCoord>();
    //     ForEachInRange(range,(HexCoord hc) => ret.Add(hc));
    //     return ret;
    // }

    public IEnumerable<HexCoord> InRange(int range) {
        for (int q = -range; q <= range; q++) {
            for (int r = Mathf.Max(-range, -q - range);
                 r <= Mathf.Min(range, -q + range); r++) {
                yield return (this + new HexCoord(q, -r));
            }
        }
    }

    public override string ToString() {
        return "(" + q.ToString() + ", " + r.ToString() + ")";
    }
}
}
