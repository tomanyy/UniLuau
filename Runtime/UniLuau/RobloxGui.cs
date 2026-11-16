using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MoonSharp.Interpreter;

                                                               
                                                           
                                                              
  
                                                                
                                                                      
                                                                    
                                                               

[MoonSharpUserData]
public class UDim2
{
    public float XScale, XOffset, YScale, YOffset;

    public UDim2(float xScale, float xOffset, float yScale, float yOffset)
    {
        XScale = xScale; XOffset = xOffset; YScale = yScale; YOffset = yOffset;
    }

    public static UDim2 operator +(UDim2 a, UDim2 b)
    {
        return new UDim2(a.XScale + b.XScale, a.XOffset + b.XOffset, a.YScale + b.YScale, a.YOffset + b.YOffset);
    }

    public static UDim2 operator -(UDim2 a, UDim2 b)
    {
        return new UDim2(a.XScale - b.XScale, a.XOffset - b.XOffset, a.YScale - b.YScale, a.YOffset - b.YOffset);
    }

    public static UDim2 operator *(UDim2 a, float s)
    {
        return new UDim2(a.XScale * s, a.XOffset * s, a.YScale * s, a.YOffset * s);
    }

    public static UDim2 operator *(float s, UDim2 a) { return a * s; }

    public UDim2 Lerp(UDim2 other, float t)
    {
        return new UDim2(
            Mathf.Lerp(XScale, other.XScale, t), Mathf.Lerp(XOffset, other.XOffset, t),
            Mathf.Lerp(YScale, other.YScale, t), Mathf.Lerp(YOffset, other.YOffset, t));
    }

    public override string ToString()
    {
        return "{" + XScale + ", " + XOffset + "}, {" + YScale + ", " + YOffset + "}";
    }
}

                                                                          
[MoonSharpUserData]
public class LuaGuiRoot { }

                                                                 
public class LuaGuiPointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler
{
    [NonSerialized] public LuaGuiObject Owner;

    public void OnPointerEnter(PointerEventData eventData) { if (Owner != null) Owner.SetHover(true); }
    public void OnPointerExit(PointerEventData eventData)  { if (Owner != null) Owner.SetHover(false); }
    public void OnPointerClick(PointerEventData eventData) { if (Owner != null) Owner.PointerClick(); }
    public void OnSelect(BaseEventData eventData)          { if (Owner != null) Owner.SetFocus(true); }
    public void OnDeselect(BaseEventData eventData)        { if (Owner != null) Owner.SetFocus(false); }
}

[MoonSharpUserData]
public class LuaGuiObject
{
    static int counter;

    readonly string className;
    readonly int order = counter++;
    readonly List<LuaGuiObject> children = new List<LuaGuiObject>();
    readonly List<LuaGuiLayout> layoutChildren = new List<LuaGuiLayout>();
    readonly List<Action> nativeClickListeners = new List<Action>();
    readonly GameObject go;
    readonly RectTransform rect;
    readonly RectTransform contentRoot;
    readonly Image background;
    readonly Image imageContent;
    readonly Text textContent;
    readonly Button button;
    readonly InputField input;
    readonly LuaGuiPointer pointer;
    readonly ScrollRect scrollRect;
    readonly RectTransform scrollContent;

    object parent;
    bool destroyed;
    bool updatingInput;

    string name;
    UDim2 position = new UDim2(0, 0, 0, 0);
    UDim2 size = new UDim2(0, 100, 0, 100);
    Vec2 anchorPoint = new Vec2(0, 0);
    Color3 backgroundColor3 = new Color3(1, 1, 1);
    float backgroundTransparency = 0f;
    bool visible = true;
    int zIndex = 1;

    string text = "";
    Color3 textColor3 = new Color3(0, 0, 0);
    int textSize = 18;
    string textXAlignment = "Center";

    string image = "";
    Color3 imageColor3 = new Color3(1, 1, 1);

    bool active;
    bool enabled = true;
    bool hovering;
    bool focused;
    UDim2 canvasSize = new UDim2(1, 0, 1, 0);

    public Signal MouseButton1Click, MouseEnter, MouseLeave;
    public Signal FocusGained, FocusLost, TextChanged;

    Signal changed;
    readonly Dictionary<string, Signal> changedSignals = new Dictionary<string, Signal>();

    [MoonSharpHidden] public bool Destroyed { get { return destroyed; } }
    [MoonSharpHidden] public bool IsButton { get { return className == "TextButton" || className == "ImageButton"; } }
    [MoonSharpHidden] public bool IsText { get { return className == "TextLabel" || className == "TextButton" || className == "TextBox"; } }
    [MoonSharpHidden] public int Order { get { return order; } }
    [MoonSharpHidden] public List<LuaGuiObject> Children { get { return children; } }
    [MoonSharpHidden] public List<LuaGuiLayout> LayoutChildren { get { return layoutChildren; } }
    [MoonSharpHidden] public RectTransform RectTransform { get { return rect; } }
    [MoonSharpHidden] public RectTransform ContentRoot { get { return contentRoot; } }

