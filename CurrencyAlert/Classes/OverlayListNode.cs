using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiLib.Extensions;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using KamiToolKit.UiOverlay;
using Newtonsoft.Json.Linq;

namespace CurrencyAlert.Classes;

public class OverlayListNode : OverlayNode {
    private readonly ListBoxNode listNode;
    private bool listEventsEnabled;

    public readonly List<CurrencyWarningNode> NodeList = [];

    public override OverlayLayer OverlayLayer => OverlayLayer.BehindUserInterface;

    public OverlayListNode() {
        listNode = new ListBoxNode {
            NodeId = 4,
            LayoutAnchor = LayoutAnchor.TopLeft,
            IsVisible = true,
            FirstItemSpacing = 10.0f,
            ItemSpacing = 10.0f,
            ClipListContents = true,
            TextTooltip = "Overlay from CurrencyAlert plugin",
        };

        listNode.AttachNode(this);
        EnableListEvents = !System.Config.DisableInteraction;
    }

    protected override void OnUpdate() {
        IsVisible = System.Config.OverlayEnabled && !(Service.Condition.IsBoundByDuty() && System.Config.HideInDuties);
        if (!IsVisible) return;

        var activeWarnings = System.Config.Currencies
            .Where(currency => currency is { HasWarning: true, Enabled: true, ShowInOverlay: true })
            .ToList();

        UpdateWarnings(activeWarnings);
    }

    public override float Width {
        get => base.Width;
        set {
            base.Width = value;
            listNode.Width = value;
        }
    }

    public override float Height {
        get => base.Height;
        set {
            base.Height = value;
            listNode.Height = value;
        }
    }

    public Vector4 BackgroundColor {
        get => listNode.BackgroundColor;
        set => listNode.BackgroundColor = value;
    }

    public LayoutOrientation LayoutOrientation {
        get => listNode.LayoutOrientation;
        set => listNode.LayoutOrientation = value;
    }

    public float ItemSpacing {
        get => listNode.ItemSpacing;
        set => listNode.ItemSpacing = value;
    }

    public bool ShowBackground {
        get => listNode.ShowBackground;
        set => listNode.ShowBackground = value;
    }

    public bool ShowBorder {
        get => listNode.ShowBorder;
        set => listNode.ShowBorder = value;
    }

    public LayoutAnchor LayoutAnchor {
        get => listNode.LayoutAnchor;
        set => listNode.LayoutAnchor = value;
    }

    public float FirstItemSpacing {
        get => listNode.FirstItemSpacing;
        set => listNode.FirstItemSpacing = value;
    }

    public bool EnableListEvents {
        get => listEventsEnabled;
        set {
            listEventsEnabled = value;
            UpdateListEvents();
        }
    }

    public void UpdateWarnings(List<TrackedCurrency> activeWarnings) {
        var oldWarnings = NodeList.Where(node => activeWarnings.All(warning => warning != node.Currency)).ToList();

        foreach (var oldWarning in oldWarnings) {
            listNode.RemoveNode(oldWarning);
            NodeList.Remove(oldWarning);
            oldWarning.Dispose();
        }

        var newWarnings = activeWarnings.Where(warning => NodeList.All(node => node.Currency != warning)).ToList();

        foreach (var newWarning in newWarnings) {
            var newWarningNode = new CurrencyWarningNode {
                Height = 32.0f,
                Currency = newWarning,
                IsVisible = true,
                TextTooltip = "Overlay from CurrencyAlert plugin",
            };

            newWarningNode.Load();
            newWarningNode.AddEvent(AtkEventType.MouseClick, OpenConfigurationWindow);

            listNode.AddNode(newWarningNode);
            NodeList.Add(newWarningNode);
        }

        foreach (var node in NodeList) {
            node.UpdateFromCurrency();
            node.RecalculateLayout();
        }

        UpdateListEvents();
        listNode.RecalculateLayout();
    }

    private void UpdateListEvents() {
        listNode.RemoveEvent(AtkEventType.MouseClick);

        if (NodeList.Count is 0 && listEventsEnabled && !System.Config.DisableInteraction) {
            listNode.AddEvent(AtkEventType.MouseClick, OpenConfigurationWindow);
        }
    }

    private void OpenConfigurationWindow()
        => System.ConfigurationWindow.UnCollapseOrToggle();

    public void Load(string filePath) {
        if (!File.Exists(filePath)) return;

        var json = JObject.Parse(File.ReadAllText(filePath));
        ApplyVector(json["Position"], value => Position = value);
        ApplyVector(json["Size"], value => Size = value);
        ApplyVector(json["Scale"], value => Scale = value);

        if (json["listNode"] is not JObject list) return;

        if (list["BackgroundColor"]?.ToObject<Vector4>() is { } backgroundColor) {
            BackgroundColor = backgroundColor;
        }

        if (list["LayoutOrientation"]?.ToObject<LayoutOrientation>() is { } orientation) {
            LayoutOrientation = orientation;
        }

        if (list["LayoutAnchor"]?.ToObject<LayoutAnchor>() is { } anchor) {
            LayoutAnchor = anchor;
        }

        if (list["FirstItemSpacing"]?.ToObject<float>() is { } firstItemSpacing) {
            FirstItemSpacing = firstItemSpacing;
        }

        if (list["ItemSpacing"]?.ToObject<float>() is { } itemSpacing) {
            ItemSpacing = itemSpacing;
        }

        if (list["ShowBackground"]?.ToObject<bool>() is { } showBackground) {
            ShowBackground = showBackground;
        }

        if (list["ShowBorder"]?.ToObject<bool>() is { } showBorder) {
            ShowBorder = showBorder;
        }

        if (list["ClipListContents"]?.ToObject<bool>() is { } clipListContents) {
            listNode.ClipListContents = clipListContents;
        }
    }

    public void Save(string filePath) {
        var json = new JObject {
            ["Position"] = JToken.FromObject(Position),
            ["Size"] = JToken.FromObject(Size),
            ["Scale"] = JToken.FromObject(Scale),
            ["listNode"] = new JObject {
                ["BackgroundColor"] = JToken.FromObject(BackgroundColor),
                ["LayoutOrientation"] = JToken.FromObject(LayoutOrientation),
                ["LayoutAnchor"] = JToken.FromObject(LayoutAnchor),
                ["FirstItemSpacing"] = FirstItemSpacing,
                ["ItemSpacing"] = ItemSpacing,
                ["ShowBackground"] = ShowBackground,
                ["ShowBorder"] = ShowBorder,
                ["ClipListContents"] = listNode.ClipListContents,
            },
        };

        File.WriteAllText(filePath, json.ToString());
    }

    private static void ApplyVector(JToken? token, Action<Vector2> setter) {
        if (token?.ToObject<Vector2>() is { } value) {
            setter(value);
        }
    }
}
