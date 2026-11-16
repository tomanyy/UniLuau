using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.SceneManagement;
[MoonSharpUserData]
public sealed class LuaUnityApi
{
    readonly GameObject owner;
    readonly bool audioEnabled, animationEnabled, sceneLoadingEnabled;
    [MoonSharpHidden]
    public LuaUnityApi(GameObject owner, bool audioEnabled, bool animationEnabled, bool sceneLoadingEnabled)
    {
        this.owner = owner;
        this.audioEnabled = audioEnabled;
        this.animationEnabled = animationEnabled;
        this.sceneLoadingEnabled = sceneLoadingEnabled;
    }
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