    [MoonSharpHidden]
    public LuaGuiObject(string cls)
    {
        className = cls;
        name = cls;

        var scheduler = LuaWorld.Instance.Scheduler;
        MouseButton1Click = new Signal(scheduler);
        MouseEnter = new Signal(scheduler);
        MouseLeave = new Signal(scheduler);
        FocusGained = new Signal(scheduler);
        FocusLost = new Signal(scheduler);
        TextChanged = new Signal(scheduler);

        active = cls == "TextButton" || cls == "ImageButton" || cls == "TextBox" || cls == "ScrollingFrame";
        if (cls == "ImageLabel" || cls == "ImageButton") backgroundTransparency = 1f;

        go = LuaPrefabObjects.Create(cls, true);
        rect = go.GetComponent<RectTransform>();
        if (rect == null)
        {
            Debug.LogError("Lua UI prefab '" + cls + "' must have a RectTransform component.");
            UnityEngine.Object.Destroy(go);
            go = new GameObject(cls, typeof(RectTransform), typeof(CanvasRenderer));
            rect = go.GetComponent<RectTransform>();
            go.SetActive(false);
        }
        if (cls == "ScrollingFrame")
        {
            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
            viewportGo.transform.SetParent(go.transform, false);
            var viewport = viewportGo.GetComponent<RectTransform>();
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            var viewportImage = viewportGo.GetComponent<Image>();
            viewportImage.color = new Color(1, 1, 1, 0);
            viewportImage.raycastTarget = true;
            var mask = viewportGo.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewport, false);
            scrollContent = contentGo.GetComponent<RectTransform>();
            scrollContent.anchorMin = new Vector2(0, 1);
            scrollContent.anchorMax = new Vector2(0, 1);
            scrollContent.pivot = new Vector2(0, 1);
            scrollContent.anchoredPosition = Vector2.zero;

            scrollRect = go.AddComponent<ScrollRect>();
            scrollRect.viewport = viewport;
            scrollRect.content = scrollContent;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            contentRoot = scrollContent;
        }
        else
        {
            contentRoot = rect;
        }

        background = go.GetComponent<Image>();
        if (background == null) background = go.AddComponent<Image>();
        background.sprite = LuaGui.WhiteSprite;
        background.color = Color.white;
        background.raycastTarget = active;
        if (cls == "ScreenGui")
        {
            background.enabled = false;
            background.raycastTarget = false;
        }

