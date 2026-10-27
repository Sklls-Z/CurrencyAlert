using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using KamiToolKit.Nodes.Simplified;
using Newtonsoft.Json.Linq;

namespace CurrencyAlert.Classes;

public class CurrencyWarningNode : SimpleComponentNode {
    private readonly ColorImageNode background;
    private readonly TextNode warningText;
    private readonly CurrencyIconNode currencyIcon;

    private static string CurrencyNodeConfigPath => Path.Combine(Service.PluginInterface.ConfigDirectory.FullName, "CurrencyNode.style.json");

    public CurrencyWarningNode() {
        background = new ColorImageNode {
            NodeId = 2,
            Position = new Vector2(-5.0f, -5.0f),
            Size = new Vector2(610.0f, 42.0f),
            Color = KnownColor.Black.Vector() with { W = 0.30f },
            WrapMode = WrapMode.Tile,
            IsVisible = false,
        };
        background.AttachNode(this);

        currencyIcon = new CurrencyIconNode {
            NodeId = 3,
            Position = new Vector2(5.0f, 0.0f),
            Size = new Vector2(32.0f, 32.0f),
            IsVisible = true,
        };
        currencyIcon.AttachNode(this);

        warningText = new TextNode {
            NodeId = 4,
            FontSize = 24,
            FontType = FontType.Axis,
            Size = new Vector2(600.0f, 32.0f),
            Position = new Vector2(42.0f, 0.0f),
            TextColor = KnownColor.White.Vector(),
            TextFlags = TextFlags.AutoAdjustNodeSize | TextFlags.Edge,
            TextOutlineColor = KnownColor.Black.Vector(),
            IsVisible = true,
        };
        warningText.AttachNode(this);
    }

    public TrackedCurrency? Currency {
        get;
        set {
            field = value;

            if (value is null) {
                IsVisible = false;
            }
            else {
                UpdateFromCurrency();
            }
        }
    }

    public void UpdateFromCurrency() {
        if (Currency is null) return;

        currencyIcon.IconId = Currency.IconId;
        warningText.String = Currency.ShowItemName ? $"{Currency.Name} {Currency.WarningText}" : $"{Currency.WarningText}";
        currencyIcon.ItemCount = Currency.CurrentCount;
        IsVisible = true;

        RecalculateLayout();
    }

    public bool ShowIcon {
        get => currencyIcon.IsVisible;
        set => currencyIcon.IsVisible = value;
    }

    public bool ShowText {
        get => warningText.IsVisible;
        set => warningText.IsVisible = value;
    }

    public int TextSize {
        get => (int) warningText.FontSize;
        set => warningText.FontSize = (uint) value;
    }

    public Vector4 TextColor {
        get => warningText.TextColor;
        set => warningText.TextColor = value;
    }

    public bool ShowItemCount {
        get => currencyIcon.ShowItemCount;
        set => currencyIcon.ShowItemCount = value;
    }

    public FontType LabelFont {
        get => warningText.FontType;
        set => warningText.FontType = value;
    }

    public void RecalculateLayout() {
        var currentPosition = 0.0f;

        if (currencyIcon.IsVisible) {
            currencyIcon.X = currentPosition;
            currentPosition += currencyIcon.LayoutWidth;
        }

        if (warningText.IsVisible) {
            warningText.X = currentPosition;
            currentPosition += warningText.Width;
        }

        background.Width = currentPosition + 10.0f;
        RecalculateSize();
    }

    private void RecalculateSize() {
        var width = 0.0f;

        if (ShowIcon) {
            width += currencyIcon.LayoutWidth;
        }

        if (ShowText) {
            width += warningText.Width;
        }

        Size = ShowText ? new Vector2(width, MathF.Max(currencyIcon.Height, warningText.Height)) : new Vector2(width, currencyIcon.Height);
        currencyIcon.Y = Height / 2.0f - currencyIcon.Height / 2.0f;
        warningText.Y = Height / 2.0f - warningText.Height / 2.0f;
    }

    public void Load()
        => Load(CurrencyNodeConfigPath);

    public void Load(string filePath) {
        if (!File.Exists(filePath)) return;

        var json = JObject.Parse(File.ReadAllText(filePath));

        if (json["ShowIcon"]?.ToObject<bool>() is { } showIcon) {
            ShowIcon = showIcon;
        }

        if (json["ShowText"]?.ToObject<bool>() is { } showText) {
            ShowText = showText;
        }

        if (json["TextSize"]?.ToObject<int>() is { } textSize) {
            TextSize = textSize;
        }

        if (json["TextColor"]?.ToObject<Vector4>() is { } textColor) {
            TextColor = textColor;
        }

        if (json["ShowItemCount"]?.ToObject<bool>() is { } showItemCount) {
            ShowItemCount = showItemCount;
        }

        if (json["LabelFont"]?.ToObject<FontType>() is { } labelFont) {
            LabelFont = labelFont;
        }
    }

    public void Load(CurrencyWarningNode other) {
        ShowIcon = other.ShowIcon;
        ShowText = other.ShowText;
        TextSize = other.TextSize;
        TextColor = other.TextColor;
        ShowItemCount = other.ShowItemCount;
        LabelFont = other.LabelFont;
    }

    public void Save()
        => Save(CurrencyNodeConfigPath);

    public void Save(string filePath) {
        var json = new JObject {
            ["ShowIcon"] = ShowIcon,
            ["ShowText"] = ShowText,
            ["TextSize"] = TextSize,
            ["TextColor"] = JToken.FromObject(TextColor),
            ["ShowItemCount"] = ShowItemCount,
            ["LabelFont"] = JToken.FromObject(LabelFont),
        };

        File.WriteAllText(filePath, json.ToString());
    }
}
