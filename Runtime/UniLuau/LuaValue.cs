using System;
using System.Globalization;
using UnityEngine;

public enum LuaValueType { Number, Boolean, String, Vector3, Color, Object }

                                                          
                                                     
[Serializable]
public class LuaValue
{
    public string name;
    public LuaValueType type;

    public float number;
    public bool boolean;
    public string text;
    public Vector3 vector;
    public Color color = Color.white;
    public GameObject target;

                                                                                          
    public object ToLua()
    {
        switch (type)
        {
            case LuaValueType.Number:  return ToDouble(number);
            case LuaValueType.Boolean: return boolean;
            case LuaValueType.String:  return text ?? "";
            case LuaValueType.Vector3: return new Vec3(vector);
            case LuaValueType.Color:   return new Color3(color.r, color.g, color.b);
            case LuaValueType.Object:  return target != null ? LuaInstance.Wrap(target) : null;
        }
        return null;
    }

                                                   
    static double ToDouble(float f)
    {
        if (float.IsNaN(f) || float.IsInfinity(f)) return f;
        return double.Parse(f.ToString("R", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
    }
}