        if (IsText)
        {
            var textGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer));
            textGo.transform.SetParent(go.transform, false);
            var tr = textGo.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;
            tr.SetAsFirstSibling();

            textContent = textGo.AddComponent<Text>();
            textContent.font = LuaGui.DefaultFont;
            textContent.fontSize = textSize;
            textContent.alignment = TextAnchor.MiddleCenter;
            textContent.raycastTarget = false;
            textContent.horizontalOverflow = HorizontalWrapMode.Wrap;
            textContent.verticalOverflow = VerticalWrapMode.Truncate;
            textContent.text = "";

            if (cls == "TextLabel") text = "Label";
            else if (cls == "TextButton") text = "Button";
        }

        if (cls == "ImageLabel" || cls == "ImageButton")
        {
            var imageGo = new GameObject("Image", typeof(RectTransform), typeof(CanvasRenderer));
            imageGo.transform.SetParent(go.transform, false);
            var ir = imageGo.GetComponent<RectTransform>();
            ir.anchorMin = Vector2.zero;
            ir.anchorMax = Vector2.one;
            ir.offsetMin = Vector2.zero;
            ir.offsetMax = Vector2.zero;
            ir.SetAsFirstSibling();

            imageContent = imageGo.AddComponent<Image>();
            imageContent.raycastTarget = false;
            imageContent.enabled = false;
        }

        if (IsButton)
        {
            button = go.AddComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.None;
            button.interactable = active;
            button.onClick.AddListener(HandleClick);
        }

        if (cls == "TextBox")
        {
            input = go.AddComponent<InputField>();
            input.targetGraphic = background;
            input.textComponent = textContent;
            input.transition = Selectable.Transition.None;
            input.lineType = InputField.LineType.SingleLine;
            input.text = "";
            input.onValueChanged.AddListener(s =>
            {
                if (updatingInput) return;
                text = s ?? "";
                if (textContent != null) textContent.text = text;
                TextChanged.Fire(text);
            });
            input.onEndEdit.AddListener(s =>
            {
                if (focused) SetFocus(false);
            });
        }

        if (cls != "ScreenGui")
        {
            pointer = go.AddComponent<LuaGuiPointer>();
            pointer.Owner = this;
        }

        ApplyBackground();
        ApplyText();
        ApplyVisibility();

        scheduler.TrackCleanup(Destroy);                                                   
    }

    public string ClassName { get { return className; } }

    public string Name
    {
        get { return name; }
        set
        {
            name = value ?? "";
            if (go != null) go.name = name;
            RaiseChanged("Name");
        }
    }

    public UDim2 Position
    {
        get { return position; }
        set { position = value; ApplyLayout(); RaiseChanged("Position"); }
    }

    public UDim2 Size
    {
        get { return size; }
        set { size = value; ApplyLayout(); RaiseChanged("Size"); }
    }

    public Vec2 AnchorPoint
    {
        get { return anchorPoint; }
        set { anchorPoint = value ?? new Vec2(0, 0); ApplyLayout(); RaiseChanged("AnchorPoint"); }
    }

    public UDim2 CanvasSize
    {
        get { return canvasSize; }
        set { canvasSize = value ?? new UDim2(1, 0, 1, 0); ApplyCanvasSize(); RaiseChanged("CanvasSize"); }
    }

    public Vec2 CanvasPosition
    {
        get { return scrollContent != null ? new Vec2(-scrollContent.anchoredPosition.x, scrollContent.anchoredPosition.y) : new Vec2(0, 0); }
        set
        {
            if (scrollContent != null && value != null)
                scrollContent.anchoredPosition = new Vector2(-value.X, value.Y);
            RaiseChanged("CanvasPosition");
        }
    }

    public bool ScrollingEnabled
    {
        get { return scrollRect == null || scrollRect.enabled; }
        set { if (scrollRect != null) scrollRect.enabled = value; RaiseChanged("ScrollingEnabled"); }
    }

    public bool HorizontalScrolling
    {
        get { return scrollRect != null && scrollRect.horizontal; }
        set { if (scrollRect != null) scrollRect.horizontal = value; RaiseChanged("HorizontalScrolling"); }
    }

    public bool VerticalScrolling
    {
        get { return scrollRect == null || scrollRect.vertical; }
        set { if (scrollRect != null) scrollRect.vertical = value; RaiseChanged("VerticalScrolling"); }
    }

    public Color3 BackgroundColor3
    {
        get { return backgroundColor3; }
        set { backgroundColor3 = value ?? new Color3(1, 1, 1); ApplyBackground(); RaiseChanged("BackgroundColor3"); }
    }

    public float BackgroundTransparency
    {
        get { return backgroundTransparency; }
        set { backgroundTransparency = Mathf.Clamp01(value); ApplyBackground(); RaiseChanged("BackgroundTransparency"); }
    }

    public bool Visible
    {
        get { return visible; }
        set { visible = value; ApplyVisibility(); RaiseChanged("Visible"); }
    }

    public int ZIndex
    {
        get { return zIndex; }
        set { zIndex = value; RaiseChanged("ZIndex"); }
    }

                                       
    public string Text
    {
        get { return text; }
        set
        {
            text = value ?? "";
            ApplyText();
            RaiseChanged("Text");
        }
    }

    public Color3 TextColor3
    {
        get { return textColor3; }
        set { textColor3 = value ?? new Color3(0, 0, 0); ApplyText(); RaiseChanged("TextColor3"); }
    }

    public int TextSize
    {
        get { return textSize; }
        set { textSize = Mathf.Max(1, value); ApplyText(); RaiseChanged("TextSize"); }
    }

    public string TextXAlignment
    {
        get { return textXAlignment; }
        set { textXAlignment = value ?? "Center"; ApplyText(); RaiseChanged("TextXAlignment"); }
    }

                               
    public string Image
    {
        get { return image; }
        set { image = value ?? ""; ApplyImage(); RaiseChanged("Image"); }
    }

    public Color3 ImageColor3
    {
        get { return imageColor3; }
        set { imageColor3 = value ?? new Color3(1, 1, 1); ApplyImage(); RaiseChanged("ImageColor3"); }
    }

                                                                                                        
    public bool Active
    {
        get { return active; }
        set
        {
            active = value;
            ApplyBackground();
            ApplyVisibility();
            if (button != null) button.interactable = active;
            if (!active && hovering)
            {
                hovering = false;
                MouseLeave.Fire();
            }
            RaiseChanged("Active");
        }
    }

    public bool Enabled
    {
        get { return enabled; }
        set { enabled = value; ApplyVisibility(); RaiseChanged("Enabled"); }
    }

                   
    public bool TextEditable
    {
        get { return input == null || input.interactable; }
        set { if (input != null) input.interactable = value; RaiseChanged("TextEditable"); }
    }

    public Signal Changed { get { if (changed == null) changed = NewSignal(); return changed; } }

    public Signal GetPropertyChangedSignal(string property)
    {
        Signal s;
        if (!changedSignals.TryGetValue(property, out s))
        {
            s = NewSignal();
            changedSignals[property] = s;
        }
        return s;
    }

    static Signal NewSignal() { return new Signal(LuaWorld.Instance.Scheduler); }

    void RaiseChanged(string property)
    {
        if (destroyed) return;
        if (changed != null) changed.Fire(property);
        Signal s;
        if (changedSignals.TryGetValue(property, out s)) s.Fire();
    }

                                                                         
    public Vec2 AbsoluteSize
    {
        get { return new Vec2(rect != null ? rect.rect.width : 0f, rect != null ? rect.rect.height : 0f); }
    }

    public Vec2 AbsolutePosition
    {
        get
        {
            if (rect == null) return new Vec2(0, 0);
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, corners[1]);
            return new Vec2(screen.x, Screen.height - screen.y);
        }
    }

                                                                  
    public object Parent
    {
        get { return parent; }
        set
        {
            if (destroyed) return;
            Detach();
            parent = value;

            var po = value as LuaGuiObject;
            if (po != null)
            {
                po.children.Add(this);
                go.transform.SetParent(po.contentRoot, false);
                go.SetActive(true);
            }
            else if (value is LuaGuiRoot && className == "ScreenGui")
            {
                LuaGui.AddScreenGui(this);
                LuaGui.AttachScreenGui(this);
            }
            else
            {
                go.transform.SetParent(null, false);
                go.SetActive(false);
            }

            ApplyVisibility();
        }
    }

    void Detach()
    {
        var po = parent as LuaGuiObject;
        if (po != null) po.children.Remove(this);
        LuaGui.RemoveScreenGui(this);
        parent = null;
    }

    public void Destroy()
    {
        if (destroyed) return;
        destroyed = true;

        Detach();
        foreach (var c in children.ToArray()) c.Destroy();
        foreach (var c in layoutChildren.ToArray()) c.Destroy();
        children.Clear();
        layoutChildren.Clear();

        if (go != null) UnityEngine.Object.Destroy(go);
    }

    public void ClearAllChildren()
    {
        foreach (var c in children.ToArray()) c.Destroy();
        foreach (var c in layoutChildren.ToArray()) c.Destroy();
    }

    public object FindFirstChild(string childName)
    {
        foreach (var c in children) if (c.Name == childName) return c;
        foreach (var c in layoutChildren) if (c.Name == childName || c.ClassName == childName) return c;
        return null;
    }

    public object FindFirstChild(string childName, bool recursive)
    {
        if (!recursive) return FindFirstChild(childName);
        var direct = FindFirstChild(childName);
        if (direct != null) return direct;
        foreach (var c in children)
        {
            var found = c.FindFirstChild(childName, true);
            if (found != null) return found;
        }
        return null;
    }

    public LuaGuiObject FindFirstAncestor(string ancestorName)
    {
        var p = parent as LuaGuiObject;
        while (p != null)
        {
            if (p.Name == ancestorName) return p;
            p = p.parent as LuaGuiObject;
        }
        return null;
    }

    public Table GetChildren()
    {
        var lua = LuaWorld.Instance.Lua;
        var t = new Table(lua);
        foreach (var c in children) t.Append(DynValue.FromObject(lua, c));
        foreach (var c in layoutChildren) t.Append(DynValue.FromObject(lua, c));
        return t;
    }

    public Table GetDescendants()
    {
        var lua = LuaWorld.Instance.Lua;
        var t = new Table(lua);
        AddDescendants(this, t, lua);
        return t;
    }

    static void AddDescendants(LuaGuiObject o, Table t, Script lua)
    {
        foreach (var c in o.children)
        {
            t.Append(DynValue.FromObject(lua, c));
            AddDescendants(c, t, lua);
        }
        foreach (var c in o.layoutChildren) t.Append(DynValue.FromObject(lua, c));
    }

                          

    [MoonSharpHidden]
    public void Tick()
    {
        if (destroyed) return;

        ApplyVisibility();
        ApplyLayout();
        ApplyCanvasSize();
        ApplyZOrder();

        foreach (var layout in layoutChildren.ToArray()) layout.Tick();
        foreach (var c in children.ToArray()) c.Tick();
    }

    void ApplyVisibility()
    {
        if (go == null) return;

        bool attached = parent is LuaGuiObject || (className == "ScreenGui" && parent is LuaGuiRoot);
        bool show = visible && enabled && attached;
        if (go.activeSelf != show) go.SetActive(show);
        if (background != null) background.raycastTarget = active && show;
    }

    void ApplyLayout()
    {
        if (rect == null || destroyed) return;

        if (className == "ScreenGui")
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return;
        }

        var po = parent as LuaGuiObject;
        if (po == null || po.rect == null) return;

        float parentWidth = po.contentRoot.rect.width;
        float parentHeight = po.contentRoot.rect.height;

        float width = size.XScale * parentWidth + size.XOffset;
        float height = size.YScale * parentHeight + size.YOffset;
        float px = position.XScale * parentWidth + position.XOffset;
        float py = position.YScale * parentHeight + position.YOffset;

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(Mathf.Clamp01(anchorPoint.X), 1f - Mathf.Clamp01(anchorPoint.Y));
        rect.sizeDelta = new Vector2(width, height);
        if (HasActiveLayoutGroup(po.contentRoot)) return;
        rect.anchoredPosition = new Vector2(px, -py);
    }

    void ApplyCanvasSize()
    {
        if (scrollContent == null || rect == null) return;
        float width = canvasSize.XScale * rect.rect.width + canvasSize.XOffset;
        float height = canvasSize.YScale * rect.rect.height + canvasSize.YOffset;
        scrollContent.sizeDelta = new Vector2(Mathf.Max(0, width), Mathf.Max(0, height));
    }

    static bool HasActiveLayoutGroup(RectTransform target)
    {
        if (target == null) return false;
        foreach (var group in target.GetComponents<LayoutGroup>())
            if (group.enabled) return true;
        return false;
    }

    void ApplyZOrder()
    {
        var po = parent as LuaGuiObject;
        if (po == null || destroyed) return;

        var sorted = new List<LuaGuiObject>(po.children);
        sorted.Sort((a, b) => a.ZIndex != b.ZIndex ? a.ZIndex.CompareTo(b.ZIndex) : a.Order.CompareTo(b.Order));
        for (int i = 0; i < sorted.Count; i++)
            if (!sorted[i].destroyed) sorted[i].go.transform.SetSiblingIndex(i + 1);
    }

    void ApplyBackground()
    {
        if (background == null) return;
        var c = backgroundColor3 ?? new Color3(1, 1, 1);
        float alpha = 1f - backgroundTransparency;
        background.color = new Color(c.R, c.G, c.B, alpha);
                                                                        
        background.enabled = className != "ScreenGui" && (backgroundTransparency < 1f || active);
    }

    void ApplyText()
    {
        if (textContent == null) return;
        textContent.text = text;
        textContent.fontSize = textSize;
        var c = textColor3 ?? new Color3(0, 0, 0);
        textContent.color = new Color(c.R, c.G, c.B, 1f);
        textContent.alignment = textXAlignment == "Left" ? TextAnchor.MiddleLeft
                                : textXAlignment == "Right" ? TextAnchor.MiddleRight
                                : TextAnchor.MiddleCenter;

        if (input != null && !updatingInput)
        {
            updatingInput = true;
            input.text = text;
            updatingInput = false;
        }
    }

    void ApplyImage()
    {
        if (imageContent == null) return;
        var sprite = LuaGui.LoadImage(image);
        imageContent.sprite = sprite;
        imageContent.enabled = sprite != null;
        var c = imageColor3 ?? new Color3(1, 1, 1);
        imageContent.color = new Color(c.R, c.G, c.B, 1f);
    }

    [MoonSharpHidden]
    public void SetHover(bool over)
    {
        if (destroyed) return;
        if (!active)
        {
            hovering = false;
            return;
        }
        if (hovering == over) return;
        hovering = over;
        if (over) MouseEnter.Fire(); else MouseLeave.Fire();
    }

                                                                   
                                     
    [MoonSharpHidden]
    public void PointerClick()
    {
        if (destroyed || !active || IsButton) return;
        HandleClick();
    }

    [MoonSharpHidden]
    public void AddNativeClickListener(Action listener)
    {
        if (listener != null) nativeClickListeners.Add(listener);
    }

    void HandleClick()
    {
        if (destroyed || !active) return;
        MouseButton1Click.Fire();
        foreach (Action listener in nativeClickListeners.ToArray()) listener();
    }

    [MoonSharpHidden]
    public void SetFocus(bool value)
    {
        if (destroyed) return;
        if (focused == value) return;
        focused = value;
        if (value) FocusGained.Fire(); else FocusLost.Fire();
    }
}

