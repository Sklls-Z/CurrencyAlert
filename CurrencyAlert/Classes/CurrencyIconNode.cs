using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using KamiToolKit.Nodes.Simplified;

namespace CurrencyAlert.Classes;

public class CurrencyIconNode : SimpleComponentNode {
    private readonly IconImageNode currencyIcon;
    private readonly TextNode itemCountText;

    public CurrencyIconNode() {
        currencyIcon = new IconImageNode {
            NodeId = 2,
            NodeFlags = NodeFlags.Visible,
            Size = new Vector2(32.0f, 32.0f),
            WrapMode = WrapMode.Tile,
            IsVisible = true,
        };
        currencyIcon.AttachNode(this);

        itemCountText = new TextNode {
            NodeId = 3,
            FontSize = 12,
            FontType = FontType.Axis,
            Size = new Vector2(32.0f, 16.0f),
            Position = new Vector2(0.0f, 16.0f),
            TextColor = KnownColor.White.Vector(),
            TextOutlineColor = KnownColor.Black.Vector(),
            TextFlags = TextFlags.Edge,
            IsVisible = true,
        };
        itemCountText.AttachNode(this);
    }

    public int ItemCount {
        get => int.TryParse(itemCountText.String.ToString(), out var count) ? count : 0;
        set {
            if (itemCountText.IsVisible) {
                itemCountText.SetNumber(value);
            }
        }
    }

    public uint IconId {
        get => currencyIcon.IconId;
        set => currencyIcon.IconId = value;
    }

    public bool ShowItemCount {
        get => itemCountText.IsVisible;
        set => itemCountText.IsVisible = value;
    }

    public float LayoutWidth
        => Math.Max(currencyIcon.Width, itemCountText.Width) + 15.0f;
}
