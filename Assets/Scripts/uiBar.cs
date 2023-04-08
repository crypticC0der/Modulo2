using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Modulo {
public interface Bar {
    public void Reset();
    public void Zero();
    public void SetValue(float current, float max);
    public void UpdateValue(float current, float max);
    public void Enable(bool b);
    public void Delete();
}

public class uiBar : MonoBehaviour, Bar {
    public TextMeshPro text;
    public Color[] colors;
    public float[] intervals;
    float startx;
    public void Reset() {
        transition = 0;
        current = 1;
        aim = 1;
        SetValue(1, 1);
    }

    public void Zero() { UpdateValue(0, 1); }
    SpriteRenderer renderer;
    float current;
    float aim;
    float transition;

    public void Start() {
        renderer = GetComponent<SpriteRenderer>();
        startx=transform.localPosition.x;
        Reset();
    }

    public void UpdateValue(float current, float max) {
        this.aim = current / max;
        transition = 3;
    }

    public void Enable(bool b) { renderer.enabled = false; }

    public void Delete() {
        Destroy(renderer.gameObject);
        Destroy(this);
    }

    public Color Remap(float v) {
        float t = Mathf.InverseLerp(intervals[0], intervals[1], v);
        return Color.LerpUnclamped(colors[0], colors[1], t);
    }

    public void Update() {
        if (transition > Time.deltaTime) {
            current += (aim - current) / transition;
            transition -= Time.deltaTime;
            SetValue(current, 1);
        } else if (transition != 0) {
            transition = 0;
            current = aim;
            SetValue(aim, 1);
        }
    }

    public void SetValue(float current, float max) {
        Vector3 s = transform.localScale;
        float v = Mathf.Clamp(current / max, 0,1);
        s.x = v*100;
        renderer.color = Remap(v);
        Vector3 p = transform.localPosition;
        p.x = 50* (1 - v) + startx;
        transform.localScale = s;
        transform.localPosition = p;

        text.text= ((int)Mathf.Floor(v*100)).ToString() + "%";

    }
}
}