[MoonSharpUserData]
public class LuaGuiLayout
{
    readonly string className;
    string name;
    LuaGuiObject parent;
    LayoutElement element;
    VerticalLayoutGroup vertical;
    HorizontalLayoutGroup horizontal;
    GridLayoutGroup grid;
    bool destroyed;
    int rebuildAfterFrame = -1;

    string fillDirection = "Vertical";
    string childAlignment = "UpperLeft";
    string startCorner = "UpperLeft";
    string startAxis = "Horizontal";
    string constraint = "Flexible";
    float spacing;
    int paddingLeft, paddingRight, paddingTop, paddingBottom;
    bool childControlWidth = true, childControlHeight = true, childForceExpandWidth, childForceExpandHeight;
    Vec2 cellSize = new Vec2(100, 100), cellPadding = new Vec2(0, 0);
    int constraintCount = 2;
    float minWidth = -1, minHeight = -1, preferredWidth = -1, preferredHeight = -1, flexibleWidth = -1, flexibleHeight = -1;
    bool ignoreLayout;

    [MoonSharpHidden]
    public LuaGuiLayout(string cls)
    {
        className = cls;
        name = cls;
        LuaWorld.Instance.Scheduler.TrackCleanup(Destroy);
    }

    public string ClassName { get { return className; } }
    public string Name { get { return name; } set { name = value ?? className; } }

