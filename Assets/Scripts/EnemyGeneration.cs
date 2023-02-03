using System.Collections.Generic;
using UnityEngine;
using MeshGen;
using static MeshGen.MeshGens;

namespace Modulo {
public class EnemySpawner {
    float spacing = 0;
    float quantity = 1;
    int level = 0;
    List<Attack> attacks = new List<Attack>();
    List<Proc> procs = new List<Proc>();
    List<Debuff> debuffs = new List<Debuff>();
    Shapes enemy = Shapes.circle;
    float timer = 0;
    Vector3 spawn = new Vector3(0, 0, 0);
    MatColour matColour = MatColour.white;

    public EnemySpawner() {}

    public bool Run() {
        if (timer < 0) {
            timer = spacing;
            quantity--;
            Spawn(spawn, level);
        }
        timer -= Time.deltaTime;
        return quantity > 0;
    }

    public EnemyFsm Spawn(Vector3 location, int level, Shapes s) {
        EnemyFsm q = EnemyGeneration.ObjGen(enemy, matColour);
        q.toApply = debuffs;
        q.procs = procs;
        foreach (Attack a in attacks) {
            a.AddAttack(q);
        }
        q.perent = this;
        q.LevelUp(level);
        return q;
    }

    public EnemyFsm Spawn(Vector3 location, int level) {
        return Spawn(location, level, enemy);
    }
}

public static class EnemyGeneration {

    public static EnemyFsm ObjGen(Shapes shape, MatColour m) {
        GameObject obj = MinObjGen(shape, m);
        // genchild
        GameObject child = MinObjGen(shape, m, 1);
        child.transform.localScale = new Vector3(0.8f, 0.8f, 0);
        child.transform.position += new Vector3(0, 0, -0.001f);
        child.transform.SetParent(obj.transform);

        obj.transform.position += new Vector3(0, 0, -0.1f);
        obj.layer = 6;
        obj.AddComponent<CircleCollider2D>();
        Rigidbody2D r = obj.AddComponent<Rigidbody2D>();
        r.constraints = RigidbodyConstraints2D.FreezeRotation;
        r.gravityScale = 0;
        r.drag = 2;
        obj.AddComponent<followAi>().force = 10;
        obj.transform.localScale *= 0.5f;

        EnemyFsm e;
        switch (shape) {
        case Shapes.star:
            e = obj.AddComponent<StarEnemy>();
            child.transform.localScale = new Vector3(0.6f, 0.6f, 0);
            break;
        case Shapes.cross:
            e = obj.AddComponent<CrossEnemy>();
            child.transform.localScale = new Vector3(0.6f, 0.6f, 0);
            break;
        case Shapes.circle:
            e = obj.AddComponent<CircleEnemy>();
            break;
        case Shapes.semicircle:
            e = obj.AddComponent<SemicircleEnemy>();
            break;
        case Shapes.quaterfoil:
            e = obj.AddComponent<QuaterfoilEnemy>();
            break;
        case Shapes.square:
            e = obj.AddComponent<SquareEnemy>();
            break;
        case Shapes.rectangle:
            e = obj.AddComponent<RectangleEnemy>();
            break;
        case Shapes.octogon:
            e = obj.AddComponent<OctogonEnemy>();
            break;
        case Shapes.triangle:
            e = obj.AddComponent<TriangleEnemy>();
            break;
        case Shapes.diamond:
            e = obj.AddComponent<DiamondEnemy>();
            break;
        case Shapes.curvilinear:
            e = obj.AddComponent<CurvilinearEnemy>();
            break;
        default:
            e = obj.AddComponent<EnemyFsm>();
            break;
        }
        return e;
    }

    public static List<EnemySpawner> spawners;
    public static void Run() {
        int i = 0;
        while (i < spawners.Count) {
            if (!spawners[i].Run()) {
                spawners.RemoveAt(i);
            } else {
                i++;
            }
        }
    }
}
}
