using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.SceneManagement;
[MoonSharpUserData]
public sealed class LuaUnityApi
{
    readonly GameObject owner;
    readonly LuaRunService runService;
    readonly bool audioEnabled, animationEnabled, sceneLoadingEnabled;
    [MoonSharpHidden]
    public LuaUnityApi(GameObject owner, LuaRunService runService, bool audioEnabled, bool animationEnabled, bool sceneLoadingEnabled)
    {
        this.owner = owner;
        this.runService = runService;
        this.audioEnabled = audioEnabled;
        this.animationEnabled = animationEnabled;
        this.sceneLoadingEnabled = sceneLoadingEnabled;
    }

    public LuaInstance Current { get { return LuaInstance.Wrap(owner); } }
    public LuaScene Scene
    {
        get
        {
            if (owner != null && owner.scene.IsValid() && owner.scene.isLoaded)
                return LuaScene.Get(owner.scene.name);
            Scene active = SceneManager.GetActiveScene();
            return active.IsValid() && active.isLoaded ? LuaScene.Get(active.name) : null;
        }
    }
    public Signal Update { get { return runService.Update; } }
    public Signal FixedUpdate { get { return runService.FixedUpdate; } }
    public Signal LateUpdate { get { return runService.LateUpdate; } }

    public LuaInstance Find(string objectName)
    {
        if (string.IsNullOrEmpty(objectName)) return null;
        LuaScene scene = Scene;
        return scene != null ? scene.FindChild(objectName) : null;
    }

    public LuaInstance FindInScene(string objectName, string sceneName)
    {
        if (string.IsNullOrEmpty(objectName) || string.IsNullOrEmpty(sceneName)) return null;
        Scene target = SceneManager.GetSceneByName(sceneName);
        if (!target.IsValid() || !target.isLoaded) return null;
        return LuaScene.Get(target.name).FindChild(objectName);
    }

    public LuaInstance Create(string unityType) { return Create(unityType, null); }

    public LuaInstance Create(string unityType, string objectName)
    {
        LuaScene scene = Scene;
        object created = LuaFactory.Create(unityType, scene);
        LuaInstance instance = created as LuaInstance;
        if (instance == null)
            throw new ScriptRuntimeException("Unity.Create currently creates Unity GameObjects; use the UI examples for UI elements.");
        if (!string.IsNullOrEmpty(objectName)) instance.Name = objectName;
        LuaWorld.Instance.Scheduler.TrackObject(instance.Go);
        return instance;
    }

    public LuaInstance Instantiate(LuaInstance original)
    {
        if (original == null) throw new ScriptRuntimeException("Unity.Instantiate needs a GameObject.");
        return original.Clone();
    }

    public void Log(string message) { Debug.Log(message, owner); }
    public void Warn(string message) { Debug.LogWarning(message, owner); }
    public void PlayAudio(string resourcePath, bool loop = false, float volume = 1f)
    {
        Require(audioEnabled, "Audio");
        AudioClip clip = Resources.Load<AudioClip>(resourcePath);
        if (clip == null) throw new ScriptRuntimeException("Audio clip not found in Resources: " + resourcePath);
        AudioSource source = owner.GetComponent<AudioSource>();
        if (source == null) source = owner.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = loop;
        source.volume = Mathf.Clamp01(volume);
        source.Play();
    }
    public void StopAudio()
    {
        Require(audioEnabled, "Audio");
        AudioSource source = owner.GetComponent<AudioSource>();
        if (source != null) source.Stop();
    }
    public void SetAnimationTrigger(string trigger)
    {
        Require(animationEnabled, "Animation");
        Animator animator = owner.GetComponent<Animator>();
        if (animator == null) throw new ScriptRuntimeException("This GameObject has no Animator component.");
        animator.SetTrigger(trigger);
    }
    public void SetAnimationFloat(string parameter, float value)
    {
        Require(animationEnabled, "Animation");
        Animator animator = owner.GetComponent<Animator>();
        if (animator == null) throw new ScriptRuntimeException("This GameObject has no Animator component.");
        animator.SetFloat(parameter, value);
    }
    public void LoadScene(string sceneName)
    {
        Require(sceneLoadingEnabled, "Scene loading");
        if (string.IsNullOrEmpty(sceneName)) throw new ScriptRuntimeException("Scene name cannot be empty.");
        SceneManager.LoadScene(sceneName);
    }
    public void LoadSceneAsync(string sceneName)
    {
        Require(sceneLoadingEnabled, "Scene loading");
        if (string.IsNullOrEmpty(sceneName)) throw new ScriptRuntimeException("Scene name cannot be empty.");
        SceneManager.LoadSceneAsync(sceneName);
    }
    static void Require(bool allowed, string capability)
    {
        if (!allowed) throw new ScriptRuntimeException(capability + " access is disabled for this Lua Behaviour in the Inspector.");
    }
}