    public object Parent
    {
        get { return parent; }
        set
        {
            var next = value as LuaGuiObject;
            if (parent == next) return;
            RemoveComponents();
            if (parent != null) parent.LayoutChildren.Remove(this);
            parent = next;
            if (parent == null || parent.Destroyed) return;
            parent.LayoutChildren.Add(this);

            if (className == "LayoutElement")
            {
                element = parent.RectTransform.gameObject.AddComponent<LayoutElement>();
            }
            else
            {
                GameObject host = parent.ContentRoot.gameObject;
                if (className == "UIListLayout")
                {
                    AddListLayoutGroup(host);
                }
                else if (className == "UIGridLayout")
                {
                    grid = host.AddComponent<GridLayoutGroup>();
                }
            }
            Apply();
        }
    }

    public string FillDirection
    {
        get { return fillDirection; }
        set
        {
            string next = value == "Horizontal" ? "Horizontal" : "Vertical";
            if (fillDirection == next) return;
            fillDirection = next;
            if (className == "UIListLayout" && parent != null && (vertical != null || horizontal != null))
            {
                if (vertical != null) UnityEngine.Object.Destroy(vertical);
                if (horizontal != null) UnityEngine.Object.Destroy(horizontal);
                vertical = null;
                horizontal = null;
                rebuildAfterFrame = Time.frameCount + 1;
            }
            Apply();
        }
    }
    public string ChildAlignment { get { return childAlignment; } set { childAlignment = value ?? "UpperLeft"; Apply(); } }
    public float Spacing { get { return spacing; } set { spacing = value; Apply(); } }
    public int PaddingLeft { get { return paddingLeft; } set { paddingLeft = value; Apply(); } }
    public int PaddingRight { get { return paddingRight; } set { paddingRight = value; Apply(); } }
    public int PaddingTop { get { return paddingTop; } set { paddingTop = value; Apply(); } }
    public int PaddingBottom { get { return paddingBottom; } set { paddingBottom = value; Apply(); } }
    public bool ChildControlWidth { get { return childControlWidth; } set { childControlWidth = value; Apply(); } }
    public bool ChildControlHeight { get { return childControlHeight; } set { childControlHeight = value; Apply(); } }
    public bool ChildForceExpandWidth { get { return childForceExpandWidth; } set { childForceExpandWidth = value; Apply(); } }
    public bool ChildForceExpandHeight { get { return childForceExpandHeight; } set { childForceExpandHeight = value; Apply(); } }
    public Vec2 CellSize { get { return cellSize; } set { cellSize = value ?? new Vec2(100, 100); Apply(); } }
    public Vec2 CellPadding { get { return cellPadding; } set { cellPadding = value ?? new Vec2(0, 0); Apply(); } }
    public string StartCorner { get { return startCorner; } set { startCorner = value ?? "UpperLeft"; Apply(); } }
    public string StartAxis { get { return startAxis; } set { startAxis = value ?? "Horizontal"; Apply(); } }
    public string Constraint { get { return constraint; } set { constraint = value ?? "Flexible"; Apply(); } }
    public int ConstraintCount { get { return constraintCount; } set { constraintCount = Mathf.Max(1, value); Apply(); } }

