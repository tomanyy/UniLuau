using UnityEngine;
public class LuaWorldDriver : MonoBehaviour
{
    [System.NonSerialized] public LuaWorld world;
    void Update() { world.Tick(Time.deltaTime); }
    void FixedUpdate() { world.FixedTick(Time.fixedDeltaTime); }
    void LateUpdate() { world.LateTick(Time.deltaTime); }
    void OnApplicationPause(bool paused) { if (world != null && world.ApplicationPaused != null) world.ApplicationPaused.Fire(paused); }
}
