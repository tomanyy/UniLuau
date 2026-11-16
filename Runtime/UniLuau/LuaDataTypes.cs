using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

                                                               
                                                                   
                               
                                                               

[MoonSharpUserData]
public class NumberRange
{
    public float Min, Max;

    [MoonSharpHidden]
    public NumberRange(float min, float max) { Min = min; Max = max; }

    public override string ToString() { return Min + ", " + Max; }
}

[MoonSharpUserData]
public class NumberSequenceKeypoint
{
    public float Time, Value, Envelope;

    [MoonSharpHidden]
    public NumberSequenceKeypoint(float time, float value, float envelope)
    {
        Time = Mathf.Clamp01(time);
        Value = value;
        Envelope = Mathf.Max(0f, envelope);
    }

    public override string ToString() { return Time + ", " + Value + ", " + Envelope; }
}

[MoonSharpUserData]
public class NumberSequence
{
    public readonly List<NumberSequenceKeypoint> Keypoints = new List<NumberSequenceKeypoint>();

    [MoonSharpHidden]
    public NumberSequence(float value)
    {
        Keypoints.Add(new NumberSequenceKeypoint(0f, value, 0f));
        Keypoints.Add(new NumberSequenceKeypoint(1f, value, 0f));
    }

    [MoonSharpHidden]
    public NumberSequence(float min, float max)
    {
        Keypoints.Add(new NumberSequenceKeypoint(0f, min, 0f));
        Keypoints.Add(new NumberSequenceKeypoint(1f, max, 0f));
    }

    [MoonSharpHidden]
    public NumberSequence(Table table)
    {
        foreach (DynValue v in table.Values)
        {
            var kp = v.UserData != null ? v.UserData.Object as NumberSequenceKeypoint : null;
            if (kp == null && v.Type == DataType.Table)
            {
                var t = v.Table;
                kp = new NumberSequenceKeypoint(
                    (float)(t.Get("Time").CastToNumber() ?? 0),
                    (float)(t.Get("Value").CastToNumber() ?? 0),
                    (float)(t.Get("Envelope").CastToNumber() ?? 0));
            }
            if (kp != null) Keypoints.Add(kp);
        }
        Keypoints.Sort((a, b) => a.Time.CompareTo(b.Time));
        if (Keypoints.Count == 0) Keypoints.Add(new NumberSequenceKeypoint(0f, 0f, 0f));
    }

    public override string ToString() { return Keypoints.Count + " keypoints"; }
}

[MoonSharpUserData]
public class ColorSequenceKeypoint
{
    public float Time;
    public Color3 Value;

    [MoonSharpHidden]
    public ColorSequenceKeypoint(float time, Color3 value)
    {
        Time = Mathf.Clamp01(time);
        Value = value ?? new Color3(1, 1, 1);
    }

    public override string ToString() { return Time + ", " + Value; }
}

[MoonSharpUserData]
public class ColorSequence
{
    public readonly List<ColorSequenceKeypoint> Keypoints = new List<ColorSequenceKeypoint>();

    [MoonSharpHidden]
    public ColorSequence(Color3 value)
    {
        Keypoints.Add(new ColorSequenceKeypoint(0f, value));
        Keypoints.Add(new ColorSequenceKeypoint(1f, value));
    }

    [MoonSharpHidden]
    public ColorSequence(Color3 min, Color3 max)
    {
        Keypoints.Add(new ColorSequenceKeypoint(0f, min));
        Keypoints.Add(new ColorSequenceKeypoint(1f, max));
    }

    [MoonSharpHidden]
    public ColorSequence(Table table)
    {
        foreach (DynValue v in table.Values)
        {
            var kp = v.UserData != null ? v.UserData.Object as ColorSequenceKeypoint : null;
            if (kp == null && v.Type == DataType.Table)
            {
                var t = v.Table;
                object c = t.Get("Value").ToObject();
                var color = c as Color3;
                if (color == null)
                {
                    var t2 = t.Get("Value").Table;
                    if (t2 != null)
                        color = new Color3(
                            (float)(t2.Get("R").CastToNumber() ?? 1),
                            (float)(t2.Get("G").CastToNumber() ?? 1),
                            (float)(t2.Get("B").CastToNumber() ?? 1));
                }
                kp = new ColorSequenceKeypoint(
                    (float)(t.Get("Time").CastToNumber() ?? 0),
                    color ?? new Color3(1, 1, 1));
            }
            if (kp != null) Keypoints.Add(kp);
        }
        Keypoints.Sort((a, b) => a.Time.CompareTo(b.Time));
        if (Keypoints.Count == 0) Keypoints.Add(new ColorSequenceKeypoint(0f, new Color3(1, 1, 1)));
    }

    public override string ToString() { return Keypoints.Count + " keypoints"; }
}
