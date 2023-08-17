using UnityEngine;

namespace MeshGen {

public static class MeshGens {

    struct PointData {
        public Vector3[] p;
        public Vector2[] u;
    }

    // triangle=3
    // square=4
    // circle=16
    public static Mesh Circle(Vector2 size, int v, float offset = 0) {
        return GenMesh(CirclePoints(size, v, offset), GenTri(v));
    }
    public static Mesh Circle(float size, int v, float offset = 0) {
        return GenMesh(CirclePoints(size, v, offset), GenTri(v));
    }

    public static Mesh Semicircle(float size, int v = 8, float offset = 0) {
        PointData d = SemiPoints(size, v, offset + 3 * Mathf.PI / 2);
        for (int i = 0; i < v; i++) {
            d.p[i] -= new Vector3(0, size / 2);
            d.u[i] = d.p[i];
        }
        return GenMesh(d, GenTri(v));
    }

    static PointData CirclePoints(float size, int v, float offset = 0) {
        return CirclePoints(new Vector2(size, size), v, offset);
    }

    static Mesh Arrow(Vector2 size) {
        Vector3[] points = new Vector3[4 + 3];
        Vector2[] uvs = new Vector2[4 + 3];
        // so what? no head
        points[0] = new Vector3(0, size.y / 2);
        points[1] = new Vector3(-size.x / 2, size.y * 1 / 4);
        points[2] = new Vector3(size.x / 2, size.y * 1 / 4);

        points[3] = new Vector3(-size.x / 4, -size.y * 1 / 4);
        points[4] = new Vector3(-size.x / 4, size.y * 1 / 4);
        points[5] = new Vector3(size.x / 4, size.y * 1 / 4);
        points[6] = new Vector3(size.x / 4, -size.y * 1 / 4);

        for (int i = 0; i < 7; i++) {
            uvs[i] = points[i];
        }

        int[] tris = new int[3 * (1 + 2)];
        tris[0] = 0;
        tris[1] = 2;
        tris[2] = 1;

        tris[3] = 3;
        tris[4] = 4;
        tris[5] = 5;
        tris[6] = 5;
        tris[7] = 6;
        tris[8] = 3;

        return GenMesh(points, uvs, tris);
    }

    static PointData CirclePoints(Vector2 size, int v, float offset = 0) {
        Vector3[] points = new Vector3[v];
        Vector2[] uv = new Vector2[v];
        float r;
        for (int i = 0; i < v; i++) {
            r = offset + 2 * Mathf.PI * (float)(i) / v;
            points[i] =
                new Vector3((Mathf.Sin(r)) * size.x, (Mathf.Cos(r)) * size.y);
            uv[i] = points[i];
        }
        return new PointData { p = points, u = uv };
    }

    static Mesh PuddlePoints(int v) {
        Vector3[] points = new Vector3[v];
        Vector2[] uv = new Vector2[v];
        float t = 0;
        float r = 0;
        float radius;
        Vector2 seed =
            new Vector3(Random.Range(-7f, 7f), Random.Range(-9f, 9f));
        for (int i = 0; i < v; i++) {
            radius = Mathf.LerpUnclamped(
                0.25f, 0.5f,
                Mathf.PerlinNoise(r * 0.7f + seed.x, r * 0.9f + seed.y));
            Vector2 v2 =
                new Vector3(radius * Mathf.Sin(r), radius * Mathf.Cos(r));
            points[i] = v2;
            uv[i] = v2;
            r += 2 * Mathf.PI / v;
        }
        int[] tris = GenTri(v);
        return GenMesh(points, uv, tris);
    }

    static Mesh GenMesh(PointData d, int[] tris) {
        Mesh m = new Mesh();
        m.vertices = d.p;
        m.uv = d.u;
        m.triangles = tris;
        return m;
    }
    static Mesh GenMesh(Vector3[] point, Vector2[] uv, int[] tris) {
        Mesh m = new Mesh();
        m.vertices = point;
        m.uv = uv;
        m.triangles = tris;
        return m;
    }

    static PointData SemiPoints(float size, int v, float offset = 0) {
        Vector3[] points = new Vector3[v];
        Vector2[] uv = new Vector2[v];
        float r;
        for (int i = 0; i < v; i++) {
            r = offset + Mathf.PI * (float)(i) / (v - 1);
            points[i] = size * new Vector3(Mathf.Sin(r), Mathf.Cos(r));
            uv[i] = points[i];
        }
        return new PointData { p = points, u = uv };
    }

    public static int[] GenTri(int v) {
        int[] tris = new int[(v - 2) * 3];
        for (int i = 0; i < v - 2; i++) {
            int j = i * 3;
            tris[j] = 0;
            tris[j + 1] = i + 1;
            tris[j + 2] = i + 2;
        }
        return tris;
    }