    public float MinWidth { get { return element != null ? element.minWidth : minWidth; } set { minWidth = value; Apply(); } }
    public float MinHeight { get { return element != null ? element.minHeight : minHeight; } set { minHeight = value; Apply(); } }
    public float PreferredWidth { get { return element != null ? element.preferredWidth : preferredWidth; } set { preferredWidth = value; Apply(); } }
    public float PreferredHeight { get { return element != null ? element.preferredHeight : preferredHeight; } set { preferredHeight = value; Apply(); } }
    public float FlexibleWidth { get { return element != null ? element.flexibleWidth : flexibleWidth; } set { flexibleWidth = value; Apply(); } }
    public float FlexibleHeight { get { return element != null ? element.flexibleHeight : flexibleHeight; } set { flexibleHeight = value; Apply(); } }
    public bool IgnoreLayout { get { return element != null ? element.ignoreLayout : ignoreLayout; } set { ignoreLayout = value; Apply(); } }

    void AddListLayoutGroup(GameObject host)
    {
        if (fillDirection == "Horizontal") horizontal = host.AddComponent<HorizontalLayoutGroup>();
        else vertical = host.AddComponent<VerticalLayoutGroup>();
    }

    [MoonSharpHidden]
    public void Tick()
    {
        if (destroyed || parent == null || className != "UIListLayout" || vertical != null || horizontal != null) return;
        if (rebuildAfterFrame >= 0 && Time.frameCount < rebuildAfterFrame) return;
        AddListLayoutGroup(parent.ContentRoot.gameObject);
        rebuildAfterFrame = -1;
        Apply();
    }

