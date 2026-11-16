using UnityEngine;

                                                                                          
public class LuaTouchRelay : MonoBehaviour
{
    [System.NonSerialized] public Signal signal;
    [System.NonSerialized] public Signal endedSignal;

    void OnCollisionEnter(Collision c) { if (signal != null) signal.Fire(LuaInstance.Wrap(c.gameObject)); }
    void OnTriggerEnter(Collider c)    { if (signal != null) signal.Fire(LuaInstance.Wrap(c.gameObject)); }

    void OnCollisionExit(Collision c)  { if (endedSignal != null) endedSignal.Fire(LuaInstance.Wrap(c.gameObject)); }
    void OnTriggerExit(Collider c)     { if (endedSignal != null) endedSignal.Fire(LuaInstance.Wrap(c.gameObject)); }
}