    public static Mesh CircleSpikedOutline(float size, float spikeSize,
                                           int v = 6, int spikes = 1,
                                           float offset = 0,
                                           float thickness = 0.8f) {
        Vector3[] points = new Vector3[v * (2 + spikes * 7)];
        Vector2[] uvs = new Vector2[v * (2 + spikes * 7)];
        int[] tris = new int[v * (2 + spikes * 3) * 3];
        float r;
        for (int i = 0; i < v; i++) {
            r = offset + 2 * Mathf.PI * (float)(i) / v;
            points[i + v] =
                new Vector3((Mathf.Sin(r)) * size, (Mathf.Cos(r)) * size);
            points[i] = points[i + v] * thickness;
            uvs[i] = points[i];
            uvs[i + v] = points[i + v];
            int j = 6 * i;
            tris[j + 2] = i;
            tris[j + 1] = (i + 1) % v;
            tris[j] = (i + 1) % v + v;
            tris[j + 5] = i;
            tris[j + 4] = tris[j];
            tris[j + 3] = i + v;
        }
        for (int i = 0; i < v; i++) {
            for (int j = 0; j < spikes; j++) {
                // a lerp between the two multiplied by the value inbetween the
                // thickness and 1
                Vector3 spikeCenter =
                    Vector3.Lerp(points[i + v], points[(i + 1) % v + v],
                                 (j + 1f) / (spikes + 1)) *
                    1.2f;
                int p = 2 * v + spikes * i * 7 + j * 7;
                int m = 6 * v + spikes * i * 9 + j * 9;
                for (int k = 0; k < 3; k++) {
                    r = 2 * Mathf.PI * k / 3 + Mathf.PI * (2 * i + 1) / v;
                    points[p] = new Vector3((Mathf.Sin(r)) * spikeSize,
                                            (Mathf.Cos(r)) * spikeSize) +
                                spikeCenter;
                    uvs[p] = points[p];
                    tris[m] = p;
                    p++;
                    m++;
                }
                points[p] =
                    Vector3.Lerp(points[i + v], points[(i + 1) % v + v],
                                 (j + 1f) / (spikes + 1) + spikeSize / 3) *
                    1.2f;
                points[p + 1] =
                    Vector3.Lerp(points[i + v], points[(i + 1) % v + v],
                                 (j + 1f) / (spikes + 1) - spikeSize / 3) *
                    1.2f;
                points[p + 2] = points[p] - spikeCenter;
                points[p + 3] = -points[p + 2];
                for (int k = 0; k < 4; k++) {
                    uvs[p + k] = points[p + k];
                }
                tris[m] = p + 3;
                tris[m + 1] = p + 1;
                tris[m + 2] = p;
                tris[m + 3] = p + 3;
                tris[m + 4] = p;
                tris[m + 5] = p + 2;
            }
        }
        return GenMesh(points, uvs, tris);
    }

    public static Mesh CircleOutline(float size, int v = 6, float offset = 0,
                                     float thickness = 0.8f) {
        Vector3[] points = new Vector3[v * 2];
        Vector2[] uvs = new Vector2[v * 2];
        int[] tris = new int[v * 6];
        PointData outer = CirclePoints(size, v, offset);
        PointData inner = CirclePoints(size * thickness, v, offset);
        for (int i = 0; i < v; i++) {
            points[i] = inner.p[i];
            uvs[i] = inner.u[i];
        }
        for (int i = 0; i < v; i++) {
            points[i + v] = outer.p[i];
            uvs[i + v] = outer.u[i];
        }
        for (int i = 0; i < v; i++) {
            int j = 6 * i;
            tris[j + 2] = i;
            tris[j + 1] = (i + 1) % v;
            tris[j] = (i + 1) % v + v;
            tris[j + 5] = i;
            tris[j + 4] = tris[j];
            tris[j + 3] = i + v;
        }
        return GenMesh(points, uvs, tris);
    }

    public static Mesh Star(float size, int v = 5, float offset = 0) {
        PointData dinner = CirclePoints(size / 3, v, offset - (Mathf.PI / v));
        PointData douter = CirclePoints(size, v, offset);
        Vector3[] points = new Vector3[v * 2];
        Vector2[] uv = new Vector2[v * 2];
        int[] tris = new int[6 * (v - 1)];
        for (int i = 0; i < v; i++) {
            int j = i * 3;
            tris[j] = v + i;
            tris[j + 1] = i;
            tris[j + 2] = v + (i + 1) % v;
            points[i] = douter.p[i];
            points[v + i] = dinner.p[i];
            uv[i] = douter.u[i];
            uv[v + i] = dinner.u[i];
        }
        for (int i = 0; i < v - 2; i++) {
            int j = (i + v) * 3;
            tris[j] = v;
            tris[j + 1] = v + 1 + i;
            tris[j + 2] = v + 2 + i;
        }
        return GenMesh(points, uv, tris);
    }