    void Apply()
    {
        if (destroyed) return;
        if (element != null)
        {
            element.minWidth = minWidth;
            element.minHeight = minHeight;
            element.preferredWidth = preferredWidth;
            element.preferredHeight = preferredHeight;
            element.flexibleWidth = flexibleWidth;
            element.flexibleHeight = flexibleHeight;
            element.ignoreLayout = ignoreLayout;
        }

        var rectOffset = new RectOffset(paddingLeft, paddingRight, paddingTop, paddingBottom);
        TextAnchor alignment;
        if (!Enum.TryParse(childAlignment, true, out alignment)) alignment = TextAnchor.UpperLeft;
        if (vertical != null)
        {
            vertical.enabled = fillDirection != "Horizontal";
            vertical.padding = rectOffset;
            vertical.spacing = spacing;
            vertical.childAlignment = alignment;
            vertical.childControlWidth = childControlWidth;
            vertical.childControlHeight = childControlHeight;
            vertical.childForceExpandWidth = childForceExpandWidth;
            vertical.childForceExpandHeight = childForceExpandHeight;
        }
        if (horizontal != null)
        {
            horizontal.enabled = fillDirection == "Horizontal";
            horizontal.padding = rectOffset;
            horizontal.spacing = spacing;
            horizontal.childAlignment = alignment;
            horizontal.childControlWidth = childControlWidth;
            horizontal.childControlHeight = childControlHeight;
            horizontal.childForceExpandWidth = childForceExpandWidth;
            horizontal.childForceExpandHeight = childForceExpandHeight;
        }
        if (grid != null)
        {
            grid.padding = rectOffset;
            grid.cellSize = cellSize.ToUnity();
            grid.spacing = cellPadding.ToUnity();
            GridLayoutGroup.Corner corner;
            if (Enum.TryParse(startCorner, true, out corner)) grid.startCorner = corner;
            GridLayoutGroup.Axis axis;
            if (Enum.TryParse(startAxis, true, out axis)) grid.startAxis = axis;
            GridLayoutGroup.Constraint gridConstraint;
            if (Enum.TryParse(constraint, true, out gridConstraint)) grid.constraint = gridConstraint;
            grid.constraintCount = constraintCount;
            grid.childAlignment = alignment;
        }
    }

    public void Destroy()
    {
        if (destroyed) return;
        destroyed = true;
        RemoveComponents();
        if (parent != null) parent.LayoutChildren.Remove(this);
        parent = null;
    }

    void RemoveComponents()
    {
        if (element != null) UnityEngine.Object.Destroy(element);
        if (vertical != null) UnityEngine.Object.Destroy(vertical);
        if (horizontal != null) UnityEngine.Object.Destroy(horizontal);
        if (grid != null) UnityEngine.Object.Destroy(grid);
        element = null;
        vertical = null;
        horizontal = null;
        grid = null;
    }
}

public static class LuaGui
{
    public static readonly List<LuaGuiObject> ScreenGuis = new List<LuaGuiObject>();

