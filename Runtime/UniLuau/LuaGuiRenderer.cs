using UnityEngine;

                                                                        
                                                                    
public class LuaGuiRenderer : MonoBehaviour
{
    void Update()
    {
        LuaGui.Tick();
    }

    void OnDestroy()
    {
        LuaGui.Shutdown();
    }
}
