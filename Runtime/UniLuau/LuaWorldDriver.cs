using UnityEngine;
public class LuaWorldDriver : MonoBehaviour
{
    [System.NonSerialized] public LuaWorld world;
    void Update() { world.Tick(Time.deltaTime); }
}