    public static Mesh Quaterfoil(float size, int v) {
        int vFix = v - 1;
        Vector3[] points = new Vector3[vFix * 4];
        Vector2[] uv = new Vector2[vFix * 4];
        int[] tris = new int[12 * vFix - 6];
        int k = 0;
        for (int i = 0; i < 4; i++) {
            float r = i * Mathf.PI / 2;
            Vector3 pointOffset =
                new Vector2(size * Mathf.Cos(r) / 2, -size * Mathf.Sin(r) / 2);
            PointData pd = SemiPoints(size / 2, v, r);
            for (int j = 0; j < v - 1; j++) {
                points[vFix * i + j] = pd.p[j] + pointOffset;
                uv[(vFix)*i + j] = pd.p[j] + pointOffset;
            }
            for (int j = 0; j < v - 2; j++) {
                int s = i * 3 * (vFix - 1); // this is the base index for j=0
                k = s + j * 3;              // this is the base index for j=j
                tris[k] = i * (vFix);
                tris[k + 1] = i * vFix + j + 1;
                tris[k + 2] = (i * vFix + j + 2) % (vFix * 4);
            }
        }
        int tl = 12 * vFix - 6;
        k += 3;
        for (int i = 0; i < 2; i++) {
            tris[k + (i * 3)] = 0;
            tris[k + (i * 3) + 1] = (i + 1) * (vFix);
            tris[k + (i * 3) + 2] = (i + 2) * (vFix);
        }
        return GenMesh(points, uv, tris);
    }

    public static Mesh Curvilinear(float size, int v, float extension) {
        int n = v * 3;
        int k = 0;
        if (extension < 0) {
            n++;
            k = 1;
        }
        Vector3[] points = new Vector3[n];
        Vector2[] uv = new Vector2[n];
        if (extension < 0) {
            points[0] = new Vector3(0, 0);
            uv[0] = new Vector3(0, 0);
        }
        for (int i = 0; i < 3; i++) {
            float angle = Mathf.PI * i * 2 / (float)3;
            float nextAngle = Mathf.PI * (i + 1) * 2 / (float)3;
            float extensionAngle = angle + Mathf.PI / 3;
            Vector3[] square = new Vector3[4];
            square[2] = extension * new Vector3(Mathf.Sin(extensionAngle),
                                                Mathf.Cos(extensionAngle));
            square[0] = size * new Vector3(Mathf.Sin(angle), Mathf.Cos(angle));
            square[1] = square[0] + square[2];
            square[3] =
                size * new Vector3(Mathf.Sin(nextAngle), Mathf.Cos(nextAngle));
            square[2] = square[2] + square[3];
            // bezier curve
            for (int j = 0; j < v; j++) {
                int idx = i * v + j + k;
                float t = j / (float)v;
                float tx = 1 - t;
                Vector3 a, b, c;
                a = square[0] * (tx) + square[1] * (t);
                b = square[1] * (tx) + square[2] * (t);
                c = square[2] * (tx) + square[3] * (t);
                a = a * (tx) + b * (t);
                b = b * (tx) + c * (t);
                points[idx] = a * (tx) + b * (t);
                uv[idx] = points[idx];
            }
        }
        int[] tris = GenTri(n);
        return GenMesh(points, uv, tris);
    }

    static string[] shapeNames =
        new string[] { "star",        "cross",
                       "circle",      "semicircle",
                       "quaterfoil",  "square",
                       "rectangle",   "octogon",
                       "triangle",    "diamond",
                       "curvilinear", "hexagonOuter",
                       "hexagon",     "spikedHexagonOuter",
                       "puddle",      "arrow" };
    static Mesh[] meshes = new Mesh[16];
    static Material[] colors = new Material[26];

    [RuntimeInitializeOnLoadMethod]
    static void StructureGen() {
        MeshGen();
        ColourGen();
    }

    public static GameObject MinObjGen(Shapes shape, MatColour m,
                                       int offset = 0) {
        return MinObjGen(shape,colors[(int)m * 2 + offset]);
    }

    public static GameObject MinObjGen(Shapes shape, Material m) {
        GameObject obj = new GameObject(shapeNames[(int)shape]);
        obj.AddComponent<MeshFilter>().mesh = meshes[(int)shape];
        obj.AddComponent<MeshRenderer>().material = m;
        return obj;
    }