    static GameObject canvasGo;
    static RectTransform canvasRect;
    static GameObject eventSystemGo;
    static Font font;
    static Sprite whiteSprite;
    static readonly Dictionary<string, Sprite> images = new Dictionary<string, Sprite>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        ScreenGuis.Clear();
        canvasGo = null;
        canvasRect = null;
        eventSystemGo = null;
        font = null;
        whiteSprite = null;
        images.Clear();
    }

    [MoonSharpHidden]
    public static RectTransform CanvasTransform { get { EnsureCanvas(); return canvasRect; } }

    public static Font DefaultFont
    {
        get
        {
            if (font != null) return font;

                                                                          
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 16);
            if (font == null) font = Font.CreateDynamicFontFromOSFont(
                new[] { "Arial", "Liberation Sans", "DejaVu Sans", "Noto Sans" }, 16);
            if (font == null) Debug.LogWarning("[Lua] No UI font found. UI text will not be visible.");
            return font;
        }
    }

    public static Sprite WhiteSprite
    {
        get
        {
            if (whiteSprite != null) return whiteSprite;
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            whiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
            whiteSprite.hideFlags = HideFlags.HideAndDontSave;
            return whiteSprite;
        }
    }

    [MoonSharpHidden]
    public static void EnsureCanvas()
    {
        if (canvasGo != null) return;

        canvasGo = new GameObject("[LuaGuiCanvas]", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        UnityEngine.Object.DontDestroyOnLoad(canvasGo);

        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

                                                                                  
                                                                        
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 1f;
        scaler.referencePixelsPerUnit = 100f;

        canvasRect = canvasGo.GetComponent<RectTransform>();

        if (EventSystem.current == null)
        {
            eventSystemGo = new GameObject("[LuaGuiEventSystem]", typeof(EventSystem));
            UnityEngine.Object.DontDestroyOnLoad(eventSystemGo);
#if ENABLE_INPUT_SYSTEM
            var module = eventSystemGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            module.AssignDefaultActions();
#else
            eventSystemGo.AddComponent<StandaloneInputModule>();
#endif
        }
    }

    [MoonSharpHidden]
    public static void AttachScreenGui(LuaGuiObject gui)
    {
        EnsureCanvas();
        if (gui == null || gui.RectTransform == null) return;
        gui.RectTransform.SetParent(canvasRect, false);
        gui.RectTransform.SetAsLastSibling();
        gui.RectTransform.gameObject.SetActive(gui.Enabled);
    }

    [MoonSharpHidden]
    public static void AddScreenGui(LuaGuiObject gui)
    {
        if (gui != null && !ScreenGuis.Contains(gui)) ScreenGuis.Add(gui);
    }

    [MoonSharpHidden]
    public static void RemoveScreenGui(LuaGuiObject gui)
    {
        ScreenGuis.Remove(gui);
    }

    [MoonSharpHidden]
    public static void Tick()
    {
        if (ScreenGuis.Count > 1)
            ScreenGuis.Sort((a, b) =>
                a.ZIndex != b.ZIndex ? a.ZIndex.CompareTo(b.ZIndex) : a.Order.CompareTo(b.Order));

        for (int i = ScreenGuis.Count - 1; i >= 0; i--)
        {
            var sg = ScreenGuis[i];
            if (sg == null || sg.Destroyed) { ScreenGuis.RemoveAt(i); continue; }
            if (sg.RectTransform != null) sg.RectTransform.SetSiblingIndex(i);
            sg.Tick();
        }
    }

    [MoonSharpHidden]
    public static Sprite LoadImage(string imageName)
    {
        if (string.IsNullOrEmpty(imageName)) return null;

        Sprite sprite;
        if (images.TryGetValue(imageName, out sprite)) return sprite;

        string path = imageName.Contains("/") ? imageName : "Images/" + imageName;
        sprite = Resources.Load<Sprite>(path);
        if (sprite == null)
        {
            var tex = Resources.Load<Texture2D>(path);
            if (tex != null)
            {
                sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                sprite.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        if (sprite == null) Debug.LogWarning("Image not found: Assets/Resources/" + path);
        images[imageName] = sprite;
        return sprite;
    }

                                                                         
                                                                
    [MoonSharpHidden]
    public static bool IsOverGui(Vector2 topLeftPixels)
    {
        if (ScreenGuis.Count == 0) return false;
        Vector2 screenPoint = new Vector2(topLeftPixels.x, Screen.height - topLeftPixels.y);
        foreach (var sg in ScreenGuis)
            if (sg != null && !sg.Destroyed && sg.Enabled && Hit(sg, screenPoint)) return true;
        return false;
    }

    static bool Hit(LuaGuiObject o, Vector2 screenPoint)
    {
        if (o == null || o.Destroyed || !o.Visible || !o.Enabled) return false;
        if (o.Active && o.RectTransform != null &&
            RectTransformUtility.RectangleContainsScreenPoint(o.RectTransform, screenPoint, null))
            return true;

        foreach (var c in o.Children)
            if (Hit(c, screenPoint)) return true;
        return false;
    }

    [MoonSharpHidden]
    public static void Shutdown()
    {
        ScreenGuis.Clear();
        if (canvasGo != null) UnityEngine.Object.Destroy(canvasGo);
        if (eventSystemGo != null) UnityEngine.Object.Destroy(eventSystemGo);
        if (whiteSprite != null) UnityEngine.Object.Destroy(whiteSprite);
        canvasGo = null;
        canvasRect = null;
        eventSystemGo = null;
        whiteSprite = null;
        font = null;
        images.Clear();
    }
}