    static void MeshGen() {
        meshes[(int)Shapes.star] = Star(0.5f);
        meshes[(int)Shapes.star].name = "star";
        meshes[(int)Shapes.cross] = Star(0.5f, 4);
        meshes[(int)Shapes.cross].name = "cross";
        meshes[(int)Shapes.circle] = Circle(0.5f, 16);
        meshes[(int)Shapes.circle].name = "circle";
        meshes[(int)Shapes.triangle] = Circle(1f, 3);
        meshes[(int)Shapes.triangle].name = "triangle";
        meshes[(int)Shapes.square] = Circle(0.5f, 4, Mathf.PI / 4);
        meshes[(int)Shapes.square].name = "square";
        meshes[(int)Shapes.semicircle] = Semicircle(0.5f);
        meshes[(int)Shapes.semicircle].name = "semicircle";
        meshes[(int)Shapes.quaterfoil] = Quaterfoil(0.5f, 9);
        meshes[(int)Shapes.quaterfoil].name = "quaterfoil";
        meshes[(int)Shapes.diamond] = Circle(new Vector2(0.35f, 0.5f), 4);
        meshes[(int)Shapes.diamond].name = "diamond";
        meshes[(int)Shapes.rectangle] =
            Circle(new Vector2(0.35f, 0.5f), 4, Mathf.PI / 4);
        meshes[(int)Shapes.rectangle].name = "rectangle";
        meshes[(int)Shapes.curvilinear] = Curvilinear(0.5f, 6, 0.2f);
        meshes[(int)Shapes.curvilinear].name = "curvilinear";
        meshes[(int)Shapes.octogon] = Circle(1, 8, Mathf.PI / 8);
        meshes[(int)Shapes.octogon].name = "octogon";
        meshes[(int)Shapes.hexagonOuter] =
            CircleOutline(Mathf.Sqrt(3) / 3, 6, 0, 0.92f);
        meshes[(int)Shapes.hexagonOuter].name = "hexagonOuter";
        meshes[(int)Shapes.hexagon] = Circle(Mathf.Sqrt(3) / 3, 6);
        meshes[(int)Shapes.hexagon].name = "hexagon";
        meshes[(int)Shapes.spikedHexagonOuter] =
            CircleSpikedOutline(Mathf.Sqrt(3) / 3, 0.1f, 6, 1, 0, 0.92f);
        meshes[(int)Shapes.spikedHexagonOuter].name = "spikedHexagonOuter";
        meshes[(int)Shapes.puddle] = PuddlePoints(20);
        meshes[(int)Shapes.puddle].name = "puddle";
        meshes[(int)Shapes.arrow] = Arrow(new Vector2(.3f, 1));
        meshes[(int)Shapes.arrow].name = "arrow";
    }

    static void ColourGen() {
        ColourMats(MatColour.white, Color.white);
        ColourMats(MatColour.red, Color.red);
        ColourMats(MatColour.green, Color.green);
        ColourMats(MatColour.blue, Color.blue);
        ColourMats(MatColour.black, Color.black);
        ColourMats(MatColour.foreground, ColorFromHex(0xa9b1d680));
        ColourMats(MatColour.background, ColorFromHex(0x1a1f2aff));
        ColourMats(MatColour.blue2, ColorFromHex(0x0715cdff));
        ColourMats(MatColour.exRed, ColorFromHex(0x0715cdff));
        ColourMats(MatColour.exYellow, ColorFromHex(0x0715cdff));
        ColourMats(MatColour.rebeccaPurple, ColorFromHex(0xdf73ffff));
        ColourMats(MatColour.rebeccaOrange, ColorFromHex(0xe79e00ff));
        ColourMats(MatColour.rebeccaOrangeAnti, ColorFromHex(0x00D73300ff));
    }

    public static Color ColorFromHex(uint hex) {
        float[] rgba = new float[4];
        for (int i = 0; i < 4; i++) {
            rgba[i] = ((hex >> (8 * (3 - i))) & 0xff) / (255f);
        }
        return new Color(rgba[0], rgba[1], rgba[2], rgba[3]);
    }

    const float blackness = 1 / 8f;
    static void ColourMats(MatColour color, Color c) {
        colors[(int)color * 2] = ColorToMat(c);
        colors[(int)color * 2 + 1]= ColorToMat(
            new Color(blackness * c.r, blackness * c.g, blackness * c.b));
    }

    public static Material ColorToMat(Color c){
        Shader s;
        if (c.a != 1) {
            s = Shader.Find("Standard");
        } else {
            s = Shader.Find("Unlit/Color");
        }
        Material m = new Material(s);
        m.color=c;
        return m;
    }
}

public enum Shapes {
    star,
    cross,
    circle,
    semicircle,
    quaterfoil,
    square,
    rectangle,
    octogon,
    triangle,
    diamond,
    curvilinear,
    hexagonOuter,
    hexagon,
    spikedHexagonOuter,
    puddle,
    arrow
}
;

public enum MatColour {
    white,
    red,
    blue,
    green,
    black,
    foreground,
    background,
    blue2,
    exRed,
    exYellow,
    rebeccaPurple,
    rebeccaOrange,
    rebeccaOrangeAnti
}
}
